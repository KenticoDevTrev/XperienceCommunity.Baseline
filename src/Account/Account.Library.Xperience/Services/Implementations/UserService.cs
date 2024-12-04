using Account.Models;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.Membership;
using Core.Models;
using CSharpFunctionalExtensions;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using System.Web;
using XperienceCommunity.ChannelSettings.Repositories;

namespace Account.Services.Implementations
{
    public class UserService(
        UserManager<ApplicationUser> userManager,
        IEventLogService eventLogService,
        IInfoProvider<MemberInfo> memberInfoProvider,
        IEmailService emailService,
        IPasswordValidator<ApplicationUser> passwordValidator,
        IChannelCustomSettingsRepository channelCustomSettingsRepository
        ) : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IInfoProvider<MemberInfo> _memberInfoProvider = memberInfoProvider;
        private readonly IEmailService _emailService = emailService;
        private readonly IPasswordValidator<ApplicationUser> _passwordValidator = passwordValidator;
        private readonly IChannelCustomSettingsRepository _channelCustomSettingsRepository = channelCustomSettingsRepository;
        private readonly IEventLogService _eventLogService = eventLogService;

        public async Task<User> CreateUserAsync(User user, string password, bool enabled = false) => (await CreateUser(user, password, enabled)).GetValueOrDefault(user);

        public async Task<Result<User>> CreateUser(User user, string password, bool enabled = false)
        {
            // Create basic user
            var applicationUser = user.ToApplicationUser();

            try {
                var registerResult = await _userManager.CreateAsync(applicationUser, password);
                if (registerResult.Succeeded) {
                    // get user
                    var members = await _memberInfoProvider.Get()
                        .WhereEquals(nameof(MemberInfo.MemberName), user.UserName)
                        .GetEnumerableTypedResultAsync();
                    if (members.FirstOrMaybe().TryGetValue(out var member)) {
                        return member.ToUser();
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

        public async Task SendPasswordResetEmailAsync(User user, string confirmationLink)
        {
            var appUser = await _userManager.FindByIdAsync(user.UserID.ToString());
            if (appUser == null) {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "SendPasswordResetEmailError", eventDescription: $"Could not send password reset for user with ID {user.UserID} was not found in Kentico.");
                return;
            }
            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

            // Creates and sends the confirmation email to the user's address
            await _emailService.SendEmail(new EmailMessage() {
                Recipients = user.Email,
                Subject = "Password Reset Request",
                Body = $"A Password reset request has been generated for your account.  If you have generated this request, you may reset your password by clicking <a href=\"{confirmationLink}?userId={user.UserGUID}&token={HttpUtility.UrlEncode(token)}\">here</a>."
            });
        }

        public async Task<IdentityResult> ResetPasswordFromTokenAsync(User user, string token, string newPassword)
        {
            var appUser = await _userManager.FindByIdAsync(user.UserID.ToString());
            if (appUser == null) {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "ResetPasswordFromTokenError", eventDescription: $"Could not reset password from token for user with ID {user.UserID} was not found in Kentico.");
                return IdentityResult.Failed([new() { Description = $"Could not find user with ID {user.UserID}", Code = "NO_USER" }]);
            }
            return await _userManager.ResetPasswordAsync(appUser, token, newPassword);
        }

        public async Task<bool> ValidateUserPasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user.ToApplicationUser(), password);
        }

        public async Task ResetPasswordAsync(User user, string password)
        {
            await _userManager.RemovePasswordAsync(user.ToApplicationUser());
            await _userManager.AddPasswordAsync(user.ToApplicationUser(), password);
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
            var result = await _passwordValidator.ValidateAsync(_userManager, new ApplicationUser(), password);
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
            var applicationUser = user.ToApplicationUser();

            try {
                var registerResult = await _userManager.CreateAsync(applicationUser);
                if (registerResult.Succeeded) {
                    // get user
                    var members = await _memberInfoProvider.Get()
                        .WhereEquals(nameof(MemberInfo.MemberName), user.UserName)
                        .GetEnumerableTypedResultAsync();
                    if (members.FirstOrMaybe().TryGetValue(out var member)) {
                        return member.ToUser();
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

        public async Task SendVerificationCodeEmailAsync(User user, string token)
        {
            // Creates and sends the confirmation email to the user's address
            await _emailService.SendEmail(new EmailMessage() {
                Recipients = user.Email,
                Subject = "Verification Code",
                Body = $"<p>Hello {user.UserName}!</p><p>When Prompted, enter the code below to finish authenticating:</p> <table align=\"center\" width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><tbody><tr><td width=\"15%\"></td><td width=\"70%\" align=\"center\" bgcolor=\"#f1f3f2\" style=\"color:black;margin-bottom:10px;border-radius:10px\"><p style=\"font-size:xx-large;font-weight:bold;margin:10px 0px\">{token}</p></td></tr></tbody></table>"
            });
        }
    }
}
