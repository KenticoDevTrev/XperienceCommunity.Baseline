using CMS.Helpers;
using Core.KX13.Repositories.Implementation;
using Core.Repositories.Implementation;
using Core.Services.Implementations;
using Kentico.Membership;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MVC.Middleware;
using Logger = Core.Services.Implementation.Logger;

namespace Core
{
    public static class CoreMiddleware
    {
        [Obsolete("Use AddCoreBaseline<TUser, TGenericUser> with ApplicationUser and User types or your own custom ones")]
        public static IServiceCollection UseCoreBaseline(this IServiceCollection services)
        {
            return services.AddCoreBaseline<ApplicationUser, User>();
        }

        /// <summary>
        /// Configures the Core Baseline
        /// </summary>
        /// <typeparam name="TUser">The User object used to identify the application's site user, usually ApplicationUser unless you extend it</typeparam>
        /// <typeparam name="TGenericUser">The generic User object used to identify the Application's site user, usually User unless you extend it</typeparam>
        /// <param name="services">The service collection</param>
        /// <param name="persistantStorageConfiguration">Configures the cookies for either TempData, Session, or neither.
        /// 
        /// Should specify this if you plan on using the IModelStateService for Post-Redirect-Get or anything regarding TempData or Session.
        /// 
        /// If using session, make sure define and pick your Session Storage mechanism (ex Memory, SQL, Redis Cache, etc) and Add it, and call the app.UseSession() to enable.
        /// </param>
        /// <param name="imageTagHelperOptionsConfiguration">Configures ImageTagHelper.cs img parsing</param>
        /// <returns></returns>
        public static IServiceCollection AddCoreBaseline<TUser, TGenericUser>(this IServiceCollection services,
            IPersistantStorageConfiguration? persistantStorageConfiguration = null,
            Action<MediaTagHelperOptions>? imageTagHelperOptionsConfiguration = null
            ) where TUser : ApplicationUser, new() where TGenericUser : User, new()
        {
            // Add MVC Caching which Core depends on
            services.AddMVCCaching();

            var mediaTagHelperOptions = new MediaTagHelperOptions();
            imageTagHelperOptionsConfiguration?.Invoke(mediaTagHelperOptions);
            services.AddSingleton(mediaTagHelperOptions);

            services
                // Largely Only dependent upon Kentico's APIs
                .AddScoped<ILogger, Logger>()
                .AddScoped<ISiteRepository, SiteRepository>()
                .AddScoped<IUrlResolver, UrlResolver>()
                .AddScoped<IBaselinePageBuilderContext, BaselinePageBuilderContext>()
                .AddScoped<IPageIdentityFactory, PageIdentityFactory>()
                .AddScoped<IIdentityService, IdentityService>()
                .AddScoped<ICategoryCachedRepository, CategoryCachedRepository>()
                .AddScoped<IModelStateService, ModelStateService>()

                // Some internal APIs
                .AddScoped<IPageContextRepository, PageContextRepository>()
                .AddScoped<IMetaDataRepository, MetaDataRepository>()


                // User Customization Points
                .AddScoped<IBaselineUserMapper<TUser>, BaselineUserMapper<TUser>>()
                .AddScoped<IMediaFileMediaMetadataProvider, MediaFileMediaMetadataProvider>()
                .AddScoped<IMetaDataRepository, MetaDataRepository>()

                // Main item retrieval that depends on baseline apis and user customizations
                .AddScoped<IUserRepository, UserRepository<TUser>>()
                .AddScoped<IMediaRepository, MediaRepository>()
                .AddScoped<IContentCategoryRepository, ContentCategoryRepository>();

                // Add fallback untyped versions for existing code

#pragma warning disable CS0618 // Type or member is obsolete
            services.AddScoped<IPageCategoryRepository, PageCategoryRepository>();
#pragma warning restore CS0618 // Type or member is obsolete

            if (persistantStorageConfiguration != null) {
                if (persistantStorageConfiguration is TempDataCookiePersistantStorageConfiguration tempDataConfiguration) {
                    // Needed for the TempData persistance without session
                    services.Configure<CookieTempDataProviderOptions>(options => {
                        options.Cookie = new CookieBuilder() {
                            Name = tempDataConfiguration.TempDataCookieName,
                            HttpOnly = true,
                            SameSite = SameSiteMode.Strict,
                            SecurePolicy = CookieSecurePolicy.Always,
                            Expiration = TimeSpan.FromDays(1),
                            IsEssential = true
                        };
                        tempDataConfiguration.TempDataCookieConfigurations?.Invoke(options);
                    });
                    CookieHelper.RegisterCookie(tempDataConfiguration.TempDataCookieName, CookieLevel.Essential);
                }
                if (persistantStorageConfiguration is SessionPersistantStorageConfiguration sessionPersistantStorageConfiguration) {
                    services.AddSession(options => {
                        options.Cookie = new CookieBuilder() {
                            Name = sessionPersistantStorageConfiguration.SessionCookieName,
                            HttpOnly = true,
                            SameSite = SameSiteMode.Strict,
                            SecurePolicy = CookieSecurePolicy.Always,
                            Expiration = TimeSpan.FromDays(1),
                            IsEssential = true
                        };
                        options.IdleTimeout = TimeSpan.FromMinutes(60);
                        sessionPersistantStorageConfiguration.SessionOptionsConfigurations?.Invoke(options);
                    });
                    CookieHelper.RegisterCookie(sessionPersistantStorageConfiguration.SessionCookieName, CookieLevel.Essential);

                }
            }

            return services;

        }

        public static IServiceCollection AddCoreBaselineKenticoAuthentication(this IServiceCollection services)
        {
            // Required for authentication
            services.AddScoped<IPasswordHasher<ApplicationUser>, Kentico.Membership.PasswordHasher<ApplicationUser>>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddApplicationIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Note: These settings are effective only when password policies are turned off in the administration settings.
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 0;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
            })
            .AddApplicationDefaultTokenProviders()
            .AddUserStore<ApplicationUserStore<ApplicationUser>>()
            .AddRoleStore<ApplicationRoleStore<ApplicationRole>>()
            .AddUserManager<ApplicationUserManager<ApplicationUser>>()
            .AddSignInManager<SignInManager<ApplicationUser>>();

            return services;
        }

        /// <summary>
        /// Registers Core Baseline Middleware
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCoreBaseline(this IApplicationBuilder app)
        {
            return app.UseMiddleware<PageBuilderModelStateClearer>();
        }
    }
}
