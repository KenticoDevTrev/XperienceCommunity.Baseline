using Core.Repositories;
using Core.Repositories.Implementation;
using Core.Services.Implementation;
using Core.Services;
using Core.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Core.Interfaces;
using Core.Models;
using MVCCaching;
using XperienceCommunity.ChannelSettings.Configuration;
using XperienceCommunity.RelationshipsExtended;
using Kentico.Membership;
using Microsoft.AspNetCore.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Http;

namespace Core
{
    public static class CoreMiddleware
    {
        public static IServiceCollection AddCoreBaseline<TUser, TGenericUser>(this IServiceCollection services,
            Action<ContentItemAssetOptions>? contentItemAssetOptions = null,
            Action<MediaFileOptions>? mediaFileOptions = null,
            Action<ContentItemTaxonomyOptions>? contentItemTaxonomyOptions = null,
            Action<RelationshipsExtendedOptions>? relationshipsExtendedOptions = null,
            Action<CookieTempDataProviderOptions>? tempDataCookieConfigurations = null,
            string tempDataCookieName = "TEMPDATA") where TUser : ApplicationUser, new() where TGenericUser : User, new()
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

            // XperienceCommunity.DevTools.RelationshipsExtended
            services.AddRelationshipsExtended((configuration) => {
                configuration.AllowContentItemCategories = true;
                relationshipsExtendedOptions?.Invoke(configuration);
            });


            // Add MVC Caching which Core depends on
            services.AddMVCCaching();

            // Enables Channel Custom Settings which some modules leverage.
            services.AddChannelCustomSettings();

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
                .AddScoped<IModelStateService, ModelStateService>()

                // Some internal APIs
                .AddScoped<IPageContextRepository, PageContextRepository>()

                // User Customization Points
                .AddScoped<IBaselineUserMapper<TUser, TGenericUser>, BaselineUserMapper<TUser, TGenericUser>>()
                .AddScoped<IMediaFileMediaMetadataProvider, MediaFileMediaMetadataProvider>()
                .AddScoped<IContentItemMediaCustomizer, ContentItemMediaCustomizer>()
                .AddScoped<IContentItemMediaMetadataQueryEditor, ContentItemMediaMetadataQueryEditor>()
                .AddScoped<ICustomTaxonomyFieldParser, CustomTaxonomyFieldParser>()
                .AddScoped<IUserMetadataProvider, UserMetadataProvider>()

                // Main item retrieval that depends on baseline apis and user customizations
                .AddScoped<IUserRepository<TGenericUser>, UserRepository<TUser, TGenericUser>>()
                .AddScoped<IMediaRepository, MediaRepository>()
                .AddScoped<IContentCategoryRepository, ContentCategoryRepository>()

                // Add fallback untyped versions for existing code
                .AddScoped<IBaselineUserMapper<TUser, User>, BaselineUserMapper<TUser, User>>()
                .AddScoped<IUserRepository<User>, UserRepository<TUser, User>>()
                .AddScoped<IUserRepository, UserRepository>();

#pragma warning disable CS0618 // Type or member is obsolete
            services.AddScoped<IPageCategoryRepository, PageCategoryRepository>();
#pragma warning restore CS0618 // Type or member is obsolete

            // Needed for the TempData persistance without session
            services.Configure<CookieTempDataProviderOptions>(options => {
                options.Cookie.Name = tempDataCookieName;
                options.Cookie.SameSite = SameSiteMode.Lax;
                tempDataCookieConfigurations?.Invoke(options);
            });
            services.Configure<CookieLevelOptions>(options => {
                options.CookieConfigurations.Add(tempDataCookieName, CookieLevel.Essential);
            });

            return services;

        }
    }
}
