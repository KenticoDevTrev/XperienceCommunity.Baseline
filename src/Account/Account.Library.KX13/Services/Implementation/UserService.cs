﻿using CMS.Base;
using CMS.Core;
using CMS.Membership;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using System.Web;

namespace Account.Services.Implementation
{
    [AutoDependencyInjection]
    public class UserService(
        IUserInfoProvider _userInfoProvider,
        ApplicationUserManager<ApplicationUser> _userManager,
        IMessageService _emailService,
        IProgressiveCache _progressiveCache,
        ISiteRepository _siteRepository,
        IEventLogService _eventLogService) : IUserService
    {

        public Task<User> CreateUserAsync(User user, string password, bool enabled = false)
        {
            // Create basic user
            var newUser = new UserInfo()
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.None,
                Enabled = enabled
            };
            _userInfoProvider.Set(newUser);

            // Generate new password, and save any other settings
            UserInfoProvider.SetPassword(newUser, password);

            return Task.FromResult(newUser.ToUser());
        }

        public async Task SendRegistrationConfirmationEmailAsync(User user, string confirmationUrl)
        {
            var appUser = await _userManager.FindByNameAsync(user.UserName);
            if(appUser == null)
            {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "SendRegistrationConfirmationError", eventDescription: $"Could not send a user confirmation email to {user.UserName} as one was not found in Kentico.");
                return;
            }
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

            // Creates and sends the confirmation email to the user's address
            await _emailService.SendEmailAsync(appUser.Email, "Confirm your new account",
                string.Format($"Please confirm your new account by clicking <a href=\"{confirmationUrl}?userId={user.UserGUID}&token={HttpUtility.UrlEncode(token)}\">here</a>"));
        }

        public async Task<IdentityResult> ConfirmRegistrationConfirmationTokenAsync(User user, string token)
        {
            var appUser = await _userManager.FindByIdAsync(user.UserID.ToString());
            if (appUser == null)
            {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "ConfirmRegistrationConfirmationTokenError", eventDescription: $"Could not confirm token for user with ID {user.UserID} was not found in Kentico.");
                return IdentityResult.Failed([new() {  Description = $"Could not find user with ID {user.UserID}", Code = "NO_USER"}]);
            }
            return await _userManager.ConfirmEmailAsync(appUser, token);
        }

        public async Task SendPasswordResetEmailAsync(User user, string confirmationLink)
        {
            var appUser = await _userManager.FindByIdAsync(user.UserID.ToString());
            if (appUser == null)
            {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "SendPasswordResetEmailError", eventDescription: $"Could not send password reset for user with ID {user.UserID} was not found in Kentico.");
                return;
            }
            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

            // Creates and sends the confirmation email to the user's address
            await _emailService.SendEmailAsync(user.Email, "Password Reset Request",
                $"A Password reset request has been generated for your account.  If you have generated this request, you may reset your password by clicking <a href=\"{confirmationLink}?userId={user.UserGUID}&token={HttpUtility.UrlEncode(token)}\">here</a>.");
        }

        public async Task<IdentityResult> ResetPasswordFromTokenAsync(User user, string token, string newPassword)
        {
            var appUser = await _userManager.FindByIdAsync(user.UserID.ToString());
            if (appUser == null)
            {
                _eventLogService.LogEvent(EventTypeEnum.Error, "UserService.cs", "ResetPasswordFromTokenError", eventDescription: $"Could not reset password from token for user with ID {user.UserID} was not found in Kentico.");
                return IdentityResult.Failed([new() { Description = $"Could not find user with ID {user.UserID}", Code = "NO_USER" }]);
            }
            return await _userManager.ResetPasswordAsync(appUser, token, newPassword);
        }

        public async Task<bool> ValidateUserPasswordAsync(User user, string password)
        {
            var userInfoObj = await GetUserInfoAsync(user.UserName);
            return UserInfoProvider.ValidateUserPassword(userInfoObj, password);
        }

        public Task ResetPasswordAsync(User user, string password)
        {
            UserInfoProvider.SetPassword(user.UserName, password, true);
            return Task.CompletedTask;
        }

        public Task<bool> ValidatePasswordPolicyAsync(string password)
        {
            return Task.FromResult(SecurityHelper.CheckPasswordPolicy(password, _siteRepository.CurrentSiteName()));
        }

        private async Task<UserInfo> GetUserInfoAsync(string userName)
        {
            return await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{UserInfo.OBJECT_TYPE}|byname|{userName}");
                }
                return await _userInfoProvider.GetAsync(userName);
            }, new CacheSettings(15, "GetUserInfoAsync", userName));
        }

        public Task CreateExternalUserAsync(User user)
        {
            // Create basic user
            var newUser = new UserInfo()
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                SiteIndependentPrivilegeLevel = UserPrivilegeLevelEnum.None,
                Enabled = user.Enabled,
                IsExternal = true
            };
            _userInfoProvider.Set(newUser);

            return Task.FromResult(newUser.ToUser());
        }

        public async Task SendVerificationCodeEmailAsync(User user, string token)
        {
            // Creates and sends the confirmation email to the user's address
            await _emailService.SendEmailAsync(user.Email, "Verification Code",
                $"<p>Hello {user.UserName}!</p><p>When Prompted, enter the code below to finish authenticating:</p> <table align=\"center\" width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><tbody><tr><td width=\"15%\"></td><td width=\"70%\" align=\"center\" bgcolor=\"#f1f3f2\" style=\"color:black;margin-bottom:10px;border-radius:10px\"><p style=\"font-size:xx-large;font-weight:bold;margin:10px 0px\">{token}</p></td></tr></tbody></table>");
        }
    }
}