using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Account.Repositories.Implementation
{
    public class AuthenticationConfiguration : IAuthenticationConfigurations
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

    public static class AuthenticationConfigurationExtensions
    {
        public static AuthenticationBuilder ConfigureAuthentication(this AuthenticationBuilder builder, Action<AuthenticationConfiguration> configuration)
        {
            var defaultObj = new AuthenticationConfiguration()
            {
                // default here
                AllExternalUserRoles = ["external-user"]
            };
            configuration.Invoke(defaultObj);
            builder.Services.AddSingleton<IAuthenticationConfigurations>(defaultObj);
            return builder;
        }
    }
}

