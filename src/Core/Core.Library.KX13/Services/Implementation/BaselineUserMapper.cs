using CMS.Membership;
using Kentico.Membership;

namespace Core.Services.Implementations
{
    // Base implementation, you can easily implement your own though to extend both the ApplicationUser and the Generic Model to add properties
    public class BaselineUserMapper<TUser> : IBaselineUserMapper<TUser> where TUser : ApplicationUser, new()
    {
        public Task<TUser> ToApplicationUser(User userInfo)
        {
            var appUser = new TUser() {
                UserName = userInfo.UserName,
                Enabled = userInfo.Enabled,
                Email = userInfo.Email,
                IsExternal = userInfo.IsExternal,
                FirstName = userInfo.FirstName.GetValueOrDefault(string.Empty),
                LastName = userInfo.LastName.GetValueOrDefault(string.Empty)
            };

            if(userInfo.UserID.TryGetValue(out var id)) { 
                appUser.Id = id;
            }

            return Task.FromResult(appUser);
        }

        public Task<User> ToUser(UserInfo userInfo) => Task.FromResult(new User() {
            UserID = userInfo.UserID,
            UserName = userInfo.UserName,
            UserGUID = userInfo.UserGUID,
            Email = userInfo.Email,
            Enabled = userInfo.Enabled,
            IsExternal = userInfo.IsExternal,
            IsPublic = userInfo.UserName.Equals("public", StringComparison.OrdinalIgnoreCase),
            FirstName = userInfo.FirstName.AsNullOrWhitespaceMaybe(),
            MiddleName = userInfo.MiddleName.AsNullOrWhitespaceMaybe(),
            LastName = userInfo.LastName.AsNullOrWhitespaceMaybe()
        });
    }
}
