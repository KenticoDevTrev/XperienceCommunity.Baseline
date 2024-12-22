using CMS.Membership;
using Kentico.Membership;

namespace Core.Services
{
    public interface IBaselineUserMapper<TUser> where TUser : ApplicationUser, new()
    {
        Task<User> ToUser(UserInfo userInfo);

        Task<TUser> ToApplicationUser(User user);
    }
}
