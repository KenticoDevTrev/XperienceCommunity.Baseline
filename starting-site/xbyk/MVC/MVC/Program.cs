using Microsoft.AspNetCore.Builder;
using MVC;
using MVC.Configuration;

var builder = WebApplication.CreateBuilder(args);

// IIS Hosting only - See https://docs.kentico.com/developers-and-admins/development/website-development-basics/configure-new-projects#configure-application-startup
// builder.Services.Configure<IISApplicationInitializationOptions>(options => {
//     options.UseDefaultSystemResponseForPreload = true;
// });

StartupConfigs.RegisterKenticoServices(builder);

StartupConfigs.AddAuthentication(builder);

StartupConfigs.RegisterLocalizationAndControllerViews(builder);

StartupConfigs.RegisterInterfaces(builder);

var app = builder.Build();

StartupConfigs.RegisterDotNetCoreConfigurationsAndKentico(app, builder);

RouteConfig.RegisterRoutes(app);

StartupConfigs.RegisterStaticFileHandling(builder);

app.Run();
