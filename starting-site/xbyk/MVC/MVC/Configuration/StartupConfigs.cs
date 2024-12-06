﻿using CMS.Base.Configuration;
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
using Account.Admin.Xperience.Models;
using Kentico.Xperience.Admin.Base;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Core.Middleware;
using XperienceModels.Classes.Membership;
using XperienceCommunity.MemberRoles.Models;
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
        public static void RegisterKenticoServices(WebApplicationBuilder builder)
        {
            // Enable desired Kentico Xperience features
            builder.Services.AddKentico(features => {
                features.UsePageBuilder(new PageBuilderOptions {
                    ContentTypeNames =
                    [
                        // Enables Page Builder for content types using their generated classes
                        Home.CONTENT_TYPE_NAME,
                        BasicPage.CONTENT_TYPE_NAME
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

            // Override Baseline customization points if wanted
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
        /// Use this if using the Baseline Account Module to hook up Member Roles, Authorization, and Logins
        /// </summary>
        /// <param name="builder"></param>
        public static void AddBaselineAccountAuthenticationAndControllerViews(WebApplicationBuilder builder)
        {
            // If you wish to hook up the various Microsoft.AspNetCore.Authentication types (Facebook, Google, MicrosoftAccount, Twitter),
            // please see this documentation https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-8.0&tabs=visual-studio and update your AppSettings.json with the IDs

            builder.AddBaselineAccountAuthentication<ApplicationUserWithNames, TagApplicationUserRole, UserExtended>(
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

            // If you are leveraging the Account.RCL controllers and such, use this to hook up the validation.  If you are using your own, then disregard.
            builder.AddBaselineAccountRcl<UserExtended>();
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

            // OPTIONAL - Enable Session Session
            // EnableSession(app, builder);

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
        /// Sample on how to enable Session State
        /// </summary>
        /// <param name="app"></param>
        /// <param name="builder"></param>
        public static void EnableSession(IApplicationBuilder app, WebApplicationBuilder builder)
        {

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options => {
                options.Cookie = new CookieBuilder() {
                    Name = "SessionId",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    SecurePolicy = CookieSecurePolicy.Always,
                    Expiration = TimeSpan.FromDays(1),
                    IsEssential = true
                };
                options.IdleTimeout = TimeSpan.FromMinutes(20);
            });

            // Enables session - Needed for Account Post-Redirect-Get Export Model State Logic
            app.UseSession();

        }

        public static void AddBaselineCore(WebApplicationBuilder builder)
        {
            // Modify as needed
            builder.Services.AddCoreBaseline<ApplicationUserWithNames, UserExtended>(relationshipsExtendedOptions: (configuration) => {
                configuration.AllowLanguageSyncConfiguration = true;

                var langSyncConfigs = new List<LanguageSyncClassConfiguration>() {
                    new (WebPage.CONTENT_TYPE_NAME, [
                        nameof(WebPage.TestLanguageAgnosticValue),
                        nameof(WebPage.TestObjectNames)
                        ])
                };
                configuration.LanguageSyncConfiguration = new LanguageSyncConfiguration(langSyncConfigs, []);
            });
        }
    }

    public record UserExtended : User
    {
        public UserExtended()
        {
        }

        public UserExtended(string userName, string email, bool enabled, bool isExternal, bool isPublic) : base(userName, email, enabled, isExternal, isPublic)
        {
        }

        public UserExtended(string userName, string firstName, string lastName, string email, bool enabled, bool isExternal, bool isPublic) : base(userName, firstName, lastName, email, enabled, isExternal, isPublic)
        {
        }

        public UserExtended(int userID, string userName, Guid userGUID, string email, bool enabled, bool isExternal, bool isPublic = false) : base(userID, userName, userGUID, email, enabled, isExternal, isPublic)
        {
        }

        public UserExtended(int userID, string userName, Guid userGUID, string email, string firstName, string middleName, string lastName, bool enabled, bool isExternal, bool isPublic = false) : base(userID, userName, userGUID, email, firstName, middleName, lastName, enabled, isExternal, isPublic)
        {
        }

        protected UserExtended(User original) : base(original)
        {
        }
    }
}
