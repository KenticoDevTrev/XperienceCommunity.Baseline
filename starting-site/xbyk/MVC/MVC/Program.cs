using Microsoft.AspNetCore.Builder;
using MVC;
using MVC.Configuration;

var builder = WebApplication.CreateBuilder(args);

StartupConfigs.RegisterKenticoServices(builder);

// IIS Hosting only - See https://docs.kentico.com/developers-and-admins/development/website-development-basics/configure-new-projects#configure-application-startup
// builder.Services.Configure<IISApplicationInitializationOptions>(options => {
//     options.UseDefaultSystemResponseForPreload = true;
// });


// Baseline.Core
StartupConfigs.AddBaselineCore(builder);

// Register other interfaces
StartupConfigs.RegisterInterfaces(builder);

// --- CHOOSE EITHER STANDARD OR BASELINE FOR AUTHENTICATION, NOT BOTH ---- //
// Standard Kentico Account / Authorization
//StartupConfigs.AddStandardKenticoAuthenticationAndControllerViews(builder);
// OR
// Baseline Account / Authorization
StartupConfigs.AddBaselineAccountAuthenticationAndControllerViews(builder);

var app = builder.Build();


StartupConfigs.RegisterDotNetCoreConfigurationsAndKentico(app, builder);

RouteConfig.RegisterRoutes(app);

StartupConfigs.RegisterStaticFileHandlingGzipAndCacheControls(builder);

app.Run();