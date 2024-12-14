using CMS.Base.Configuration;
using Generic;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MVCCaching;
using XperienceCommunity.RelationshipsExtended.Models;
using Testing;
using Core;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using Kentico.Xperience.Admin.Base;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Core.Middleware;
using XperienceCommunity.MemberRoles.Models;

// BASELINE CUSTOMIZATION: Account Module - Added Usings
using Account.Models;
using Account.Admin.Xperience.Models;
using Core.Repositories;
using Site.Repositories.Implementations;
using MVC.NewFolder;
using Navigation.Extensions;

// BASELINE CUSTOMIZATION: Account Module - Add this to edit Channel Settings
[assembly: UIPage(parentType: typeof(Kentico.Xperience.Admin.Base.UIPages.ChannelEditSection),
                slug: "member-password-channel-custom-settings",
                uiPageType: typeof(MemberPasswordChannelSettingsExtender),
                name: "Member Password Settings",
                templateName: TemplateNames.EDIT,
                order: UIPageOrder.NoOrder)]

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
                        Home.CONTENT_TYPE_NAME,
                        BasicPage.CONTENT_TYPE_NAME,

                        // BASELINE CUSTOMIZATION: Navigation - If using Navigation content type, MUST add it here
                        Generic.Navigation.CONTENT_TYPE_NAME,

                        // BASELINE CUSTOMIZATION: Account - If using Accounts with Account Type, MUST add it here
                        Generic.Account.CONTENT_TYPE_NAME,
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

            // see https://docs.kentico.com/developers-and-admins/configuration/content-hub-configuration
            builder.Services.Configure<FileUploadOptions>(options => {
                // Sets the maximum file size to 500 MB
                options.MaxFileSize = 524288000L;
                // Sets the chunk size to 20 MB
                options.ChunkSize = 20971520;
            });
        }

        /// <summary>
        /// BASELINE CUSTOMIZATION: Core - Configure the below to your site specifications
        /// </summary>
        /// <param name="builder"></param>
        public static void AddBaselineCore(WebApplicationBuilder builder)
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

            builder.Services.AddCoreBaseline<ApplicationUserBaseline>(
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

            // Gzip compression handling for js.gz, css.gz and map.gz files and setting CacheControl headers based on ?v parameter detection
            builder.Services.UseGzipAndCacheControlFileHandling();

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

        public static void AddStandardKenticoAuthenticationAndControllerViews(WebApplicationBuilder builder)
        {
            // Adds Basic Kentico Authentication, needed for user context and some tools
            builder.Services.AddAuthentication();

            // Adds and configures ASP.NET Identity for the application

            // XperienceCommunity.MemberRoles, make sure Role is TagApplicationUserRole or an inherited member here
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
            })
                .AddUserStore<ApplicationUserStore<ApplicationUser>>()
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddSignInManager<SignInManager<ApplicationUser>>();

            // Adds authorization support to the app
            builder.Services.ConfigureApplicationCookie(options => {
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
                options.LoginPath = new PathString("/Account/Signin");
                options.AccessDeniedPath = new PathString("/Error/403");
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });
            builder.Services.AddAuthorization();

            // Localizer
            builder.Services.AddLocalization()
                    //.AddXperienceLocalizer() // Call after AddLocalization
                    .AddControllersWithViews()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization(options => {
                        options.DataAnnotationLocalizerProvider = (type, factory) => {
                            return factory.Create(typeof(SharedResources));
                        };
                    });
        }

        /// <summary>
        /// BASELINE CUSTOMIZATION: Account - Configure this method for your own uses
        /// Use this if using the Baseline Account Module to hook up Member Roles, Authorization, and Logins
        /// </summary>
        /// <param name="builder"></param>
        public static void AddBaselineAccountAuthenticationAndControllerViews(WebApplicationBuilder builder)
        {
            

            // If you wish to hook up the various Microsoft.AspNetCore.Authentication types (Facebook, Google, MicrosoftAccount, Twitter),
            // please see this documentation https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-8.0&tabs=visual-studio and update your AppSettings.json with the IDs

            // NOTE: While you create your own ApplicationUser variant, if you use a Generic User Model other than Core.Model.User as your User type,
            // then you will NOT be able to leverage the Baseline.Account.RCL, and will need to clone that project, as you have to inject the
            // IUserService<User> and IUserRepository<User> of the same type you define here, it won't work if the types miss-matched
            builder.AddBaselineAccountAuthentication<ApplicationUserBaseline, TagApplicationUserRole>(
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

            // Localizer
            builder.Services.AddLocalization()
                    //.AddXperienceLocalizer() // Call after AddLocalization
                    .AddControllersWithViewsWithKenticoAuthorization()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization(options => {
                        options.DataAnnotationLocalizerProvider = (type, factory) => {
                            return factory.Create(typeof(SharedResources));
                        };
                    });

            // If you are leveraging the Account.RCL controllers and such, use this to hook up the validation, and optionally installs the Generic.Account WebPage (will still need to add the RegisterPageTemplates, see AssemblyTags.md)
            builder.AddBaselineAccountRcl(new BaselineAccountOptions(UseAccountWebpageType: true));
        }

        public static void RegisterStaticFileHandlingGzipAndCacheControls(WebApplicationBuilder builder)
        {

            // While IIS and IIS Express automatically handle StaticFiles from the root, default Kestrel doesn't, so safer to 
            // add this for any Site Media Libraries if you ever plan on linking directly to the file.  /getmedia linkes are not
            // impacted. 
            //
            // Also, if you ever need to bypass the IIS/IIS Express default StaticFile handling, you can add a web.config in the media folder 
            // with the below:
            // <?xml version="1.0"?>
            // <configuration>
            //     <system.webServer>
            //         <handlers>
            //             <add name="ForceStaticFileHandlingToNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Either" requireAccess="Read" />
            //         </handlers>
            //     </system.webServer>
            // </configuration>
            /*
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Baseline")),
                    RequestPath = "/Baseline"
                });
            */

            builder.WebHost.UseStaticWebAssets();
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

            // While IIS and IIS Express automatically handle StaticFiles from the root, default Kestrel doesn't, so safer to 
            // add this for any Site Media Libraries if you ever plan on linking directly to the file.  /getmedia linkes are not
            // impacted. 
            //
            // Also, if you ever need to bypass the IIS/IIS Express default StaticFile handling, you can add a web.config in the media folder 
            // with the below:
            // <?xml version="1.0"?>
            // <configuration>
            //     <system.webServer>
            //         <handlers>
            //             <add name="ForceStaticFileHandlingToNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Either" requireAccess="Read" />
            //         </handlers>
            //     </system.webServer>
            // </configuration>
            /*
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Baseline")),
                    RequestPath = "/Baseline"
                });
            */

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


            if (builder.Environment.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            //////////////////////////////
            //////// ERROR HANDLING //////
            //////////////////////////////

            // Standard HttpError handling
            // See Features/HttpErrors/HttpErrorsController.cs
            // TODO: This breaks things due too admin, so will need to figure out if can resolve.
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            if (!builder.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/error/500");
                app.UseHsts();
            }

            // BizStream's Status Code Pages
            // See Features/HttpErrors/XperienceStausCodePage.cs
            // app.UseXperienceStatusCodePages();

            //////////////////////////////
            //////// ERROR HANDLING //////
            //////////////////////////////
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
        /// BASELINE CUSTOMIZATION: Starting Site - If you want to use Session, call this extension
        /// </summary>
        /// <param name="app"></param>
        /// <param name="builder"></param>
        public static void UseSession(IApplicationBuilder app)
        {
            app.UseSession();
        }

        public static void RegisterBaselineCoreMiddleware(WebApplication app)
        {
            app.UseCoreBaseline();
        }

        public static void AddBaselineNavigation(WebApplicationBuilder builder)
        {
            builder.Services.AddBaselineNavigation(new Navigation.Models.BaselineNavigationOptions() {
                ShowPagesNotTranslatedInSitemapUrlSet = false
            });
        }
    }
}
