namespace Account.Services.Implementations
{
    public class RoleService : IRoleService
    {
        public Task CreateRoleIfNotExisting(string roleName, string siteName)
        {
            // TODO: This won't work until Kentico provides Member Roles
            return Task.CompletedTask;
        }

        public Task SetUserRole(int userID, string roleName, string siteName, bool roleToggle)
        {
            // TODO: This won't work until Kentico provides Member Roles
            return Task.CompletedTask;
        }
    }
}
