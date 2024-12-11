using Kentico.Membership;
using Microsoft.AspNetCore.Identity;


namespace Account.Services.Implementations
{
    public class BaselineUserTwoFactorTokenProvider<TUser> : IUserTwoFactorTokenProvider<TUser> where TUser : ApplicationUser, new()
    {
        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            throw new NotImplementedException();
        }
    }
}
