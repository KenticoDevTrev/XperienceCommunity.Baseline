using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class AccountMiddleware
    {
        public static WebApplicationBuilder AddBaselineKenticoAuthentication(this WebApplicationBuilder builder)
        {

            /*
            // Adds and configures ASP.NET Identity for the application
            builder.Services.AddIdentity<ApplicationUser, NoOpApplicationRole>(options => {
                // Ensures that disabled member accounts cannot sign in
                options.SignIn.RequireConfirmedAccount = true;
                // Ensures unique emails for registered accounts
                options.User.RequireUniqueEmail = true;

                // Note: Can customize password requirements here, there is no longer a 'settings' in Kentico for this.
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 10;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 3;
            })
                .AddUserStore<ApplicationUserStore<ApplicationUser>>()
                // TODO: Experiment with how to set 'roles' for membership in future
                .AddRoleStore<NoOpApplicationRoleStore>()
                //.AddRoleStore<ApplicationRoleStore<ApplicationRole>>()

                .AddUserManager<UserManager<ApplicationUser>>()
                .AddSignInManager<SignInManager<ApplicationUser>>();
            */
            // Adds and configures ASP.NET Identity for the application
            builder.Services.AddIdentity<ApplicationUser, NoOpApplicationRole>(options =>
            {
                // Ensures that disabled member accounts cannot sign in
                options.SignIn.RequireConfirmedAccount = true;
                // Ensures unique emails for registered accounts
                options.User.RequireUniqueEmail = true;
            })
                .AddUserStore<ApplicationUserStore<ApplicationUser>>()
                .AddRoleStore<NoOpApplicationRoleStore>()
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddSignInManager<SignInManager<ApplicationUser>>();

            return builder;
        }
    }
}
