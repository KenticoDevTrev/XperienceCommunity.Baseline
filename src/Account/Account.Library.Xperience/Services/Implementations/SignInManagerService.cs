using CSharpFunctionalExtensions;
using Kentico.Membership;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
namespace Account.Services.Implementations
{
    public class SignInManagerService<TUser>(
        UserManager<TUser> UserManager,
        SignInManager<TUser> SignInManager) : ISignInManagerService where TUser : ApplicationUser, new()
    {
        private readonly UserManager<TUser> _userManager = UserManager;
        private readonly SignInManager<TUser> _signInManager = SignInManager;

        public async Task<bool> IsTwoFactorClientRememberedByNameAsync(string userName) => await IsTwoFactorClientRememberedAsync(await _userManager.FindByNameAsync(userName));
        public async Task<bool> IsTwoFactorClientRememberedByEmailAsync(string email) => await IsTwoFactorClientRememberedAsync(await _userManager.FindByEmailAsync(email));
        public async Task<bool> IsTwoFactorClientRememberedByIdAsync(string userId) => await IsTwoFactorClientRememberedAsync(await _userManager.FindByIdAsync(userId));
        public async Task<bool> IsTwoFactorClientRememberedByLoginAsync(string loginProvider, string providerKey) => await IsTwoFactorClientRememberedAsync(await _userManager.FindByLoginAsync(loginProvider, providerKey));
        private async Task<bool> IsTwoFactorClientRememberedAsync(TUser? applicationUser)
        {
            if (applicationUser == null) {
                return false;
            }
            return await _signInManager.IsTwoFactorClientRememberedAsync(applicationUser);
        }

        public async Task RememberTwoFactorClientRememberedByNameAsync(string userName) => await RememberTwoFactorClientRememberedAsync(await _userManager.FindByNameAsync(userName));
        public async Task RememberTwoFactorClientRememberedByEmailAsync(string email) => await RememberTwoFactorClientRememberedAsync(await _userManager.FindByEmailAsync(email));
        public async Task RememberTwoFactorClientRememberedByIdAsync(string userId) => await RememberTwoFactorClientRememberedAsync(await _userManager.FindByIdAsync(userId));
        public async Task RememberTwoFactorClientRememberedByLoginAsync(string loginProvider, string providerKey) => await RememberTwoFactorClientRememberedAsync(await _userManager.FindByLoginAsync(loginProvider, providerKey));
        private async Task RememberTwoFactorClientRememberedAsync(TUser? applicationUser)
        {
            if (applicationUser == null) {
                return;
            }
            await _signInManager.RememberTwoFactorClientAsync(applicationUser);
        }

        public async Task SignInByNameAsync(string userName, bool stayLoggedIn) => await SignInAsync(await _userManager.FindByNameAsync(userName), stayLoggedIn);
        public async Task SignInByEmailAsync(string email, bool stayLoggedIn) => await SignInAsync(await _userManager.FindByEmailAsync(email), stayLoggedIn);
        public async Task SignInByIdAsync(string userId, bool stayLoggedIn) => await SignInAsync(await _userManager.FindByIdAsync(userId), stayLoggedIn);
        public async Task SignInByLoginAsync(string loginProvider, string providerKey, bool stayLoggedIn) => await SignInAsync(await _userManager.FindByLoginAsync(loginProvider, providerKey), stayLoggedIn);
        private async Task SignInAsync(TUser? applicationUser, bool stayLoggedIn)
        {
            if (applicationUser == null) {
                return;
            }
            await _signInManager.SignInAsync(applicationUser, stayLoggedIn);
        }

        public async Task<SignInResult> PasswordSignInByNameAsync(string userName, string password, bool stayLoggedIn, bool lockoutOnFailure) => await PasswordSignInAsync(await _userManager.FindByNameAsync(userName), password, stayLoggedIn, lockoutOnFailure);
        public async Task<SignInResult> PasswordSignInByEmailAsync(string email, string password, bool stayLoggedIn, bool lockoutOnFailure) => await PasswordSignInAsync(await _userManager.FindByEmailAsync(email), password, stayLoggedIn, lockoutOnFailure);
        public async Task<SignInResult> PasswordSignInByIdAsync(string userId, string password, bool stayLoggedIn, bool lockoutOnFailure) => await PasswordSignInAsync(await _userManager.FindByIdAsync(userId), password, stayLoggedIn, lockoutOnFailure);
        private async Task<SignInResult> PasswordSignInAsync(TUser? applicationUser, string password, bool stayLoggedIn, bool lockoutOnFailure)
        {
            if (applicationUser == null) {
                return SignInResult.Failed;
            }
            return await _signInManager.PasswordSignInAsync(applicationUser, password, stayLoggedIn, lockoutOnFailure);
        }

        public async Task<Result<ExternalLoginInfo>> GetExternalLoginInfoAsync()
        {
            var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo != null) {
                return externalLoginInfo;
            }
            return Result.Failure<ExternalLoginInfo>("External Login Info is null");
        }

        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        public async Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
        {
            return await _signInManager.GetExternalAuthenticationSchemesAsync();
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
