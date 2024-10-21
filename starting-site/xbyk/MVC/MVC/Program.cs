using CMS.Base.Configuration;
using Generic;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Web.Mvc;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MVC;
using System;
using Testing;
using XperienceCommunity.RelationshipsExtended;
using XperienceCommunity.RelationshipsExtended.Models;
using MVCCaching;
using CMS.Core;
using Core.Services;
using Core.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);


// Enable desired Kentico Xperience features
builder.Services.AddKentico(features =>
{
    features.UsePageBuilder(new PageBuilderOptions {
        ContentTypeNames = new[]
        {
            // Enables Page Builder for content types using their generated classes
            Home.CONTENT_TYPE_NAME,
            BasicPage.CONTENT_TYPE_NAME
        },
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


// IIS Hosting only - See https://docs.kentico.com/developers-and-admins/development/website-development-basics/configure-new-projects#configure-application-startup
// builder.Services.Configure<IISApplicationInitializationOptions>(options => {
//     options.UseDefaultSystemResponseForPreload = true;
// });

builder.Services.AddAuthentication();
// builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

// see https://docs.kentico.com/developers-and-admins/configuration/content-hub-configuration
builder.Services.Configure<FileUploadOptions>(options => {
    // Sets the maximum file size to 500 MB
    options.MaxFileSize = 524288000L;
    // Sets the chunk size to 20 MB
    options.ChunkSize = 20971520;
});

builder.Services.AddRelationshipsExtended((configuration) => {
    configuration.AllowContentItemCategories = true;
    configuration.AllowLanguageSyncConfiguration = true;
    var langSyncConfigs = new List<LanguageSyncClassConfiguration>() {
        new LanguageSyncClassConfiguration(WebPage.CONTENT_TYPE_NAME, [
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

//builder.Services.UseCoreBaseline();

var app = builder.Build();

// Must be first!
app.InitKentico();

app.UseStaticFiles();

app.UseCookiePolicy();

// Needed for Page Builder, not just normal auth
app.UseAuthentication();

// Only for Saas
// app.useKenticoCloud()

// Enables Kentico middleware and configuration
app.UseKentico();


// app.UseAuthorization();

//RouteConfig.RegisterRoutes(app);

// Adds system routes such as HTTP handlers and feature-specific routes
app.Kentico().MapRoutes();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Only enable until home page is set
// app.MapGet("/", () => "The MVC site has not been configured yet.");


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

builder.WebHost.UseStaticWebAssets();

app.Run();
