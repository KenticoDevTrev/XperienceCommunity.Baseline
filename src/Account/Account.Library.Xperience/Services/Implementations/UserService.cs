using Account.Models;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.Membership;
using Core.Models;
using Core.Services;
using CSharpFunctionalExtensions;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.Web;
using XperienceCommunity.ChannelSettings.Repositories;

namespace Account.Services.Implementations
{
    public class UserService<TUser>(
        UserManager<TUser> userManager,
        IEventLogService eventLogService,
        IEmailService emailService,
        IPasswordValidator<TUser> passwordValidator,
        IChannelCustomSettingsRepository channelCustomSettingsRepository,
        IBaselineUserMapper<TUser> baselineUserMapper,
        IOptions<SystemEmailOptions> systemEmailOptions
        ) : IUserService where TUser : ApplicationUser, new()
    {
        private readonly UserManager<TUser> _userManager = userManager;
        private readonly IEmailService _emailService = emailService;
        private readonly IPasswordValidator<TUser> _passwordValidator = passwordValidator;
        private readonly IChannelCustomSettingsRepository _channelCustomSettingsRepository = channelCustomSettingsRepository;
        private readonly IBaselineUserMapper<TUser> _baselineUserMapper = baselineUserMapper;
        private readonly SystemEmailOptions _systemEmailOptions = systemEmailOptions.Value;
        private readonly IEventLogService _eventLogService = eventLogService;

        public async Task<User> CreateUserAsync(User user, string password, bool enabled = false) => (await CreateUser(user, password, enabled)).GetValueOrDefault(user);

        public async Task<Result<User>> CreateUser(User user, string password, bool enabled = false)
        {
            // Create basic user
            var applicationUser = await _baselineUserMapper.ToApplicationUser(user);

            try {
                var registerResult = await _userManager.CreateAsync(applicationUser, password);
                if (registerResult.Succeeded) {
                    if((await GeUser(applicationUser)).TryGetValue(out var newUser)) {
                        return newUser;
                    }
                    return Result.Failure<User>("<p>Could not retrieve newly created user...</p>");
                } else {
                    return Result.Failure<User>($"<p>Could not create user: </p><ul class='registration-error-list'><li>{string.Join("</li><li>", registerResult.Errors.Select(x => $"{x.Code} - {x.Description}"))}</li></ul>");
                }
            } catch (Exception ex) {
                _eventLogService.LogException("UserService", "CreateUserAsync", ex);
                return Result.Failure<User>($"<p>Could not create user: {ex.Message}</p>");
            }
        }

        public async Task SendRegistrationConfirmationEmailAsync(User user, string confirmationUrl)
        {
            var appUser = await _userManager.FindByNameAsync(user.UserName);
            if (appUser == null) {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "SendRegistrationConfirmationError", eventDescription: $"Could not send a user confirmation email to {user.UserName} as one was not found in Kentico.");
                return;
            }
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

            // Creates and sends the confirmation email to the user's address
            await _emailService.SendEmail(new EmailMessage() {
                From = $"no-reply@{_systemEmailOptions.SendingDomain}",
                Recipients = appUser.Email,
                Subject = "Confirm your new account",
                Body = string.Format($"Please confirm your new account by clicking <a href=\"{confirmationUrl}?userId={user.UserGUID}&token={HttpUtility.UrlEncode(token)}\">here</a>")
            });
        }

        public async Task<IdentityResult> ConfirmRegistrationConfirmationTokenAsync(User user, string token)
        {
            var appUser = await _userManager.FindByIdAsync(user.UserID.ToString());
            if (appUser == null) {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "ConfirmRegistrationConfirmationTokenError", eventDescription: $"Could not confirm token for user with ID {user.UserID} was not found in Kentico.");
                return IdentityResult.Failed([new() { Description = $"Could not find user with ID {user.UserID}", Code = "NO_USER" }]);
            }
            return await _userManager.ConfirmEmailAsync(appUser, token);
        }

        public async Task<string> GetPasswordResetTokenAsync(User user)
        {
            var appUser = await _userManager.FindByIdAsync(user.UserID.ToString());
            if (appUser == null) {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "SendPasswordResetEmailError", eventDescription: $"Could not send password reset for user with ID {user.UserID} was not found in Kentico.");
                return string.Empty;
            }
            return await _userManager.GeneratePasswordResetTokenAsync(appUser);
        }

        public async Task SendPasswordResetEmailAsync(User user, string confirmationLink, string? resetToken = null)
        {
            var appUser = await _userManager.FindByIdAsync(user.UserID.ToString());
            if (appUser == null) {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "SendPasswordResetEmailError", eventDescription: $"Could not send password reset for user with ID {user.UserID} was not found in Kentico.");
                return;
            }
            string token = resetToken ?? await _userManager.GeneratePasswordResetTokenAsync(appUser);

            // Creates and sends the confirmation email to the user's address
            await _emailService.SendEmail(new EmailMessage() {
                From = $"no-reply@{_systemEmailOptions.SendingDomain}",
                Recipients = user.Email,
                Subject = "Password Reset Request",
                Body = $"A Password reset request has been generated for your account.  If you have generated this request, you may reset your password by clicking <a href=\"{confirmationLink}?userId={user.UserGUID}&token={HttpUtility.UrlEncode(token)}\">here</a>."
            });
        }

        public async Task<IdentityResult> ResetPasswordFromTokenAsync(User user, string token, string newPassword)
        {
            var appUser = await _userManager.FindByNameAsync(user.UserName.ToString());
            if (appUser == null) {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "ResetPasswordFromTokenError", eventDescription: $"Could not reset password from token for user with ID {user.UserID} was not found in Kentico.");
                return IdentityResult.Failed([new() { Description = $"Could not find user with ID {user.UserID}", Code = "NO_USER" }]);
            }
            return await _userManager.ResetPasswordAsync(appUser, token, newPassword);
        }

        public async Task<bool> ValidateUserPasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(await _baselineUserMapper.ToApplicationUser(user), password);
        }

        public async Task ResetPasswordAsync(User user, string newPassword, string currentPassword)
        {
            var appUser = await _baselineUserMapper.ToApplicationUser(user);
            await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
        }

        public async Task<bool> ValidatePasswordPolicyAsync(string password)
        {
            // Check for Channel Specific Password policy
            var model = await _channelCustomSettingsRepository.GetSettingsModel<MemberPasswordChannelSettings>();
            if (model.UsePasswordPolicy) {
                var policy = new PasswordPolicySettings(model.UsePasswordPolicy, model.MinLength, model.NumNonAlphanumericChars, model.Regex.AsNullOrWhitespaceMaybe().AsNullableValue(), model.ViolationMessage.AsNullOrWhitespaceMaybe().AsNullableValue());
                return CheckPasswordPolicy(password, policy);
            }

            // Use normal Identity password validator
            var result = await _passwordValidator.ValidateAsync(_userManager, new TUser(), password);
            return result.Succeeded;
        }

        private static bool CheckPasswordPolicy(string password, PasswordPolicySettings passwordSettings)
        {
            // Check minimal length
            if (passwordSettings.MinLength.TryGetValue(out var minLengthVal) && password.Length < minLengthVal) {
                return false;
            }

            // Check number of non alphanum characters
            int counter = 0;
            foreach (char c in password) {
                if (!Char.IsLetterOrDigit(c)) {
                    counter++;
                }
            }

            if (passwordSettings.NumNonAlphanumericChars.TryGetValue(out var nonAlphaNum) && counter < nonAlphaNum) {
                return false;
            }

            if(passwordSettings.UniqueChars.TryGetValue(out var uniqueChars) && password.ToCharArray().Distinct().Count() < uniqueChars) {
                return false;
            }

            // Check regular expression
            if (passwordSettings.Regex.TryGetValue(out var regexVal)) {
                Regex regex = RegexHelper.GetRegex(regexVal);
                if (!regex.IsMatch(password)) {
                    return false;
                }
            }

            return true;
        }

        public async Task CreateExternalUserAsync(User user)
        {
            await CreateExternalUser(user);
            return;
        }

        public async Task<Result<User>> CreateExternalUser(User user)
        {
            // Create basic user
            var applicationUser = await _baselineUserMapper.ToApplicationUser(user);

            try {
                var registerResult = await _userManager.CreateAsync(applicationUser);
                if (registerResult.Succeeded) {
                    // get user
                    if((await GeUser(applicationUser)).TryGetValue(out var newUser)) {
                        return newUser;
                    }
                    return Result.Failure<User>("Could not retrieve newly created user...");
                } else {
                    return Result.Failure<User>("Could not create user: " + registerResult.Errors);
                }
            } catch (Exception ex) {
                _eventLogService.LogException("UserService", "CreateUserAsync", ex);
                return Result.Failure<User>("Could not create user: " + ex.Message); ;
            }
        }

        private async Task<Result<User>> GeUser(TUser user)
        {
            if (user.Id > 0) {
                var foundNewUser = await _userManager.FindByIdAsync(user.Id.ToString());
                if (foundNewUser != null) {
                    return await _baselineUserMapper.ToUser(foundNewUser);
                }
            }
            if (!string.IsNullOrWhiteSpace(user.UserName)) {
                var foundNewUser = await _userManager.FindByNameAsync(user.UserName);
                if (foundNewUser != null) {
                    return await _baselineUserMapper.ToUser(foundNewUser);
                }
            }
            if (!string.IsNullOrWhiteSpace(user.Email)) {
                var foundNewUser = await _userManager.FindByEmailAsync(user.Email);
                if (foundNewUser != null) {
                    return await _baselineUserMapper.ToUser(foundNewUser);
                }
            }
            return Result.Failure<User>("Could not find user");
        }

        public async Task SendVerificationCodeEmailAsync(User user, string token)
        {
            // Creates and sends the confirmation email to the user's address
            await _emailService.SendEmail(new EmailMessage() {
                From = $"no-reply@{_systemEmailOptions.SendingDomain}",
                Recipients = user.Email,
                Subject = "Verification Code",
                Body = $"<p>Hello {user.UserName}!</p><p>When Prompted, enter the code below to finish authenticating:</p> <table align=\"center\" width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><tbody><tr><td width=\"15%\"></td><td width=\"70%\" align=\"center\" bgcolor=\"#f1f3f2\" style=\"color:black;margin-bottom:10px;border-radius:10px\"><p style=\"font-size:xx-large;font-weight:bold;margin:10px 0px\">{token}</p></td></tr></tbody></table>"
            });
        }
    }
}
