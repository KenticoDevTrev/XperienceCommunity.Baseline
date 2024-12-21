using CMS.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MVC;
using MVC.Configuration;
using MVC.Resources;
using XperienceCommunity.MemberRoles.Models;

var builder = WebApplication.CreateBuilder(args);

StartupConfigs.RegisterKenticoServices(builder);

// IIS Hosting only - See https://docs.kentico.com/developers-and-admins/development/website-development-basics/configure-new-projects#configure-application-startup
// builder.Services.Configure<IISApplicationInitializationOptions>(options => {
//     options.UseDefaultSystemResponseForPreload = true;
// });

// BASELINECONFIGURATION: Starting Site - If you want to use Session, enable below (and configure)
// StartupConfigs.AddSession(builder);

// Baseline.Core
StartupConfigs.AddBaselineCore<ApplicationUserBaseline>(builder);

// Register other interfaces your site will use.
StartupConfigs.RegisterInterfaces(builder);

// Register Controllers, optionally 
StartupConfigs.AddLocalizationAndControllerViews(builder, 
    addXperienceCommunityLocalization: true, 
    addAuthorizationFilters: true,
    localizationResourceType: typeof(MySiteResources)
    );

// Baseline.Localization (currently only contains Category Localization interface)
StartupConfigs.AddBaselineLocalization(builder);

// Baseline.TabbedPages (currently only ITabRepository)
StartupConfigs.AddBaselineTabbedPages(builder);

// Baseline.Search
StartupConfigs.AddBaselineSearch(builder);

// BASELINECONFIGURATION: Account - CHOOSE EITHER STANDARD OR BASELINE FOR AUTHENTICATION, NOT BOTH
// Standard Kentico Account
// StartupConfigs.AddStandardKenticoIdentity<ApplicationUserBaseline, TagApplicationUserRole>(builder);
// OR
// Baseline Account
StartupConfigs.AddBaselineAccountIdentity<ApplicationUserBaseline, TagApplicationUserRole>(builder);

// Baseline.Navigation
StartupConfigs.AddBaselineNavigation(builder);

if (builder.Environment.IsDevelopment()) {
    builder.Services.Configure<UrlResolveOptions>(options => options.UseSSL = false);
}

StartupConfigs.RegisterGzipAndCacheControls(builder);

var app = builder.Build();

StartupConfigs.RegisterBaselineCoreMiddleware(app);

StartupConfigs.RegisterDotNetCoreConfigurationsAndKentico(app, builder);

// BASELINECONFIGURATION: Starting Site - If you wish to use session, enable below
// StartupConfigs.UseSession(app);

RouteConfig.RegisterRoutes(app);

app.Run();