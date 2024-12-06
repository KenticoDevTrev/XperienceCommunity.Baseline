using CMS.Membership;
using Kentico.Membership;

namespace Core.Services.Implementations
{
    // Base implementation, you can easily implement your own though to extend both the ApplicationUser and the Generic Model to add properties
    public class BaselineUserMapper<TUser, TGenericUser> : IBaselineUserMapper<TUser, TGenericUser> where TUser : ApplicationUser, new() where TGenericUser : User, new()
    {
        public Task<TUser> ToApplicationUser(TGenericUser user)
        {
            var appUser = new TUser() {
                UserName = user.UserName,
                Enabled = user.Enabled,
                Email = user.Email,
                IsExternal = user.IsExternal,
                FirstName = user.FirstName.GetValueOrDefault(string.Empty),
                LastName = user.LastName.GetValueOrDefault(string.Empty)
            };
            if (user.UserID.TryGetValue(out var userId)) {
                appUser.Id = userId;
            }

            return Task.FromResult(appUser);
        }

        public Task<TGenericUser> ToUser(UserInfo memberInfo) => Task.FromResult(new TGenericUser() {
            UserID = memberInfo.UserID,
            UserName = memberInfo.UserName,
            UserGUID = memberInfo.UserGUID,
            Email = memberInfo.Email,
            Enabled = memberInfo.Enabled,
            IsExternal = memberInfo.IsExternal,
            IsPublic = memberInfo.UserName.Equals("public", StringComparison.OrdinalIgnoreCase)
        });
    }
}
