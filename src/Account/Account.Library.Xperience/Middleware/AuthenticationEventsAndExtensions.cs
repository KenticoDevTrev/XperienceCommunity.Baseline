using Account.Repositories;
using Account.Repositories.Implementations;
using Account.Services;
using Account.Services.Implementations;
using Core.Models;
using Kentico.Membership;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.Authorization;
using XperienceCommunity.MemberRoles.Models;

namespace Microsoft.AspNetCore.Builder
{
    public static class AuthenticationServiceExtensions

    {

        public static IMvcBuilder AddControllersWithViewsWithKenticoAuthorization(this IServiceCollection services)
        {
            return services.AddControllersWithViews(options => {
                options.Filters.AddKenticoAuthorizationFilters(); // XperienceCommunity.DevTools.Authorization
            });
        }

        public static WebApplicationBuilder AddBaselineAccountAuthentication<TUser, TRole, TGenericUser>(
            this WebApplicationBuilder builder,
            Action<IdentityOptions>? identityOptions = null,
            Action<AuthenticationConfigurations>? authenticationConfigurations = null,
            Action<CookieAuthenticationOptions>? cookieConfigurations = null,
            string AUTHENTICATION_COOKIE_NAME = "identity.authentication",
            string defaultLoginUrl = "/Account/LogIn",
            string defaultLogOutUrl = "/Account/LogOut",
            string defaultAccessDeniedPath = "/Error/403") 
            where TUser : ApplicationUser, new() 
            where TRole : TagApplicationUserRole, new() 
            where TGenericUser : User, new()
        {
            // Register IUserService Fallback with normal User
            builder.Services.AddScoped<IUserService<User>, UserService<TUser, User>>()
                .AddScoped<IUserService, UserService<TUser>>();

            // Register DI
            builder.Services.AddScoped<IAccountSettingsRepository, AccountSettingsRepository>()
                .AddScoped<IRoleRepository, RoleRepository<TUser, TRole>>()
                .AddScoped<IRoleService, RoleService<TUser, TRole>>()
                .AddScoped<ISignInManagerService, SignInManagerService<TUser>>()
                .AddScoped<IUserManagerService, UserManagerService<TUser>>()
                .AddScoped<IUserService<TGenericUser>, UserService<TUser, TGenericUser>>()

                // Add Generic Fallback
                .AddScoped<IUserService, UserService<TUser>>();

            // Baseline Configuration of External Authentication
            var defaultObj = new AuthenticationConfigurations() {
                // default here
                AllExternalUserRoles = ["external-user"]
            };
            authenticationConfigurations?.Invoke(defaultObj);
            builder.Services.AddSingleton<IAuthenticationConfigurations>(defaultObj);

            // Adds Basic Kentico Authentication, needed for user context and some tools
            builder.Services.AddAuthentication();

            // Adds and configures ASP.NET Identity for the application
            builder.Services.AddIdentity<TUser, TRole>(options => {
                // Ensures that disabled member accounts cannot sign in
                options.SignIn.RequireConfirmedAccount = true;

                // Ensures unique emails for registered accounts
                options.User.RequireUniqueEmail = true;

                identityOptions?.Invoke(options);
            })
                .AddDefaultTokenProviders()
                .AddUserStore<ApplicationUserStore<TUser>>()
                .AddMemberRolesStores<TUser, TRole>() // XperienceCommunity.MemberRoles
                .AddUserManager<UserManager<TUser>>()
                .AddSignInManager<SignInManager<TUser>>();

            // Get default 
            var authBuilder = builder.Services.AddAuthentication();

            var googleAuth = builder.Configuration.GetSection("Authentication:Google");
            if (googleAuth.Exists()) {
                authBuilder.AddGoogle("Google", opt => {
                    opt.ClientId = googleAuth["ClientId"] ?? string.Empty;
                    opt.ClientSecret = googleAuth["ClientSecret"] ?? string.Empty;
                    opt.SignInScheme = IdentityConstants.ExternalScheme;
                    opt.EventsType = typeof(SiteSettingsOauthAuthenticationEvents);
                });
            }
            var facebookAuth = builder.Configuration.GetSection("Authentication:Facebook");
            if (facebookAuth.Exists()) {
                authBuilder.AddFacebook("Facebook", opt => {
                    opt.AppId = facebookAuth["AppId"] ?? string.Empty;
                    opt.AppSecret = facebookAuth["AppSecret"] ?? string.Empty;
                    opt.SignInScheme = IdentityConstants.ExternalScheme;
                    opt.EventsType = typeof(SiteSettingsFacebookOauthAuthenticationEvents);
                });
            }
            var twitterAuth = builder.Configuration.GetSection("Authentication:Twitter");
            if (twitterAuth.Exists()) {
                authBuilder.AddTwitter(opt => {
                    opt.ConsumerKey = twitterAuth["APIKey"];
                    opt.ConsumerSecret = twitterAuth["APIKeySecret"];
                    opt.RetrieveUserDetails = true;
                    opt.EventsType = typeof(SiteSettingsTwitterOauthAuthenticationEvents);
                    opt.CallbackPath = twitterAuth["CallbackPath"] ?? "/signin-twitter";
                });
            }
            var microsoftAuth = builder.Configuration.GetSection("Authentication:Microsoft");
            if (microsoftAuth.Exists()) {
                authBuilder.AddMicrosoftAccount(opt => {
                    opt.ClientId = microsoftAuth["ClientId"] ?? string.Empty;
                    opt.ClientSecret = microsoftAuth["ClientSecret"] ?? string.Empty;
                    opt.EventsType = typeof(SiteSettingsOauthAuthenticationEvents);
                    opt.CallbackPath = microsoftAuth["CallbackPath"] ?? "/signin-microsoft";
                });
            }

            // Kentico's normal Authorization
            builder.Services.AddAuthorization();

            // Member Role Authorization (XperienceCommunity.Authorization)
            builder.Services.AddKenticoAuthorization();

            // Register authentication cookie
            // Overwrite login logout based on site settings, with fall back to the default controllers
            builder.Services.AddScoped<SiteSettingsCookieAuthenticationEvents>()
                            .AddScoped<SiteSettingsOauthAuthenticationEvents>()
                            .AddScoped<SiteSettingsFacebookOauthAuthenticationEvents>()
                            .AddScoped<SiteSettingsTwitterOauthAuthenticationEvents>();

            // Configures the application's authentication cookie
            builder.Services.ConfigureApplicationCookie(c => {
                // These 3 are actually handled on the SiteSettingsOauthAuthenticationEvents
                // and are overwritten by site settings
                c.LoginPath = new PathString(defaultLoginUrl);
                c.LogoutPath = new PathString(defaultLogOutUrl);
                c.AccessDeniedPath = new PathString(defaultAccessDeniedPath);
                c.ExpireTimeSpan = TimeSpan.FromDays(1);
                c.SlidingExpiration = true;
                c.EventsType = typeof(SiteSettingsCookieAuthenticationEvents);
                c.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                c.Cookie.SameSite = SameSiteMode.Lax;
                c.Cookie.Name = AUTHENTICATION_COOKIE_NAME;
                c.Cookie.IsEssential = true;
                // Customize
                cookieConfigurations?.Invoke(c);
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
            string url = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync(context.Options.LoginPath);
            string queryString = context.RedirectUri.Contains('?') ? "?" + context.RedirectUri.Split('?')[1] : "";
            context.RedirectUri = url + queryString;
            await base.RedirectToLogin(context);
        }

        public override async Task RedirectToLogout(RedirectContext<CookieAuthenticationOptions> context)
        {
            string url = await _accountSiteSettingsRepository.GetAccountLogOutUrlAsync(context.Options.LoginPath);
            context.RedirectUri = url;
            await base.RedirectToLogout(context);
        }
        public override async Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            string url = await _accountSiteSettingsRepository.GetAccessDeniedUrlAsync(context.Options.AccessDeniedPath.HasValue ? context.Options.AccessDeniedPath.Value : string.Empty);
            if (url.AsNullOrWhitespaceMaybe().TryGetValue(out var accessDeniedUrl)) {
                context.RedirectUri = accessDeniedUrl;
                await base.RedirectToLogout(context);
            } else {
                await base.RedirectToLogout(context);
            }
        }
    }

    public class SiteSettingsFacebookOauthAuthenticationEvents(IAccountSettingsRepository _accountSiteSettingsRepository) : OAuthEvents
    {

        public override async Task AccessDenied(AccessDeniedContext context)
        {
            context.AccessDeniedPath = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync("/Account/LogIn");
        }
        public override Task RedirectToAuthorizationEndpoint(RedirectContext<OAuthOptions> context)
        {
            if (context.Properties.Parameters.TryGetValue("AuthType", out var authTypeObj) && authTypeObj is string authType) {
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
            context.ReturnUrl = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync("/Account/LogIn");
            //await base.RedirectToLogout(context);
        }

    }

    public class SiteSettingsTwitterOauthAuthenticationEvents(IAccountSettingsRepository _accountSiteSettingsRepository) : OAuthEvents
    {
        public override async Task AccessDenied(AccessDeniedContext context)
        {
            context.AccessDeniedPath = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync("/Account/LogIn");
            //await base.RedirectToLogout(context);
        }
    }
}
