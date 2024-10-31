using Account.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Account.Repositories.Implementations
{
    public class AuthenticationConfigurations : IAuthenticationConfigurations
    {
        public ExistingInternalUserBehavior ExistingInternalUserBehavior { get; set; } = ExistingInternalUserBehavior.LeaveAsIs;
        public List<string> InternalUserRoles { get; set; } = [];
        public List<string> AllExternalUserRoles { get; set; } = [];
        public List<string> FacebookUserRoles { get; set; } = [];
        public List<string> GoogleUserRoles { get; set; } = [];
        public List<string> MicrosoftUserRoles { get; set; } = [];
        public List<string> TwitterUserRoles { get; set; } = [];
        public bool UseTwoFormAuthentication { get; set; } = false;

        public ExistingInternalUserBehavior GetExistingInternalUserBehavior() => ExistingInternalUserBehavior;

        IEnumerable<string> IAuthenticationConfigurations.AllExternalUserRoles() => AllExternalUserRoles;

        IEnumerable<string> IAuthenticationConfigurations.FacebookUserRoles() => FacebookUserRoles;

        IEnumerable<string> IAuthenticationConfigurations.GoogleUserRoles() => GoogleUserRoles;

        IEnumerable<string> IAuthenticationConfigurations.InternalUserRoles() => InternalUserRoles;

        IEnumerable<string> IAuthenticationConfigurations.MicrosoftUserRoles() => MicrosoftUserRoles;

        IEnumerable<string> IAuthenticationConfigurations.TwitterUserRoles() => TwitterUserRoles;

        bool IAuthenticationConfigurations.UseTwoFormAuthentication() => UseTwoFormAuthentication;
    }
}
