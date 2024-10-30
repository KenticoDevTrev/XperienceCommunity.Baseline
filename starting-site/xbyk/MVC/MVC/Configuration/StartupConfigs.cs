using CMS.Base.Configuration;
using Core.Services.Implementations;
using Core.Services;
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
using Core.Interfaces;

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
            builder.Services.AddRelationshipsExtended((configuration) => {
                configuration.AllowContentItemCategories = true;
                configuration.AllowLanguageSyncConfiguration = true;
                var langSyncConfigs = new List<LanguageSyncClassConfiguration>() {
                    new (WebPage.CONTENT_TYPE_NAME, [
                        nameof(WebPage.TestLanguageAgnosticValue),
                        nameof(WebPage.TestObjectNames)
                        ])
                };
                configuration.LanguageSyncConfiguration = new LanguageSyncConfiguration(langSyncConfigs, []);

            });

            builder.Services.AddMVCCaching();
            builder.Services.AddMVCCachingAutoDependencyInjectionByAttribute();

            // Baseline services
            builder.Services.AddScoped<IUrlResolver, UrlResolver>();

            builder.Services.UseCoreBaseline();

            // Override Baseline customization points if wanted
            /*
            builder.Services.AddScoped<IUserMetadataProvider, CustomUserMetadataProvider>();
            builder.Services.AddScoped<IMediaFileMediaMetadataProvider, CustomMediaFileMediaMetadataProvider>();
            builder.Services.AddScoped<IContentItemMediaCustomizer, CustomContentItemMediaCustomizer>();
            builder.Services.AddScoped<IContentItemMediaMetadataQueryEditor, CustomContentItemMediaMetadataQueryEditor>();
            builder.Services.AddScoped<ICustomTaxonomyFieldParser, CustomCustomTaxonomyFieldParser>();
            */
        }

        public static void AddAuthentication(WebApplicationBuilder builder, string AUTHENTICATION_COOKIE_NAME = "identity.authentication")
        {
            // Adds Basic Kentico Authentication, needed for user context and some tools
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
        }

        public static void RegisterLocalizationAndControllerViews(WebApplicationBuilder builder)
        {
            // Localizer
            builder.Services.AddLocalization()
                    //.AddXperienceLocalizer() // Call after AddLocalization
                    .AddControllersWithViews() // .AddControllersWithViewsAndKenticoAuthorization()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization(options => {
                        options.DataAnnotationLocalizerProvider = (type, factory) => {
                            return factory.Create(typeof(SharedResources));
                        };
                    });
        }


        public static void RegisterStaticFileHandling(WebApplicationBuilder builder)
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

            if (builder.Environment.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/error/500");
                app.UseHsts();
            }

            //////////////////////////////
            //////// ERROR HANDLING //////
            //////////////////////////////

            // Standard HttpError handling
            // See Features/HttpErrors/HttpErrorsController.cs
            // TODO: This breaks things due too admin, so will need to figure out if can resolve.
            // app.UseStatusCodePagesWithReExecute("/error/{0}");

            // BizStream's Status Code Pages
            // See Features/HttpErrors/XperienceStausCodePage.cs
            // app.UseXperienceStatusCodePages();

            //////////////////////////////
            //////// ERROR HANDLING //////
            //////////////////////////////

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
        }
    }
}
