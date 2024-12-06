using Account.Repositories.Implementation;
using Account.Services;
using Account.Services.Implementation;
using Kentico.Membership;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.Authorization;

namespace Account
{
    public static class AuthenticationServiceExtensions

    {

        public static IMvcBuilder AddControllersWithViewsWithKenticoAuthorization(this IServiceCollection services)
        {
            return services.AddControllersWithViews(opt => opt.Filters.AddKenticoAuthorization());
        }

        [Obsolete("Use AddBaselineAccountAuthentication")]
        public static IServiceCollection AddKenticoAuthentication(this IServiceCollection services, IConfiguration configuration, string AUTHENTICATION_COOKIE_NAME = "identity.authentication")
        {
            return services.AddBaselineAccountAuthentication<ApplicationUser, ApplicationRole, User>(configuration, AUTHENTICATION_COOKIE_NAME: AUTHENTICATION_COOKIE_NAME);
        }

        public static IServiceCollection AddBaselineAccountAuthentication<TUser, TRole, TGenericUser>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IdentityOptions>? identityOptions = null,
            Action<AuthenticationConfiguration>? authenticationConfigurations = null,
            Action<CookieAuthenticationOptions>? cookieConfigurations = null,
            string AUTHENTICATION_COOKIE_NAME = "identity.authentication",
            string defaultLoginUrl = "/Account/LogIn",
            string defaultLogOutUrl = "/Account/LogOut",
            string defaultAccessDeniedPath = "/Error/403")
            where TUser : ApplicationUser, new()
            where TRole : ApplicationRole, new()
            where TGenericUser : User, new()
        {
            // Register IUserService Fallback with normal User
            services.AddScoped<IUserService<User>, UserService<TUser, User>>()
                .AddScoped<IUserService, UserService<TUser>>();

            var defaultObj = new AuthenticationConfiguration() {
                // default here
                AllExternalUserRoles = ["external-user"]
            };
            authenticationConfigurations?.Invoke(defaultObj);
            services.AddSingleton<IAuthenticationConfigurations>(defaultObj);

            // Required for authentication
            services.AddScoped<IPasswordHasher<ApplicationUser>, Kentico.Membership.PasswordHasher<ApplicationUser>>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddApplicationIdentity<ApplicationUser, ApplicationRole>(options => {
                // Note: These settings are effective only when password policies are turned off in the administration settings.
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 0;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
                identityOptions?.Invoke(options);
            })
                    .AddApplicationDefaultTokenProviders()
                    .AddUserStore<ApplicationUserStore<ApplicationUser>>()
                    .AddRoleStore<ApplicationRoleStore<ApplicationRole>>()
                    .AddUserManager<ApplicationUserManager<ApplicationUser>>()
                    .AddSignInManager<SignInManager<ApplicationUser>>();

            // Get default 
            var authBuilder = services.AddAuthentication();

            var googleAuth = configuration.GetSection("Authentication:Google");
            if (googleAuth.Exists()) {
                authBuilder.AddGoogle("Google", opt => {
                    opt.ClientId = googleAuth["ClientId"] ?? string.Empty;
                    opt.ClientSecret = googleAuth["ClientSecret"] ?? string.Empty;
                    opt.SignInScheme = IdentityConstants.ExternalScheme;
                    opt.EventsType = typeof(SiteSettingsOauthAuthenticationEvents);
                });
            }
            var facebookAuth = configuration.GetSection("Authentication:Facebook");
            if (facebookAuth.Exists()) {
                authBuilder.AddFacebook("Facebook", opt =>
                {
                    opt.AppId = facebookAuth["AppId"] ?? string.Empty;
                    opt.AppSecret = facebookAuth["AppSecret"] ?? string.Empty;
                    opt.SignInScheme = IdentityConstants.ExternalScheme;
                    opt.EventsType = typeof(SiteSettingsFacebookOauthAuthenticationEvents);
                });
            }
            var twitterAuth = configuration.GetSection("Authentication:Twitter");
            if (twitterAuth.Exists()) {
                authBuilder.AddTwitter(opt => {
                    opt.ConsumerKey = twitterAuth["APIKey"];
                    opt.ConsumerSecret = twitterAuth["APIKeySecret"];
                    opt.RetrieveUserDetails = true;
                    opt.EventsType = typeof(SiteSettingsTwitterOauthAuthenticationEvents);
                    opt.CallbackPath = twitterAuth["CallbackPath"] ?? "/signin-twitter";
                });
            }
            var microsoftAuth = configuration.GetSection("Authentication:Microsoft");
            if (microsoftAuth.Exists()) {
                authBuilder.AddMicrosoftAccount(opt =>
                {

                    opt.ClientId = microsoftAuth["ClientId"] ?? string.Empty;
                    opt.ClientSecret = microsoftAuth["ClientSecret"] ?? string.Empty;
                    opt.EventsType = typeof(SiteSettingsOauthAuthenticationEvents);
                    opt.CallbackPath = microsoftAuth["CallbackPath"] ?? "/signin-microsoft";
                });
            }

            // Baseline Configuration of External Authentication
            authBuilder.ConfigureAuthentication(config =>
            {
                config.ExistingInternalUserBehavior = ExistingInternalUserBehavior.SetToExternal;
                config.FacebookUserRoles.Add("facebook-user");
                config.UseTwoFormAuthentication = false;
            });

            services.AddAuthorization();

            // Register authentication cookie
            // Overwrite login logout based on site settings, with fall back to the default controllers
            services.AddScoped<SiteSettingsCookieAuthenticationEvents>()
                            .AddScoped<SiteSettingsOauthAuthenticationEvents>()
                            .AddScoped<SiteSettingsFacebookOauthAuthenticationEvents>()
                            .AddScoped<SiteSettingsTwitterOauthAuthenticationEvents>();

            // Configures the application's authentication cookie
            services.ConfigureApplicationCookie(c => {
                // These 3 are actually handled on the SiteSettingsOauthAuthenticationEvents
                // and are overwritten by site settings
                c.LoginPath = new PathString(defaultLoginUrl);
                c.LogoutPath = new PathString(defaultLogOutUrl);
                c.AccessDeniedPath = new PathString(defaultAccessDeniedPath);
                c.ExpireTimeSpan = TimeSpan.FromDays(14);
                c.SlidingExpiration = true;
                c.EventsType = typeof(SiteSettingsCookieAuthenticationEvents);
                c.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                c.Cookie.SameSite = SameSiteMode.Lax;
                c.Cookie.Name = AUTHENTICATION_COOKIE_NAME;
                c.Cookie.IsEssential = true;

                // Customize
                cookieConfigurations?.Invoke(c);
            });


            CookieHelper.RegisterCookie(AUTHENTICATION_COOKIE_NAME, CookieLevel.Essential);

            return services;
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
            context.AccessDeniedPath = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync("/Account/LogIn");
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
            context.ReturnUrl = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync("/Account/LogIn");
            //await base.RedirectToLogout(context);
        }

    }

    public class SiteSettingsTwitterOauthAuthenticationEvents(IAccountSettingsRepository _accountSiteSettingsRepository) : TwitterEvents
    {
        public override async Task AccessDenied(AccessDeniedContext context)
        {
            context.AccessDeniedPath = await _accountSiteSettingsRepository.GetAccountLoginUrlAsync("/Account/LogIn");
            //await base.RedirectToLogout(context);
        }
    }
}
