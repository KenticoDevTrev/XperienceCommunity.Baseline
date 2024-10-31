using Account.Models;
using CSharpFunctionalExtensions;

namespace Account.Repositories.Implementations
{
    /// <summary>
    /// TODO: Implement once Kentico has this available...
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        public Task<Result<RoleItem>> GetRoleAsync(string roleName, string siteName)
        {
            return Task.FromResult(Result.Failure<RoleItem>("Currently Xperience by Kentico does not have Member Roles"));
        }

        public Task<bool> UserHasPermissionAsync(int userID, string resourceName, string permissionName, string siteName)
        {
            return Task.FromResult(false);
        }

        public Task<bool> UserInRoleAsync(int userID, string roleName, string siteName)
        {
            return Task.FromResult(false);
        }
    }
}
