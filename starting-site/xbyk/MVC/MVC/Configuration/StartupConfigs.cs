// Kentico
using CMS.Base.Configuration;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Web.Mvc;
using Kentico.Membership;
using Kentico.Xperience.Admin.Base;

// Microsoft
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

// Your Site
using Testing;
using Site.Repositories.Implementations;
using MVC.Repositories.Implementations;

// Core
using Core;
using Core.Middleware;
using Core.Repositories;

// Core used libraries
using MVCCaching;
using XperienceCommunity.RelationshipsExtended.Models;
using XperienceCommunity.MemberRoles.Models;
using XperienceCommunity.Localizer;

// BASELINE CUSTOMIZATION: Account - Added Usings
using Account.Models;
using Account.Admin.Xperience.Models;

// BASELINE CUSTOMIZATION: Navigation - Added Usings
using Navigation.Repositories;
using Navigation.Features.PartialNavigation;

// BASELINE CUSTOMIZATION: TabbedPages - Add Using
using TabbedPages.Features.Tab;
using TabbedPages.Features.TabParent;
using Generic;

// BASELINE CUSTOMIZATION: Account - Add this to edit Channel Settings
[assembly: UIPage(parentType: typeof(Kentico.Xperience.Admin.Base.UIPages.ChannelEditSection),
                slug: "member-password-channel-custom-settings",
                uiPageType: typeof(MemberPasswordChannelSettingsExtender),
                name: "Member Password Settings",
                templateName: TemplateNames.EDIT,
                order: UIPageOrder.NoOrder)]

// BASELINE CUSTOMIZATION: Navigation - Add this for the Navigation Mega Menu Support
[assembly: RegisterPageTemplate(
    "Generic.Navigation_Default",
    "Navigation",
    typeof(NavigationPageTemplateProperties),
    "/Features/Navigation/PartialNavigation/NavigationPageTemplate.cshtml",
    ContentTypeNames = [Generic.Navigation.CONTENT_TYPE_NAME])]

// BASELINE CUSTOMIZATION: TabbedPages - Add this for the Navigation Mega Menu Support
[assembly: RegisterPageTemplate(
    "Generic.Tab_Default",
    "Tab",
    typeof(TabPageTemplateProperties),
    "/Features/Tab/TabPageTemplate.cshtml",
    ContentTypeNames = [Tab.CONTENT_TYPE_NAME])]

[assembly: RegisterPageTemplate(
    "Generic.TabParent_Default",
    "Tab Parent",
    typeof(TabParentPageTemplateProperties),
    "/Features/TabParent/TabParentPageTemplate.cshtml",
    ContentTypeNames = [TabParent.CONTENT_TYPE_NAME])]

namespace MVC.Configuration
{
    public static class StartupConfigs
    {
        /// <summary>
        /// BASELINE CUSTOMIZATION: Starting Site - Adjust your Kentico features to your site
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterKenticoServices(WebApplicationBuilder builder)
        {
            // Enable desired Kentico Xperience features
            builder.Services.AddKentico(features => {
                features.UsePageBuilder(new PageBuilderOptions {
                    ContentTypeNames =
                    [
                        // Enables Page Builder for content types using their generated classes

                        // BASELINE CUSTOMIZATION: Starting Site - If you wish to use the Home and Basic Pages, MUST add it here
                        Generic.Home.CONTENT_TYPE_NAME,
                        Generic.BasicPage.CONTENT_TYPE_NAME,

                        // BASELINE CUSTOMIZATION: Navigation - If using Navigation content type, MUST add it here
                        Generic.Navigation.CONTENT_TYPE_NAME,

                        // BASELINE CUSTOMIZATION: Account - If using Accounts with Account Type, MUST add it here
                        Generic.Account.CONTENT_TYPE_NAME,

                        // BASELINE CUSTOMIZATION: TabbedPages - If using TabbedPages, MUST add it here
                        Generic.TabParent.CONTENT_TYPE_NAME,
                        Generic.Tab.CONTENT_TYPE_NAME

                    ],
                    // Specifies a default section for the page builder feature
                    // If you install BootstrapLayoutTool.PageBuilderContainered.Kentico.MVC.Core package on MVC, you can use below for bootstrap layout tool
                    // DefaultSectionIdentifier = Bootstrap4LayoutToolProperties.IDENTITY,
                    // Disables the system's built-in 'Default' section
                    RegisterDefaultSection = true
                });
                // features.UseActivityTracking();
                features.UseWebPageRouting();
                // features.UseEmailStatisticsLogging();
                // features.UseEmailMarketing();
                // feature.UseCrossSiteTracking();
            });

            builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            // see https://docs.kentico.com/developers-and-admins/configuration/content-hub-configuration
            builder.Services.Configure<FileUploadOptions>(options => {
                // Sets the maximum file size to 500 MB
                options.MaxFileSize = 524288000L;
                // Sets the chunk size to 20 MB
                options.ChunkSize = 20971520;
            });
        }

        /// <summary>
        /// BASELINE CUSTOMIZATION: Starting Site - If you want to use Session, set your own Session storage method below
        /// BASELINE CUSTOMIZATION: Core - Make sure if using Session, add the appropriate IPersistantStorageConfiguration to use Session to the AddCoreBaseline extension.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="builder"></param>
        public static void AddSession(WebApplicationBuilder builder)
        {
            // You can use whichever storage method you desire
            // https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-8.0
            builder.Services.AddDistributedMemoryCache();

            // builder.Services.AddSession Configuration should be configured by passing the IPersistantStorageConfiguration to the AddCoreBaseline extension.
            // Below is what the Core Baseline will ultimately do:
            /* FOR REFERENCE ONLY, DON'T COMMENT OUT
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
             */
        }

        /// <summary>
        /// BASELINE CUSTOMIZATION: Core - Configure the below to your site specifications
        /// </summary>
        /// <param name="builder"></param>
        public static void AddBaselineCore<TUser>(WebApplicationBuilder builder) where TUser : ApplicationUser, new()
        {
            // Modify as needed
            // NOTE: While you create your own ApplicationUser variant, if you use a Generic User Model other than Core.Model.User as your User type,
            // then you will NOT be able to leverage the Baseline.Account.RCL, and will need to clone that project, as you have to inject the
            // IUserService<User> and IUserRepository<User> of the same type you define here, it won't work if the types miss-matched
            // Additionally, if you use a model other than ApplicationUserWithNames, you'll want to implement and register your own 
            var baselineInstallerOptions = new BaselineCoreInstallerOptions(
                AddMemberFields: true,
                AddHomePageType: true,
                AddBasicPageType: true,
                AddMediaPageTypes: true,
                ImageFormatsSupported: "jpg;jpeg;webp;gif;png;apng;bmp;ico;avif", // svg excluded by default due to security, you can add in: https://docs.kentico.com/developers-and-admins/configuration/content-hub-configuration#support-for-svg-images
                VideoFormatsSupported: "mp4;webm;ogg;ogv;avi;wmv",
                AudioFormatsSupported: "txt;pdf;docx;pptx;xlsx;zip"
                );

            builder.Services.AddCoreBaseline<TUser>(
                installerOptions: baselineInstallerOptions,
                contentItemAssetOptionsConfiguration: (options) => {
                    // If AddMediaPageTypes is true on installer, have these configurations set
                    options.ContentItemConfigurations.Add(new ContentItemAssetOptions.ContentItemWithAssetsConfiguration("Generic.Image", [
                        new ContentItemAssetOptions.AssetFieldIdentifierConfiguration("ImageFile", Core.Enums.ContentItemAssetMediaType.Image, "ImageTitle", "ImageDescription")
                    ], preCache: true));

                    options.ContentItemConfigurations.Add(new ContentItemAssetOptions.ContentItemWithAssetsConfiguration("Generic.Audio", [
                        new ContentItemAssetOptions.AssetFieldIdentifierConfiguration("AudioFile", Core.Enums.ContentItemAssetMediaType.Audio, "AudioTitle", "AudioDescription")
                    ], preCache: true));

                    options.ContentItemConfigurations.Add(new ContentItemAssetOptions.ContentItemWithAssetsConfiguration("Generic.Video", [
                        new ContentItemAssetOptions.AssetFieldIdentifierConfiguration("VideoFile", Core.Enums.ContentItemAssetMediaType.Video, "VideoTitle", "VideoDescription")
                    ], preCache: true));

                    options.ContentItemConfigurations.Add(new ContentItemAssetOptions.ContentItemWithAssetsConfiguration("Generic.File", [
                        new ContentItemAssetOptions.AssetFieldIdentifierConfiguration("FileFile", Core.Enums.ContentItemAssetMediaType.File, "FileTitle", "FileDescription")
                    ], preCache: true));
                },
                mediaFileOptionsConfiguration: (options) => {

                },
                contentItemTaxonomyOptionsConfiguration: (options) => {

                },
                relationshipsExtendedOptionsConfiguration: (options) => {
                    options.AllowLanguageSyncConfiguration = true;

                    var langSyncConfigs = new List<LanguageSyncClassConfiguration>() {
                        new (WebPage.CONTENT_TYPE_NAME, [
                            nameof(WebPage.TestLanguageAgnosticValue),
                            nameof(WebPage.TestObjectNames)
                            ])
                    };
                    options.LanguageSyncConfiguration = new LanguageSyncConfiguration(langSyncConfigs, []);
                },
                metaDataOptionsConfiguration: (options) => {

                },
                persistantStorageConfiguration: new TempDataCookiePersistantStorageConfiguration("TEMPDATA", (configurations) => {
                    // Configure TempData Cookie
                })
            );

            /// BASELINE CUSTOMIZATION: Starting Site - Add your own Page metadata Converter here
            builder.Services.AddScoped<IWebPageToPageMetadataConverter, CustomWebPageToPageMetadataConverter>();
        }

        /// <summary>
        /// BASELINE CONFIGURATION: Starting Site - Add any interfaces and other services here
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterInterfaces(WebApplicationBuilder builder)
        {
            // XperienceCommunity.DevTools.MVCCaching
            // builder.Services.AddMVCCaching() added in AddCoreBaseline()
            builder.Services.AddMVCCachingAutoDependencyInjectionByAttribute();

            // Optional - Add up IUrlHelper if you wish to use it
            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.Services.AddScoped(x => {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                if (actionContext == null) {
                    // Should never really occur
                    return new UrlHelper(new ActionContext());
                }
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });

            // Page Builder context
            builder.Services.AddPageBuilderContext();

            // Widget Filters
            //builder.Services.AddWidgetFilter();

            // BASELINE CONFIGURATION: Core - Override Baseline customization points if wanted
            /*
            builder.Services.AddScoped<IUserMetadataProvider, CustomUserMetadataProvider>();
            builder.Services.AddScoped<IMediaFileMediaMetadataProvider, CustomMediaFileMediaMetadataProvider>();
            builder.Services.AddScoped<IContentItemMediaCustomizer, CustomContentItemMediaCustomizer>();
            builder.Services.AddScoped<IContentItemMediaMetadataQueryEditor, CustomContentItemMediaMetadataQueryEditor>();
            builder.Services.AddScoped<ICustomTaxonomyFieldParser, CustomCustomTaxonomyFieldParser>();
            */
        }

        /// <summary>
        /// Registers Localization, Controllers with Views, and Authorization filters
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="addXperienceCommunityLocalization">Adds the Localization Keys module in the Admin and hooks it up to the default IStringLocalizer/IHtmlLocalizer</param>
        /// <param name="addAuthorizationFilters">This will add the XperienceCommunity.Authorization system to be able to control authorization on controllers and page templates</param>
        /// <param name="localizationResourceType">The resource type for your default IStringLocalizer/IHtmlLocalizer</param>
        public static void AddLocalizationAndControllerViews(WebApplicationBuilder builder, bool addXperienceCommunityLocalization, bool addAuthorizationFilters, Type localizationResourceType)
        {
            if (addAuthorizationFilters) {
                // Member Role Authorization (XperienceCommunity.Authorization)
                builder.Services.AddKenticoAuthorization();
            }

            if (addXperienceCommunityLocalization) {
                builder.Services.AddXperienceCommunityLocalization();
            } else {
                builder.Services.AddLocalization();
            }

            var mvcBuilder = addAuthorizationFilters ?
                                builder.Services.AddControllersWithViewsWithKenticoAuthorization()
                                : builder.Services.AddControllersWithViews();

            mvcBuilder.AddViewLocalization()
                        .AddDataAnnotationsLocalization(options => {
                            options.DataAnnotationLocalizerProvider = (type, factory) => {
                                return factory.Create(localizationResourceType);
                            };
                        });
        }

        /// <summary>
        /// Adds Baseline Localization.
        /// </summary>
        /// <param name="builder"></param>
        public static void AddBaselineLocalization(WebApplicationBuilder builder)
        {
            builder.Services.AddBaselineLocalization();
        }

        /// <summary>
        /// Adds Baseline Localization.
        /// </summary>
        /// <param name="builder"></param>
        public static void AddBaselineTabbedPages(WebApplicationBuilder builder)
        {
            builder.Services.AddTabbedPages();
        }


        /// <summary>
        /// Adds the standard Kentico identity (based roughly off of the Dancing Goat Sample Site)
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TRole"></typeparam>
        /// <param name="builder"></param>
        public static void AddStandardKenticoIdentity<TUser, TRole>(WebApplicationBuilder builder) where TUser : ApplicationUser, new() where TRole : TagApplicationUserRole, new()
        {
            // Adds and configures ASP.NET Identity for the application
            // XperienceCommunity.MemberRoles, make sure Role is TagApplicationUserRole or an inherited member here
            builder.Services.AddIdentity<TUser, TRole>(options => {
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
                .AddUserStore<ApplicationUserStore<TUser>>()
                .AddMemberRolesStores<TUser, TRole>() // XperienceCommunity.MemberRoles
                .AddUserManager<UserManager<TUser>>()
                .AddSignInManager<SignInManager<TUser>>();

            // Adds authorization support to the app
            builder.Services.ConfigureApplicationCookie(options => {
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
                options.LoginPath = new PathString("/Account/Signin");
                options.AccessDeniedPath = new PathString("/Error/403");
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    var factory = ctx.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
                    var urlHelper = factory.GetUrlHelper(new ActionContext(ctx.HttpContext, new RouteData(ctx.HttpContext.Request.RouteValues), new ActionDescriptor()));
                    var url = urlHelper.Action("Signin", "Account") + new Uri(ctx.RedirectUri).Query;

                    ctx.Response.Redirect(url);

                    return Task.CompletedTask;
                };
                // These are not part of standard kentico but 
                if(builder.Environment.IsDevelopment()) {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                } else {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                } 
            });

            builder.Services.Configure<AdminIdentityOptions>(options =>
            {
                // The expiration time span for admin. In production environment, set expiration according to best practices.
                options.AuthenticationOptions.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });

            builder.Services.AddAuthorization();
        }

        /// <summary>
        /// BASELINE CUSTOMIZATION: Account - Configure this method for your own uses
        /// Use this if using the Baseline Account Module to hook up Member Roles, Authorization, and Logins
        /// </summary>
        /// <param name="builder"></param>
        public static void AddBaselineAccountIdentity<TUser, TRole>(WebApplicationBuilder builder) where TUser : ApplicationUser, new() where TRole : TagApplicationUserRole, new()
        {
            // If you wish to hook up the various Microsoft.AspNetCore.Authentication types (Facebook, Google, MicrosoftAccount, Twitter),
            // please see this documentation https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-8.0&tabs=visual-studio and update your AppSettings.json with the IDs

            // NOTE: While you create your own ApplicationUser variant, if you use a Generic User Model other than Core.Model.User as your User type,
            // then you will NOT be able to leverage the Baseline.Account.RCL, and will need to clone that project, as you have to inject the
            // IUserService<User> and IUserRepository<User> of the same type you define here, it won't work if the types miss-matched
            builder.AddBaselineAccountAuthentication<TUser, TRole>(
            identityOptions => {
                // Customize IdentityOptions, including Password Settings (fall back if no ChannelSettings is defined
                identityOptions.Password.RequireDigit = false;
                identityOptions.Password.RequireLowercase = false;
                identityOptions.Password.RequireNonAlphanumeric = false;
                identityOptions.Password.RequireUppercase = false;
                identityOptions.Password.RequiredLength = 0;
            },
            authenticationConfigurations => {
                // Customize Baseline's Authentication settings, including two form and additional roles per Auth Type
                authenticationConfigurations.UseTwoFormAuthentication = true;
            },
            cookieAuthenticationOptions => {
                // Customize Cookie Auth Options
                // Note that the LoginPath/LogoutPath/AccessDeniedPath are handled in SiteSettingsCookieAuthenticationEvents
                // These are not part of standard kentico but 
                if (builder.Environment.IsDevelopment()) {
                    cookieAuthenticationOptions.Cookie.SecurePolicy = CookieSecurePolicy.None;
                    cookieAuthenticationOptions.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                } else {
                    cookieAuthenticationOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    cookieAuthenticationOptions.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                }
            }
            );

            /* To define Channel Specific Password Settings, add the below assembly tag somewhere, then in Xperience Admin select your Channel, the side menu will show these settings configurations */
            /*
             [assembly: UIPage(parentType: typeof(Kentico.Xperience.Admin.Base.UIPages.ChannelEditSection),
                slug: "member-password-channel-custom-settings",
                uiPageType: typeof(MemberPasswordChannelSettingsExtender),
                name: "Member Password Settings",
                templateName: TemplateNames.EDIT,
                order: UIPageOrder.NoOrder)]
            */

            // If you are leveraging the Account.RCL controllers and such, use this to hook up the validation, and optionally installs the Generic.Account WebPage (will still need to add the RegisterPageTemplates, see AssemblyTags.md)
            builder.AddBaselineAccountRcl(new BaselineAccountOptions(UseAccountWebpageType: true));

            builder.Services.Configure<AdminIdentityOptions>(options => {
                // The expiration time span for admin. In production environment, set expiration according to best practices.
                options.AuthenticationOptions.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });

            builder.Services.AddAuthorization();
        }

        /// <summary>
        /// Adds the Baseline Navigation, along with your Custom Dynamic Navigation Repository.
        /// </summary>
        /// <param name="builder"></param>
        public static void AddBaselineNavigation(WebApplicationBuilder builder)
        {
            // BASELINE CUSTOMIZATION: Navigation - Add navigation and configure here
            builder.Services.AddBaselineNavigation(new Navigation.Models.BaselineNavigationOptions() {
                ShowPagesNotTranslatedInSitemapUrlSet = false
            });

            // Add my repo for dynamic nav
            builder.Services.AddScoped<IDynamicNavigationRepository, CustomDynamicNavigationRepository>();
        }

        /// <summary>
        /// Enables Gzip compression for js and css resources.
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterGzipAndCacheControls(WebApplicationBuilder builder)
        {
            // Gzip compression handling for js.gz, css.gz and map.gz files and setting CacheControl headers based on ?v parameter detection
            builder.Services.UseGzipAndCacheControlFileHandling();
        }

        /// <summary>
        /// Registers Baseline Core middleware, which is only one item that can go at the beginning/end of the pipeline.
        /// </summary>
        /// <param name="app"></param>
        public static void RegisterBaselineCoreMiddleware(WebApplication app)
        {
            app.UseCoreBaseline();
        }

        /// <summary>
        /// BASELINE CUSTOMIZATION - Starting Site - Configure Middleware to your liking
        /// </summary>
        /// <param name="app"></param>
        /// <param name="builder"></param>
        public static void RegisterDotNetCoreConfigurationsAndKentico(IApplicationBuilder app, WebApplicationBuilder builder)
        {
            // Must be first!
            app.InitKentico();

            // Allow Static Files
            app.UseStaticFiles();

            // Make sure to call the middleware in the provided order
            app.UseCookiePolicy();

            // Only for Saas
            // app.useKenticoCloud()

            // Needed for Page Builder, not just normal auth
            app.UseAuthentication();

            // Only for Saas
            // app.useKenticoCloud()

            // Enables Kentico middleware and configuration
            app.UseKentico();

            // Enables Authorization, used in admin too
            app.UseAuthorization();

            if (!builder.Environment.IsDevelopment()) {
                app.UseHsts();
            }

            //////////////////////////////
            //////// ERROR HANDLING //////
            //////////////////////////////

            // Standard HttpError handling
            // See Features/HttpErrors/HttpErrorsController.cs
            // TODO: This breaks things due too admin, so will need to figure out if can resolve.

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            if (builder.Environment.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // BizStream's Status Code Pages
            // See Features/HttpErrors/XperienceStausCodePage.cs
            // app.UseXperienceStatusCodePages();

            //////////////////////////////
            //////// ERROR HANDLING //////
            //////////////////////////////
        }

        /// <summary>
        /// BASELINE CUSTOMIZATION: Starting Site - If you want to use Session, call this extension
        /// </summary>
        /// <param name="app"></param>
        /// <param name="builder"></param>
        public static void UseSession(IApplicationBuilder app)
        {
            app.UseSession();
        }

        
    }
}
