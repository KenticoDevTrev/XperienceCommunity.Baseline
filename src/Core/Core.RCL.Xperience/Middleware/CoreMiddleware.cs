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
using Core.Installers;
using Microsoft.AspNetCore.Builder;
using PartialWidgetPage;
using XperienceCommunity.ImageProcessing;
using MVC.Middleware;
using Core.Middleware;

namespace Core
{
    public static class CoreMiddleware
    {
        /// <summary>
        /// Configures the Core Baseline
        /// </summary>
        /// <typeparam name="TUser">The User object used to identify the application's site user, usually ApplicationUser unless you extend it</typeparam>
        /// <param name="services">The service collection</param>
        /// <param name="contentItemAssetOptionsConfiguration">Configures the Content Item Assets (for media retrieval), needed for custom Title and Description retrieval.</param>
        /// <param name="mediaFileOptionsConfiguration">Configuration of the Media File Options (for media retrieval), needed for Media Library configurations</param>
        /// <param name="contentItemTaxonomyOptionsConfiguration">Configures what Taxonomy fields exist by content type, needed for the various Page Category retrieval and parsing</param>
        /// <param name="relationshipsExtendedOptionsConfiguration">Configures the Relationships Extended tool that is used by the Baseline</param>
        /// <param name="metaDataOptionsConfiguration">Configures the Metadata retrieval logic that is used by the Baseline</param>
        /// <param name="imageProcessingOptionsConfiguration">Configures XperienceCommunity.ImageProcessing</param>
        /// <param name="imageTagHelperOptionsConfiguration">Configures ImageTagHelper.cs img parsing</param>
        /// <param name="persistantStorageConfiguration">Configures the cookies for either TempData, Session, or neither.
        /// 
        /// Should specify this if you plan on using the IModelStateService for Post-Redirect-Get or anything regarding TempData or Session.
        /// 
        /// If using session, make sure define and pick your Session Storage mechanism (ex Memory, SQL, Redis Cache, etc) and Add it, and call the app.UseSession() to enable.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddCoreBaseline<TUser>(this IServiceCollection services,
            BaselineCoreInstallerOptions installerOptions,
            Action<ContentItemAssetOptions>? contentItemAssetOptionsConfiguration = null,
            Action<MediaFileOptions>? mediaFileOptionsConfiguration = null,
            Action<ContentItemTaxonomyOptions>? contentItemTaxonomyOptionsConfiguration = null,
            Action<RelationshipsExtendedOptions>? relationshipsExtendedOptionsConfiguration = null,
            Action<MetadataOptions>? metaDataOptionsConfiguration = null,
            Action<ImageProcessingOptions>? imageProcessingOptionsConfiguration = null,
            Action<MediaTagHelperOptions>? imageTagHelperOptionsConfiguration = null,
            IPersistantStorageConfiguration? persistantStorageConfiguration = null
            ) where TUser : ApplicationUser, new()
        {
            // Configuration Points
            var contentItemAssetOptions = new ContentItemAssetOptions();
            contentItemAssetOptionsConfiguration?.Invoke(contentItemAssetOptions);
            services.AddSingleton(contentItemAssetOptions);

            var mediaFileOptions = new MediaFileOptions();
            mediaFileOptionsConfiguration?.Invoke(mediaFileOptions);
            services.AddSingleton(mediaFileOptions);

            var contentItemTaxonomyOption = new ContentItemTaxonomyOptions();
            contentItemTaxonomyOptionsConfiguration?.Invoke(contentItemTaxonomyOption);
            services.AddSingleton(contentItemTaxonomyOption);

            var metadataOptions = new MetadataOptions();
            metaDataOptionsConfiguration?.Invoke(metadataOptions);
            services.AddSingleton(metadataOptions);

            var mediaTagHelperOptions = new MediaTagHelperOptions();
            imageTagHelperOptionsConfiguration?.Invoke(mediaTagHelperOptions);
            services.AddSingleton(mediaTagHelperOptions);

            // XperienceCommunity.DevTools.RelationshipsExtended
            services.AddRelationshipsExtended((configuration) => {
                configuration.AllowContentItemCategories = true;
                relationshipsExtendedOptionsConfiguration?.Invoke(configuration);
            });

            // Add MVC Caching which Core depends on
            services.AddMVCCaching();

            // Enables Channel Custom Settings which some modules leverage.
            services.AddChannelCustomSettings();

            // Used by Tabs and Navigation modules
            services.AddPartialWidgetPage();

            // XperienceCommunity.ImageProcessing
            var imageProcessingOptions = new ImageProcessingOptions() { ProcessContentItemAssets = true, ProcessMediaLibrary = true };
            imageProcessingOptionsConfiguration?.Invoke(imageProcessingOptions);
            services.AddSingleton(imageProcessingOptions);

            services
                // Largely Only dependent upon Kentico's APIs
                .AddScoped<ILogger, Logger>()
                .AddScoped<ISiteRepository, SiteRepository>()
                .AddScoped<ILanguageRepository, LanguageRepository>()
                .AddScoped<IUrlResolver, UrlResolver>()
                .AddScoped<IBaselinePageBuilderContext, BaselinePageBuilderContext>()
                .AddScoped<IPageIdentityFactory, PageIdentityFactory>()
                .AddScoped<IIdentityService, IdentityService>()
                .AddScoped<ICategoryCachedRepository, CategoryCachedRepository>()
                .AddScoped<IModelStateService, ModelStateService>()
                .AddScoped<IContentItemLanguageMetadataRepository, ContentItemLanguageMetadataRepository>()

                // Some internal APIs, some are transient so they can be used by Search Indexers concerning Metadata and the metadata thumbnail retrieval.
                .AddTransient<IContentTypeRetriever, ContentTypeRetriever>()
                .AddTransient<IContentTranslationInformationRepository, ContentTranslationInformationRepository>()
                .AddTransient<ILanguageIdentifierRepository, LanguageIdentifierRepository>()
                .AddTransient<IContentItemReferenceService, ContentItemReferenceService>()
                .AddTransient<IClassContentTypeAssetConfigurationRepository, ClassContentTypeAssetConfigurationRepository>()
                .AddTransient<IMetaDataWebPageDataContainerConverter, MetaDataWebPageDataContainerConverter>()
                .AddScoped<IPageContextRepository, PageContextRepository>()
                .AddScoped<IMappedContentItemRepository, MappedContentItemRepository>()
                .AddScoped<IMetaDataRepository, MetaDataRepository>();

            // User Customization Points, register your own versions afterwards if you wish
            services
                .AddScoped<IBaselineUserMapper<TUser>, BaselineUserMapper<TUser>>()
                .AddScoped<IMediaFileMediaMetadataProvider, MediaFileMediaMetadataProvider>()
                .AddScoped<IContentItemMediaCustomizer, ContentItemMediaCustomizer>()
                .AddScoped<IContentItemMediaMetadataQueryEditor, ContentItemMediaMetadataQueryEditor>()
                .AddScoped<ICustomTaxonomyFieldParser, CustomTaxonomyFieldParser>()
                .AddScoped<IWebPageToPageMetadataConverter, DefaultWebPageToPageMetadataConverter>()

                // Main item retrieval that depends on baseline apis and user customizations
                .AddScoped<IUserRepository, UserRepository<TUser>>()
                .AddScoped<IMediaRepository, MediaRepository>()
                .AddScoped<IMediaTagHelperService, MediaTagHelperService>()
                .AddScoped<IContentCategoryRepository, ContentCategoryRepository>();

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
                    services.Configure<CookieLevelOptions>(options => {
                        options.CookieConfigurations.Add(tempDataConfiguration.TempDataCookieName, CookieLevel.Essential);
                    });
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
                    services.Configure<CookieLevelOptions>(options => {
                        options.CookieConfigurations.Add(sessionPersistantStorageConfiguration.SessionCookieName, CookieLevel.Essential);
                    });
                }
            }

            // Force the member fields if the type is or inherits from ApplicationUserBaseline
            if (!installerOptions.AddMemberFields && (typeof(TUser).IsSubclassOf(typeof(ApplicationUserBaseline)) || typeof(TUser) == typeof(ApplicationUserBaseline))) {
                installerOptions = installerOptions with { AddMemberFields = true };
            }
            services.AddSingleton(installerOptions);
            services.AddSingleton<BaselineModuleInstaller>();

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

        /// <summary>
        /// Registers Core Baseline Middleware
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCoreBaselineEnd(this IApplicationBuilder app)
        {
            return app.UseXperienceCommunityImageProcessing()
                .UseMiddleware<BaseRedirectMiddleware>()
                .UseCustomVaryByHeaders();
        }


    }
}
