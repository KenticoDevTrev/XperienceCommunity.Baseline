﻿using Core.Repositories;
using Core.Repositories.Implementation;
using Core.Services.Implementation;
using Core.Services;
using Core.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Core.Interfaces;
using Core.Models;

namespace Core
{
    public static class CoreMiddleware
    {
        public static IServiceCollection UseCoreBaseline(this IServiceCollection services,
            Action<ContentItemAssetOptions>? contentItemAssetOptions = null,
            Action<MediaFileOptions>? mediaFileOptions = null,
            Action<ContentItemTaxonomyOptions>? contentItemTaxonomyOptions = null)
        {
            // Configuration Points
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
                services.AddSingleton(new MediaFileOptions());
            }

            if (contentItemTaxonomyOptions != null) {
                var contentItemTaxonomyOption = new ContentItemTaxonomyOptions();
                contentItemTaxonomyOptions.Invoke(contentItemTaxonomyOption);
                services.AddSingleton(contentItemTaxonomyOption);
            } else {
                services.AddSingleton(new ContentItemTaxonomyOptions());
            }

            services
                // Largely Only dependent upon Kentico's APIs
                .AddScoped<ILogger, Logger>()
                .AddScoped<ISiteRepository, SiteRepository>()
                .AddScoped<ILanguageFallbackRepository, LanguageFallbackRepository>()
                .AddScoped<IUrlResolver, UrlResolver>()
                .AddScoped<IBaselinePageBuilderContext, BaselinePageBuilderContext>()
                .AddScoped<IPageIdentityFactory, PageIdentityFactory>()
                .AddScoped<IIdentityService, IdentityService>()
                .AddScoped<ICategoryCachedRepository, CategoryCachedRepository>()

                // Some internal APIs
                .AddScoped<IPageContextRepository, PageContextRepository>()

                // User Customization Points
                .AddScoped<IMediaFileMediaMetadataProvider, MediaFileMediaMetadataProvider>()
                .AddScoped<IContentItemMediaCustomizer, ContentItemMediaCustomizer>()
                .AddScoped<IContentItemMediaMetadataQueryEditor, ContentItemMediaMetadataQueryEditor>()
                .AddScoped<ICustomTaxonomyFieldParser, CustomTaxonomyFieldParser>()
                .AddScoped<IUserMetadataProvider, UserMetadataProvider>()

                // Main item retrieval that depends on baseline apis and user customizations
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IMediaRepository, MediaRepository>()
                .AddScoped<IContentCategoryRepository, ContentCategoryRepository>();

#pragma warning disable CS0618 // Type or member is obsolete
            services.AddScoped<IPageCategoryRepository, PageCategoryRepository>();
#pragma warning restore CS0618 // Type or member is obsolete

            return services;

        }

        public static IServiceCollection AddCoreBaselineKenticoAuthentication(this IServiceCollection services)
        {
                       
            // Required for authentication
            /*
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
            */
            return services;
            
        }
    }
}