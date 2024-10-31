using Account.Repositories;
using CMS.Core;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.Membership;
using Core.Models;
using Core.Repositories;
using CSharpFunctionalExtensions;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using System.Web;

namespace Account.Services.Implementations
{
    public class UserService(
        UserManager<ApplicationUser> userManager,
        IProgressiveCache progressiveCache,
        ISiteRepository siteRepository,
        IEventLogService eventLogService,
        IInfoProvider<MemberInfo> memberInfoProvider,
        IEmailService emailService,
        IAccountSettingsRepository accountSettingsRepository
        ) : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ISiteRepository _siteRepository = siteRepository;
        private readonly IInfoProvider<MemberInfo> _memberInfoProvider = memberInfoProvider;
        private readonly IEmailService _emailService = emailService;
        private readonly IAccountSettingsRepository _accountSettingsRepository = accountSettingsRepository;
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
            var passwordPolicy = await _accountSettingsRepository.GetPasswordPolicyAsync();

            return !passwordPolicy.UsePasswordPolicy || CheckPasswordPolicy(password, passwordPolicy.MinLength, passwordPolicy.NumNonAlphanumericChars, passwordPolicy.Regex);
        }

        private static bool CheckPasswordPolicy(string password, Maybe<int> minLength, Maybe<int> minNonAlphaNum, Maybe<string> regularExpression)
        {
            // Check minimal length
            if (minLength.TryGetValue(out var minLengthVal) && password.Length < minLengthVal) {
                return false;
            }

            // Check number of non alphanum characters
            int counter = 0;
            foreach (char c in password) {
                if (!Char.IsLetterOrDigit(c)) {
                    counter++;
                }
            }

            if (minNonAlphaNum.TryGetValue(out var nonAlphaNum) && counter < nonAlphaNum) {
                return false;
            }

            // Check regular expression
            if (regularExpression.TryGetValue(out var regexVal)) {
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
