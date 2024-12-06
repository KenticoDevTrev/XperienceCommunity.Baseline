using CMS.Helpers;
using Core.KX13.Repositories.Implementation;
using Core.Repositories.Implementation;
using Core.Services.Implementations;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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

        public static IServiceCollection AddCoreBaseline<TUser, TGenericUser>(this IServiceCollection services,
            Action<CookieTempDataProviderOptions>? tempDataCookieConfigurations = null,
            string tempDataCookieName = "TEMPDATA"
            ) where TUser : ApplicationUser, new() where TGenericUser : User, new()
        {
            // Add MVC Caching which Core depends on
            services.AddMVCCaching();

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

                // User Customization Points
                .AddScoped<IBaselineUserMapper<TUser, TGenericUser>, BaselineUserMapper<TUser, TGenericUser>>()
                .AddScoped<IMediaFileMediaMetadataProvider, MediaFileMediaMetadataProvider>()

                // Main item retrieval that depends on baseline apis and user customizations
                .AddScoped<IUserRepository<TGenericUser>, UserRepository<TUser, TGenericUser>>()
                .AddScoped<IMediaRepository, MediaRepository>()
                .AddScoped<IContentCategoryRepository, ContentCategoryRepository>()

                // Add fallback untyped versions for existing code
                .AddScoped<IBaselineUserMapper<TUser, User>, BaselineUserMapper<TUser, User>>()
                .AddScoped<IUserRepository<User>, UserRepository<TUser, User>>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IMetaDataRepository, MetaDataRepository>();

#pragma warning disable CS0618 // Type or member is obsolete
            services.AddScoped<IPageCategoryRepository, PageCategoryRepository>();
#pragma warning restore CS0618 // Type or member is obsolete

            // Temp Data Cookie necessary for the TempData to persis with the ModelState logic
            services.Configure<CookieTempDataProviderOptions>(options => {
                options.Cookie.Name = tempDataCookieName;
                options.Cookie.SameSite = SameSiteMode.Lax;
                tempDataCookieConfigurations?.Invoke(options);
            });

            CookieHelper.RegisterCookie(tempDataCookieName, CookieLevel.Essential);

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
    }
}
