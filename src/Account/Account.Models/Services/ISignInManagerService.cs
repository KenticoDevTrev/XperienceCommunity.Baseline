using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Account.Services
{
    public interface ISignInManagerService
    {
        Task<bool> IsTwoFactorClientRememberedByNameAsync(string userName);
        Task<bool> IsTwoFactorClientRememberedByEmailAsync(string userName);
        Task<bool> IsTwoFactorClientRememberedByIdAsync(string userName);
        Task<bool> IsTwoFactorClientRememberedByLoginAsync(string loginProvider, string providerKey);

        Task RememberTwoFactorClientRememberedByNameAsync(string userName);
        Task RememberTwoFactorClientRememberedByEmailAsync(string userName);
        Task RememberTwoFactorClientRememberedByIdAsync(string userName);
        Task RememberTwoFactorClientRememberedByLoginAsync(string loginProvider, string providerKey);



        Task SignInByNameAsync(string userName, bool stayLoggedIn);
        Task SignInByEmailAsync(string email, bool stayLoggedIn);
        Task SignInByIdAsync(string userId, bool stayLoggedIn);
        Task SignInByLoginAsync(string loginProvider, string providerKey, bool stayLoggedIn);

        Task<SignInResult> PasswordSignInByNameAsync(string userName, string password, bool isPersistant, bool lockoutOnFailure);
        Task<SignInResult> PasswordSignInByEmailAsync(string email, string password, bool isPersistant, bool lockoutOnFailure);
        Task<SignInResult> PasswordSignInByIdAsync(string userId, string password, bool isPersistant, bool lockoutOnFailure);

        Task SignOutAsync();

        /// <summary>
        /// Configures the redirect URL and user identifier for the specified external login <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">The provider to configure.</param>
        /// <param name="redirectUrl">The external login URL users should be redirected to during the login flow.</param>
        /// <param name="userId">The current user's identifier, which will be used to provide CSRF protection.</param>
        /// <returns>A configured <see cref="AuthenticationProperties"/>.</returns>
        AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl);
        
        Task<Result<ExternalLoginInfo>> GetExternalLoginInfoAsync();
        Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync();
    }
}
