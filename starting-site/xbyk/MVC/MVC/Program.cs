using Microsoft.AspNetCore.Builder;
using MVC;
using MVC.Configuration;
using XperienceCommunity.ChannelSettings.Configuration;

var builder = WebApplication.CreateBuilder(args);

StartupConfigs.RegisterKenticoServices(builder);

builder.Services.AddChannelCustomSettings();

// IIS Hosting only - See https://docs.kentico.com/developers-and-admins/development/website-development-basics/configure-new-projects#configure-application-startup
// builder.Services.Configure<IISApplicationInitializationOptions>(options => {
//     options.UseDefaultSystemResponseForPreload = true;
// });


// --- CHOOSE EITHER STANDARD OR BASELINE, NOT BOTH ---- //

// /////////////////////////////////////////////////
// /// Standard Kentico Account / Authorization  ///
// /////////////////////////////////////////////////

//StartupConfigs.AddStandardKenticoAuthentication(builder);
//StartupConfigs.RegisterStandardKenticoLocalizationAndControllerViews(builder);

// /////////////////////////////////////////////////
// /// Standard Kentico Account / Authorization  ///
// /////////////////////////////////////////////////

// OR

// /////////////////////////////////////////
// ///  Baseline Account / Authorization ///
// /////////////////////////////////////////

StartupConfigs.AddBaselineKenticoAuthentication(builder);
StartupConfigs.RegisterBaselineAccountLocalizationAndControllerViews(builder);

// /////////////////////////////////////////
// ///  Baseline Account / Authorization ///
// /////////////////////////////////////////


StartupConfigs.RegisterInterfaces(builder);

var app = builder.Build();

StartupConfigs.RegisterDotNetCoreConfigurationsAndKentico(app, builder);

RouteConfig.RegisterRoutes(app);

StartupConfigs.RegisterStaticFileHandling(builder);

app.Run();