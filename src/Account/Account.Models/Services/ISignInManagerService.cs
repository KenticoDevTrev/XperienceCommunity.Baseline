using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Account.Services
{
    public interface ISignInManagerService
    {
        /// <summary>
        /// If two factor is saved and remembered for the user (check before doing two factor prompt)
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<bool> IsTwoFactorClientRememberedByNameAsync(string userName);
        /// <summary>
        /// If two factor is saved and remembered for the user (check before doing two factor prompt)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<bool> IsTwoFactorClientRememberedByEmailAsync(string email);
        /// <summary>
        /// If two factor is saved and remembered for the user (check before doing two factor prompt)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> IsTwoFactorClientRememberedByIdAsync(string userId);
        /// <summary>
        /// If two factor is saved and remembered for the user (check before doing two factor prompt)
        /// </summary>
        /// <param name="loginProvider"></param>
        /// <param name="providerKey"></param>
        /// <returns></returns>
        Task<bool> IsTwoFactorClientRememberedByLoginAsync(string loginProvider, string providerKey);

        /// <summary>
        /// Sets that the two factor client is remembered for the user (use after two factor authentication successful)
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task RememberTwoFactorClientRememberedByNameAsync(string userName);
        /// <summary>
        /// Sets that the two factor client is remembered for the user (use after two factor authentication successful)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task RememberTwoFactorClientRememberedByEmailAsync(string email);
        /// <summary>
        /// Sets that the two factor client is remembered for the user (use after two factor authentication successful)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task RememberTwoFactorClientRememberedByIdAsync(string userId);
        /// <summary>
        /// Sets that the two factor client is remembered for the user (use after two factor authentication successful)
        /// </summary>
        /// <param name="loginProvider"></param>
        /// <param name="providerKey"></param>
        /// <returns></returns>
        Task RememberTwoFactorClientRememberedByLoginAsync(string loginProvider, string providerKey);

        /// <summary>
        /// Signs in the user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="stayLoggedIn"></param>
        /// <returns></returns>
        Task SignInByNameAsync(string userName, bool stayLoggedIn);
        /// <summary>
        /// Signs in the user
        /// </summary>
        /// <param name="email"></param>
        /// <param name="stayLoggedIn"></param>
        /// <returns></returns>
        Task SignInByEmailAsync(string email, bool stayLoggedIn);
        /// <summary>
        /// Signs in the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="stayLoggedIn"></param>
        /// <returns></returns>
        Task SignInByIdAsync(string userId, bool stayLoggedIn);
        /// <summary>
        /// Signs in the user
        /// </summary>
        /// <param name="loginProvider"></param>
        /// <param name="providerKey"></param>
        /// <param name="stayLoggedIn"></param>
        /// <returns></returns>
        Task SignInByLoginAsync(string loginProvider, string providerKey, bool stayLoggedIn);

        /// <summary>
        /// Signs in the user if the given password is correct
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="isPersistant"></param>
        /// <param name="lockoutOnFailure"></param>
        /// <returns></returns>
        Task<SignInResult> PasswordSignInByNameAsync(string userName, string password, bool isPersistant, bool lockoutOnFailure);
        /// <summary>
        /// Signs in the user if the given password is correct
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="isPersistant"></param>
        /// <param name="lockoutOnFailure"></param>
        /// <returns></returns>
        Task<SignInResult> PasswordSignInByEmailAsync(string email, string password, bool isPersistant, bool lockoutOnFailure);
        /// <summary>
        /// Signs in the user if the given password is correct
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="isPersistant"></param>
        /// <param name="lockoutOnFailure"></param>
        /// <returns></returns>
        Task<SignInResult> PasswordSignInByIdAsync(string userId, string password, bool isPersistant, bool lockoutOnFailure);

        /// <summary>
        /// Signs out the user
        /// </summary>
        /// <returns></returns>
        Task SignOutAsync();

        /// <summary>
        /// Configures the redirect URL and user identifier for the specified external login <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">The provider to configure.</param>
        /// <param name="redirectUrl">The external login URL users should be redirected to during the login flow.</param>
        /// <returns>A configured <see cref="AuthenticationProperties"/>.</returns>
        AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);

        /// <summary>
        /// Attempts to get the External Login Information of the current user (Result.Failure if none)
        /// </summary>
        /// <returns></returns>
        Task<Result<ExternalLoginInfo>> GetExternalLoginInfoAsync();
        
        /// <summary>
        /// Gets the Authentication Schemas available (various .net Identity providers, like Google, Facebook, etc.  Configured through applicationsettings)
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync();
    }
}
