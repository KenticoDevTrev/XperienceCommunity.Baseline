using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using MVC;
using MVC.Configuration;

var builder = WebApplication.CreateBuilder(args);

StartupConfigs.RegisterKenticoServices(builder);

// IIS Hosting only - See https://docs.kentico.com/developers-and-admins/development/website-development-basics/configure-new-projects#configure-application-startup
// builder.Services.Configure<IISApplicationInitializationOptions>(options => {
//     options.UseDefaultSystemResponseForPreload = true;
// });

// //////////////////////////////////////////
// /////////  Authentication (Member) ///////
// //////////////////////////////////////////
// 
// XperienceCommunity.Baseline.Account.RCL.Xperience - If using Member Logins on web site
builder.AddBaselineKenticoAuthentication(identityOptions => {

    // Can customize things here, the below are the default but you get the idea

    // Ensures that disabled member accounts cannot sign in
    identityOptions.SignIn.RequireConfirmedAccount = true;
    // Ensures unique emails for registered accounts
    identityOptions.User.RequireUniqueEmail = true;

    identityOptions.Password.RequireDigit = true;
    identityOptions.Password.RequireNonAlphanumeric = true;
    identityOptions.Password.RequiredLength = 10;
    identityOptions.Password.RequireUppercase = true;
    identityOptions.Password.RequireLowercase = true;
    identityOptions.Password.RequiredUniqueChars = 3;
},
// At this time can't modify Web Channel Settings to make this dynamic, so need to set manually
passwordPolicySettings: new Account.Models.PasswordPolicySettings(
    usePasswordPolicy: true,
    minLength: 10,
    numNonAlphanumericChars: 1,
    regex: null,
    violationMessage: "Invalid Password")
);

// //////////////////////////////////////////
// /////////  Authentication (Member) ///////
// //////////////////////////////////////////

// Use below if not using the AddBaselineKenticoAuthentication
// StartupConfigs.AddStandardKenticoAuthentication(builder);

StartupConfigs.RegisterLocalizationAndControllerViews(builder);

StartupConfigs.RegisterInterfaces(builder);

var app = builder.Build();

StartupConfigs.RegisterDotNetCoreConfigurationsAndKentico(app, builder);

RouteConfig.RegisterRoutes(app);

StartupConfigs.RegisterStaticFileHandling(builder);

app.Run();