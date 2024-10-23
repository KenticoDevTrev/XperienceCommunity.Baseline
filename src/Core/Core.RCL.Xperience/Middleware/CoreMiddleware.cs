using Core.Repositories;
using Core.Repositories.Implementation;
using Core.Services.Implementation;
using Core.Services;
using Core.Services.Implementations;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Core.Interfaces;
using Core.Models;
using CMS.ContentEngine;

namespace Core
{
    public static class CoreMiddleware
    {
        public static IServiceCollection UseCoreBaseline(this IServiceCollection services, Action<ContentItemAssetOptions>? contentItemAssetOptions = null, Action<MediaFileOptions>? mediaFileOptions = null)
        {
            services.AddScoped<IBaselinePageBuilderContext, BaselinePageBuilderContext>()
                .AddScoped<ICategoryCachedRepository, CategoryCachedRepository>()
                .AddScoped<IMediaRepository, MediaRepository>()
                //.AddScoped<IMetaDataRepository, MetaDataRepository>()
                //.AddScoped<IPageCategoryRepository, PageCategoryRepository>()
                //.AddScoped<IPageContextRepository, PageContextRepository>()
                //.AddScoped<ISiteRepository, SiteRepository>()
                //.AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IIdentityService, IdentityService>()
                //.AddScoped<ILogger, Logger>()
                //.AddScoped<IPageIdentityFactory, PageIdentityFactory>()
                .AddScoped<IUrlResolver, UrlResolver>()
                .AddScoped<IMediaFileMediaMetadataProvider, MediaFileMediaMetadataProvider>()
                .AddScoped<IContentItemMediaCustomizer, ContentItemMediaCustomizer>()
                .AddScoped<IContentItemMediaMetadataQueryEditor, ContentItemMediaMetadataQueryEditor>();

            if (contentItemAssetOptions != null) {
                var contentItemAssetOption = new ContentItemAssetOptions();
                contentItemAssetOptions.Invoke(contentItemAssetOption);
                services.AddSingleton(contentItemAssetOption);
            } else {
                services.AddSingleton(new ContentItemAssetOptions());
            }

            if (mediaFileOptions != null) {
                var mediaFileOption = new MediaFileOptions();
                mediaFileOptions.Invoke(mediaFileOption);
                services.AddSingleton(mediaFileOptions);
            } else {
                services.AddSingleton(new ContentItemAssetOptions());
            }

            return services;

        }

        public static IServiceCollection AddCoreBaselineKenticoAuthentication(this IServiceCollection services)
        {
            return services;
            /*
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
            })
            .AddApplicationDefaultTokenProviders()
            .AddUserStore<ApplicationUserStore<ApplicationUser>>()
            .AddRoleStore<ApplicationRoleStore<ApplicationRole>>()
            .AddUserManager<ApplicationUserManager<ApplicationUser>>()
            .AddSignInManager<SignInManager<ApplicationUser>>();

            return services;
            */
        }
    }
}
