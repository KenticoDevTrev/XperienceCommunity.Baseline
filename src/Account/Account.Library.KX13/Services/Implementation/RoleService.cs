using CMS.Membership;

namespace Account.Services.Implementation
{
    [AutoDependencyInjection]
    public class RoleService(
        IRoleRepository _roleRepository,
        IRoleInfoProvider _roleInfoProvider,
        ISiteRepository _siteRepository,
        IUserRoleInfoProvider _userRoleInfoProvider) : IRoleService
    {
        public async Task CreateRoleIfNotExisting(string roleName, string siteName)
        {
            var roleResult = await _roleRepository.GetRoleAsync(roleName, siteName);
            if (roleResult.IsFailure)
            {
                var newRole = new RoleInfo()
                {
                    RoleName = roleName,
                    RoleDisplayName = roleName,
                    SiteID = _siteRepository.GetChannelID(siteName).GetValueOrDefault(0),
                    RoleDescription = "auto generated from IAuthenticationConfiguration settings"
                };
                _roleInfoProvider.Set(newRole);
            }
            return;
        }

        public async Task SetUserRole(int userID, string roleName, string siteName, bool roleToggle)
        {
            var roleResult = await _roleRepository.GetRoleAsync(roleName, siteName);
            if (roleResult.TryGetValue(out var role))
            {
                if (roleToggle && !(await _roleRepository.UserInRoleAsync(userID, roleName, siteName)))
                {
                    _userRoleInfoProvider.Add(userID, role.RoleID);
                }
                else
                {
                    _userRoleInfoProvider.Remove(userID, role.RoleID);
                }
            }
            return;
        }
    }
}