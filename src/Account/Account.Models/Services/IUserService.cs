﻿using Microsoft.AspNetCore.Identity;

namespace Account.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Creates a user on the website.
        /// </summary>
        /// <param name="user">The User information</param>
        /// <param name="password">The Password</param>
        /// <param name="enabled">If they should be enabled right away</param>
        /// <returns>The UserInfo Object</returns>
        [Obsolete("Use CreateUser")]
        Task<User> CreateUserAsync(User user, string password, bool enabled = false);

        /// <summary>
        /// Creates a user on the website.
        /// </summary>
        /// <param name="user">The User information</param>
        /// <param name="password">The Password</param>
        /// <param name="enabled">If they should be enabled right away</param>
        /// <returns>The UserInfo Object</returns>
        Task<Result<User>> CreateUser(User user, string password, bool enabled = false);


        /// <summary>
        /// Sends a Registration Confirmation Email Asyncly to the given User, with the Confirmation link provided
        /// </summary>
        /// <param name="user">The User object</param>
        /// <param name="confirmationLink">The base URL for the Email Confirmation string, the user GUID and Hash are appended to this</param>
        /// <returns></returns>
        Task SendRegistrationConfirmationEmailAsync(User user, string confirmationLink);

        /// <summary>
        /// Validates the Token request for the given User
        /// </summary>
        /// <param name="UserID">The User's ID</param>
        /// <param name="token">The Token</param>
        /// <returns>If the token is valid</returns>
        Task<IdentityResult> ConfirmRegistrationConfirmationTokenAsync(User user, string token);

        /// <summary>
        /// Generates the Password Reset Token, not needed if you use the SendPasswordEmailAsync, but is if you want to write your own email sender
        /// </summary>
        /// <param name="user">The User object</param>
        /// <returns>The Email token that should be passed to the password reset link</returns>
        Task<string> GetPasswordResetTokenAsync(User user);

        /// <summary>
        /// Sends a password reset email for the given user
        /// </summary>
        /// <param name="user">The User object</param>
        /// <param name="confirmationLink">The base URL for the Email Confirmation string, the user GUID and Hash are appended to this</param>
        /// <param name="resetToken">The reset token generated from ConfirmRegistrationConfirmationTokenAsync, optional, if not passed will use this method to generate.</param>
        /// <returns></returns>
        Task SendPasswordResetEmailAsync(User user, string confirmationLink, string? resetToken = null);

        /// <summary>
        /// Validates and resets the password for the given user and token
        /// </summary>
        /// <param name="user">The User's ID</param>
        /// <param name="token">The Token</param>
        /// <param name="newPassword">The new password</param>
        /// <returns>If the operation was successful</returns>
        Task<IdentityResult> ResetPasswordFromTokenAsync(User user, string token, string newPassword);

        /// <summary>
        /// Validates if the password is valid for the given user
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="password">the password</param>
        /// <returns>If it's the correct password</returns>
        Task<bool> ValidateUserPasswordAsync(User user, string password);

        /// <summary>
        /// Resets the password of the given user
        /// </summary>
        /// <param name="user">The Username</param>
        /// <param name="newPassword">The password to reset it to</param>
        /// <param name="currentPassword">the current password, must match</param>
        Task ResetPasswordAsync(User user, string newPassword, string currentPassword);

        /// <summary>
        /// Validates that the given password passes the site's Password Policy
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> ValidatePasswordPolicyAsync(string password);

        /// <summary>
        /// Creates an external user
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns></returns>
        [Obsolete("Use CreateExternalUser")]
        Task CreateExternalUserAsync(User user);

        /// <summary>
        /// Creates an external user
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns></returns>
        Task<Result<User>> CreateExternalUser(User user);

        /// <summary>
        /// Sends the verification token to the given user.
        /// </summary>
        /// <param name="actualUser"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task SendVerificationCodeEmailAsync(User actualUser, string token);
    }
}