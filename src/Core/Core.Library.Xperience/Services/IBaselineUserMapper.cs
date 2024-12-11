using Kentico.Membership;

namespace Core.Services
{
    public interface IBaselineUserMapper<TUser> where TUser : ApplicationUser, new()
    {
        Task<User> ToUser(TUser applicationUser);

        Task<TUser> ToApplicationUser(User user);
    }
}
