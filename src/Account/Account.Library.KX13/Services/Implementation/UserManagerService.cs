using Account.Services;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;

namespace Account.KX13.Services.Implementation
{
    public class UserManagerService : IUserManagerService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagerService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> CheckPasswordByNameAsync(string userName, string password) => await CheckPasswordAsync(await _userManager.FindByNameAsync(userName), password);
        public async Task<bool> CheckPasswordByEmailAsync(string email, string password) => await CheckPasswordAsync(await _userManager.FindByEmailAsync(email), password);
        public async Task<bool> CheckPasswordByIdAsync(string userId, string password) => await CheckPasswordAsync(await _userManager.FindByIdAsync(userId), password);
        public async Task<bool> CheckPasswordByLoginAsync(string loginProvider, string providerKey, string password) => await CheckPasswordAsync(await _userManager.FindByLoginAsync(loginProvider, providerKey), password);
        private async Task<bool> CheckPasswordAsync(ApplicationUser applicationUser, string password)
        {
            return await _userManager.CheckPasswordAsync(applicationUser, password);
        }


        public async Task<string> GenerateTwoFactorTokenByNameAsync(string userName, string tokenProvider) => await GenerateTwoFactorTokenAsync(await _userManager.FindByNameAsync(userName), tokenProvider);
        public async Task<string> GenerateTwoFactorTokenByEmailAsync(string email, string tokenProvider) => await GenerateTwoFactorTokenAsync(await _userManager.FindByEmailAsync(email), tokenProvider);
        public async Task<string> GenerateTwoFactorTokenByIdAsync(string userId, string tokenProvider) => await GenerateTwoFactorTokenAsync(await _userManager.FindByIdAsync(userId), tokenProvider);
        public async Task<string> GenerateTwoFactorTokenByLoginAsync(string loginProvider, string providerKey, string tokenProvider) => await GenerateTwoFactorTokenAsync(await _userManager.FindByLoginAsync(loginProvider, providerKey), tokenProvider);

        private async Task<string> GenerateTwoFactorTokenAsync(ApplicationUser applicationUser, string tokenProvider)
        {
            if(applicationUser == null)
            {
                return string.Empty;
            }
            return await _userManager.GenerateTwoFactorTokenAsync(applicationUser, tokenProvider);
        }

        public async Task<bool> UserExistsByNameAsync(string userName) => (await _userManager.FindByNameAsync(userName)) != null;
        public async Task<bool> UserExistsByEmailAsync(string email) => (await _userManager.FindByEmailAsync(email)) != null;
        public async Task<bool> UserExistsByIdAsync(string userId) => (await _userManager.FindByIdAsync(userId)) != null;
        public async Task<bool> UserExistsByLoginAsync(string loginProvider, string providerKey) => (await _userManager.FindByLoginAsync(loginProvider, providerKey)) != null;


        public async Task<int> GetUserIDByNameAsync(string userName) => (await _userManager.FindByNameAsync(userName))?.Id ?? 0;
        public async Task<int> GetUserIDByEmailAsync(string email) => (await _userManager.FindByEmailAsync(email))?.Id ?? 0;
        public async Task<int> GetUserIDByIdAsync(string userId) => (await _userManager.FindByIdAsync(userId))?.Id ?? 0;
        public async Task<int> GetUserIDByLoginAsync(string loginProvider, string providerKey) => (await _userManager.FindByLoginAsync(loginProvider, providerKey))?.Id ?? 0;


        public async Task<bool> VerifyTwoFactorTokenByNameAsync(string userName, string tokenProvider, string twoFormCode) => await VerifyTwoFactorTokenAsync(await _userManager.FindByNameAsync(userName), tokenProvider, twoFormCode);
        public async Task<bool> VerifyTwoFactorTokenByEmailAsync(string email, string tokenProvider, string twoFormCode) => await VerifyTwoFactorTokenAsync(await _userManager.FindByEmailAsync(email), tokenProvider, twoFormCode);
        public async Task<bool> VerifyTwoFactorTokenByIdAsync(string userId, string tokenProvider, string twoFormCode) => await VerifyTwoFactorTokenAsync(await _userManager.FindByIdAsync(userId), tokenProvider, twoFormCode);
        public async Task<bool> VerifyTwoFactorTokenByLoginAsync(string loginProvider, string providerKey, string tokenProvider, string twoFormCode) => await VerifyTwoFactorTokenAsync(await _userManager.FindByLoginAsync(loginProvider, providerKey), tokenProvider, twoFormCode);
        private async Task<bool> VerifyTwoFactorTokenAsync(ApplicationUser applicationUser, string tokenProvider, string twoFormCode)
        {
            if(applicationUser == null)
            {
                return false;
            }
            return await _userManager.VerifyTwoFactorTokenAsync(applicationUser, tokenProvider, twoFormCode);
        }

        public async Task<bool> EnableUserByNameAsync(string userName, bool setAsExternalIfExternal) => await EnableUserAsync(await _userManager.FindByNameAsync(userName), setAsExternalIfExternal);
        public async Task<bool> EnableUserByEmailAsync(string email, bool setAsExternalIfExternal) => await EnableUserAsync(await _userManager.FindByEmailAsync(email), setAsExternalIfExternal);
        public async Task<bool> EnableUserByIdAsync(string userId, bool setAsExternalIfExternal) => await EnableUserAsync(await _userManager.FindByIdAsync(userId), setAsExternalIfExternal);
        public async Task<bool> EnableUserByLoginAsync(string loginProvider, string providerKey, bool setAsExternalIfExternal) => await EnableUserAsync(await _userManager.FindByLoginAsync(loginProvider, providerKey), setAsExternalIfExternal);

        private async Task<bool> EnableUserAsync(ApplicationUser applicationUser, bool setAsExternalIfExternal)
        {
            bool updateUser = false;
            if(applicationUser == null)
            {
                return false;
            }

            // If user started account through email, but then didn't confirm and 
            // then authenticated 3rd party, their email is now 'confirmed' so will enable
            // and set that to true.  This way you can STILL disable an existing account
            if (!applicationUser.Enabled && !applicationUser.EmailConfirmed)
            {
                applicationUser.Enabled = true;
                applicationUser.EmailConfirmed = true;
                updateUser = true;
            }

            // Depending on setting, convert to external only
            if (!applicationUser.IsExternal && setAsExternalIfExternal)
            {
                applicationUser.IsExternal = true;
                updateUser = true;
            }

            if (updateUser)
            {
                await _userManager.UpdateAsync(applicationUser);
            }
            return updateUser;
        }
    }
}
