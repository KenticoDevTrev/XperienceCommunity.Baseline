using Account.Features.Account.LogIn;
using Account.Models;
using Account.Repositories;
using Account.Repositories.Implementations;
using Account.Services;
using Account.Services.Implementations;
using Kentico.Membership;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using XperienceCommunity.Authorization;

namespace Microsoft.AspNetCore.Builder
{
    public static class AuthenticationServiceExtensions

    {

        public static IMvcBuilder AddControllersWithViewsWithKenticoAuthorization(this IServiceCollection services)
        {
            return services.AddControllersWithViews();
            // TODO: Once built, add this back in
            //return services.AddControllersWithViews(opt => opt.Filters.AddKenticoAuthorization());
        }

        public static WebApplicationBuilder AddBaselineKenticoAuthentication(this WebApplicationBuilder builder, Action<IdentityOptions>? identityOptions = null, Action<AuthenticationConfigurations>? authenticationConfigurations = null, PasswordPolicySettings? passwordPolicySettings = null, string AUTHENTICATION_COOKIE_NAME = "identity.authentication")
        {
            // Register DI
            builder.Services.AddScoped<IAccountSettingsRepository, AccountSettingsRepository>()
                .AddScoped<IRoleRepository, RoleRepository>()
                .AddScoped<IRoleService, RoleService>()
                .AddScoped<ISignInManagerService, SignInManagerService>()
                .AddScoped<IUserManagerService, UserManagerService>()
                .AddScoped<IUserService, UserService>();

            // Baseline Configuration of External Authentication
            var defaultObj = new AuthenticationConfigurations() {
                // default here
                AllExternalUserRoles = ["external-user"]
            };
            authenticationConfigurations?.Invoke(defaultObj);
            builder.Services.AddSingleton<IAuthenticationConfigurations>(defaultObj);


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

                identityOptions?.Invoke(options);
            })
                .AddUserStore<ApplicationUserStore<ApplicationUser>>()
                // TODO: Experiment with how to set 'roles' for membership in future
                .AddRoleStore<NoOpApplicationRoleStore>()
                //.AddRoleStore<ApplicationRoleStore<ApplicationRole>>()

                .AddUserManager<UserManager<ApplicationUser>>()
                .AddSignInManager<SignInManager<ApplicationUser>>();

            builder.Services.AddSingleton(passwordPolicySettings ?? new PasswordPolicySettings(
               usePasswordPolicy: false
               )
           );

            // Get default 
            var authBuilder = builder.Services.AddAuthentication();

            var googleAuth = builder.Configuration.GetSection("Authentication:Google");
            if (googleAuth.Exists())
            {
                authBuilder.AddGoogle("Google", opt =>
                {
                    opt.ClientId = googleAuth["ClientId"] ?? string.Empty;
                    opt.ClientSecret = googleAuth["ClientSecret"] ?? string.Empty;
                    opt.SignInScheme = IdentityConstants.ExternalScheme;
                    opt.EventsType = typeof(SiteSettingsOauthAuthenticationEvents);
                });
            }
            var facebookAuth = builder.Configuration.GetSection("Authentication:Facebook");
            if (facebookAuth.Exists())
            {
                authBuilder.AddFacebook("Facebook", opt =>
                 {
                     opt.AppId = facebookAuth["AppId"] ?? string.Empty;
                     opt.AppSecret = facebookAuth["AppSecret"] ?? string.Empty;
                     opt.SignInScheme = IdentityConstants.ExternalScheme;
                     opt.EventsType = typeof(SiteSettingsFacebookOauthAuthenticationEvents);
                 });
            }
            var twitterAuth = builder.Configuration.GetSection("Authentication:Twitter");
            if(twitterAuth.Exists())
            {
                authBuilder.AddTwitter(opt =>
                {
                    opt.ConsumerKey = twitterAuth["APIKey"];
                    opt.ConsumerSecret = twitterAuth["APIKeySecret"];
                    opt.RetrieveUserDetails = true;
                    opt.EventsType = typeof(SiteSettingsTwitterOauthAuthenticationEvents);
                });
            }
            var microsoftAuth = builder.Configuration.GetSection("Authentication:Microsoft");
            if (microsoftAuth.Exists())
            {
                authBuilder.AddMicrosoftAccount(opt =>
                 {
                     opt.ClientId = microsoftAuth["ClientId"] ?? string.Empty;
                     opt.ClientSecret = microsoftAuth["ClientSecret"] ?? string.Empty;
                     opt.EventsType = typeof(SiteSettingsOauthAuthenticationEvents);
                 });
            }

            builder.Services.AddAuthorization();

            // Register authentication cookie
            // Overwrite login logout based on site settings, with fall back to the default controllers
            builder.Services.AddScoped<SiteSettingsCookieAuthenticationEvents>()
                            .AddScoped<SiteSettingsOauthAuthenticationEvents>()
                            .AddScoped<SiteSettingsFacebookOauthAuthenticationEvents>()
                            .AddScoped<SiteSettingsTwitterOauthAuthenticationEvents>();

            // Configures the application's authentication cookie
            builder.Services.ConfigureApplicationCookie(c =>
            {
                // These 3 are actually handled on the SiteSettingsOauthAuthenticationEvents
                // and are overwritten by site settings
                c.LoginPath = new PathString("/Account/Signin");
                c.LogoutPath = new PathString("/Account/Signout");
                c.AccessDeniedPath = new PathString("/Error/403");

                c.ExpireTimeSpan = TimeSpan.FromDays(14);
                c.SlidingExpiration = true;
                c.Cookie.Name = AUTHENTICATION_COOKIE_NAME;
                c.EventsType = typeof(SiteSettingsCookieAuthenticationEvents);
            });

            builder.Services.Configure<CookieLevelOptions>(options => {
                options.CookieConfigurations.Add(AUTHENTICATION_COOKIE_NAME, CookieLevel.Essential);
            });
            
            return builder;
        }
    }

    public class SiteSettingsCookieAuthenticationEvents(IAccountSettingsRepository _accountSiteSettingsRepository) : CookieAuthenticationEvents
    {

        public override async Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            string url = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync(LogInController.GetUrl());
            string queryString = context.RedirectUri.Contains('?') ? "?" + context.RedirectUri.Split('?')[1] : "";
            context.RedirectUri = url + queryString;
            await base.RedirectToLogin(context);
        }

        public override async Task RedirectToLogout(RedirectContext<CookieAuthenticationOptions> context)
        {
            string url = await _accountSiteSettingsRepository.GetAccountLogOutUrlAsync(LogInController.GetUrl());
            context.RedirectUri = url;
            await base.RedirectToLogout(context);
        }
        public override async Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            string url = await _accountSiteSettingsRepository.GetAccessDeniedUrlAsync(string.Empty);
            if (url.AsNullOrWhitespaceMaybe().TryGetValue(out var accessDeniedUrl))
            {
                context.RedirectUri = accessDeniedUrl;
                await base.RedirectToLogout(context);
            }
            else
            {
                await base.RedirectToLogout(context);
            }
        }
    }

    public class SiteSettingsFacebookOauthAuthenticationEvents(IAccountSettingsRepository _accountSiteSettingsRepository) : OAuthEvents
    {

        public override async Task AccessDenied(AccessDeniedContext context)
        {
            context.AccessDeniedPath = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync(LogInController.GetUrl());
        }
        public override Task RedirectToAuthorizationEndpoint(RedirectContext<OAuthOptions> context)
        {
            if (context.Properties.Parameters.TryGetValue("AuthType", out var authTypeObj) && authTypeObj is string authType)
            {
                context.RedirectUri = QueryHelpers.AddQueryString(context.RedirectUri, "auth_type", authType);
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    }

    public class SiteSettingsOauthAuthenticationEvents(IAccountSettingsRepository _accountSiteSettingsRepository) : OAuthEvents
    {
        public override async Task AccessDenied(AccessDeniedContext context)
        {
            context.ReturnUrl = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync(LogInController.GetUrl());
            //await base.RedirectToLogout(context);
        }

    }

    public class SiteSettingsTwitterOauthAuthenticationEvents(IAccountSettingsRepository _accountSiteSettingsRepository) : TwitterEvents
    {
        public override async Task AccessDenied(AccessDeniedContext context)
        {
            context.AccessDeniedPath = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync(LogInController.GetUrl());
            //await base.RedirectToLogout(context);
        }
    }
}
