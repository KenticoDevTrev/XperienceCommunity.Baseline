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
            };
            if (user.UserID.TryGetValue(out var userId)) {
                appUser.Id = userId;
            }

            return Task.FromResult(appUser);
        }

        public Task<TGenericUser> ToUser(MemberInfo memberInfo) => Task.FromResult(new TGenericUser() {
            UserID = memberInfo.MemberID,
            UserName = memberInfo.MemberName,
            UserGUID = memberInfo.MemberGuid,
            Email = memberInfo.MemberEmail,
            Enabled = memberInfo.MemberEnabled,
            IsExternal = memberInfo.MemberIsExternal,
            IsPublic = memberInfo.MemberName.Equals("public", StringComparison.OrdinalIgnoreCase)
        });
    }
}
