using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using XperienceCommunity.MemberRoles.Models;

namespace Account.Services.Implementations
{
    public class RoleService(IRoleStore<TagApplicationUserRole> roleStore, IUserRoleStore<ApplicationUser> userRoleStore, IUserStore<ApplicationUser> userStore) : IRoleService
    {
        private readonly IRoleStore<TagApplicationUserRole> _roleStore = roleStore;
        private readonly IUserRoleStore<ApplicationUser> _userRoleStore = userRoleStore;
        private readonly IUserStore<ApplicationUser> _userStore = userStore;

        public async Task CreateRoleIfNotExisting(string roleName, string siteName)
        {
            // This will create if it doesn't exist, if it does then it's fine.
            await _roleStore.CreateAsync(new TagApplicationUserRole() {
                Name = roleName,
                NormalizedName = roleName.Normalize()
            }, CancellationToken.None);
        }

        public async Task SetUserRole(int userID, string roleName, string siteName, bool roleToggle)
        {
            var user = await _userStore.FindByIdAsync(userID.ToString(), CancellationToken.None);
            if(user != null) {
                if (roleToggle) {
                    await _userRoleStore.AddToRoleAsync(user, roleName, CancellationToken.None);
                } else {
                    await _userRoleStore.RemoveFromRoleAsync(user, roleName, CancellationToken.None);
                }
            }
        }
    }
}
