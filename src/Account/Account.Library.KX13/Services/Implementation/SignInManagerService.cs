using Account.Services;
using Kentico.Membership;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.KX13.Services.Implementation
{
    public class SignInManagerService : ISignInManagerService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SignInManagerService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

 

        public async Task<bool> IsTwoFactorClientRememberedByNameAsync(string userName) => await IsTwoFactorClientRememberedAsync(await _userManager.FindByNameAsync(userName));
        public async Task<bool> IsTwoFactorClientRememberedByEmailAsync(string email) => await IsTwoFactorClientRememberedAsync(await _userManager.FindByEmailAsync(email));
        public async Task<bool> IsTwoFactorClientRememberedByIdAsync(string userId) => await IsTwoFactorClientRememberedAsync(await _userManager.FindByIdAsync(userId));
        public async Task<bool> IsTwoFactorClientRememberedByLoginAsync(string loginProvider, string providerKey) => await IsTwoFactorClientRememberedAsync(await _userManager.FindByLoginAsync(loginProvider, providerKey));
        private async Task<bool> IsTwoFactorClientRememberedAsync(ApplicationUser applicationUser)
        {
            if(applicationUser == null)
            {
                return false;
            }
            return await _signInManager.IsTwoFactorClientRememberedAsync(applicationUser);
        }

        public async Task SignInByNameAsync(string userName, bool stayLoggedIn) => await SignInAsync(await _userManager.FindByNameAsync(userName), stayLoggedIn);
        public async Task SignInByEmailAsync(string email, bool stayLoggedIn) => await SignInAsync(await _userManager.FindByEmailAsync(email), stayLoggedIn);
        public async Task SignInByIdAsync(string userId, bool stayLoggedIn) => await SignInAsync(await _userManager.FindByIdAsync(userId), stayLoggedIn);
        public async Task SignInByLoginAsync(string loginProvider, string providerKey, bool stayLoggedIn) => await SignInAsync(await _userManager.FindByLoginAsync(loginProvider, providerKey), stayLoggedIn);
        private async Task SignInAsync(ApplicationUser applicationUser, bool stayLoggedIn)
        {
            if (applicationUser == null)
            {
                return;
            }
            await _signInManager.SignInAsync(applicationUser, stayLoggedIn);
        }

        public async Task<SignInResult> PasswordSignInByNameAsync(string userName, string password, bool stayLoggedIn, bool lockoutOnFailure) => await PasswordSignInAsync(await _userManager.FindByNameAsync(userName), password, stayLoggedIn, lockoutOnFailure);
        public async Task<SignInResult> PasswordSignInByEmailAsync(string email, string password, bool stayLoggedIn, bool lockoutOnFailure) => await PasswordSignInAsync(await _userManager.FindByEmailAsync(email), password, stayLoggedIn, lockoutOnFailure);
        public async Task<SignInResult> PasswordSignInByIdAsync(string userId, string password, bool stayLoggedIn, bool lockoutOnFailure) => await PasswordSignInAsync(await _userManager.FindByIdAsync(userId), password, stayLoggedIn, lockoutOnFailure);
        private async Task<SignInResult> PasswordSignInAsync(ApplicationUser applicationUser, string password, bool stayLoggedIn, bool lockoutOnFailure)
        {
            if (applicationUser == null)
            {
                return SignInResult.Failed;
            }
            return await _signInManager.PasswordSignInAsync(applicationUser, password, stayLoggedIn, lockoutOnFailure);
        }

        public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            return await _signInManager.GetExternalLoginInfoAsync();
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
