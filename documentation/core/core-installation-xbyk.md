# Baseline Core Installation (Xperience by Kentico)

These are the instructions for the Baseline Core Mode Installation.  The Starter Sites already have everything installed, so if you are using the Starting Site, you can largely skip this.

If you are installing this on your own instance, I would highly recommend analyzing the Starting Site's [Program.cs](../starting-site/xbyk/MVC/Program.cs) and [StartupConfigs.cs](../starting-site/xbyk/MVC/Configuration/StartupConfigs.cs) for inspiration.

## 1. Nuget Packages

The Baseline Core packages are highly modularized, and each library has different dependencies, however all of them boil down to these two packages:

`XperienceCommunity.Baseline.Core.RCL.Xperience` and `XperienceCommunity.Baseline.Core.Admin.Xperience`.

on your project, run:

```
npm install XperienceCommunity.Baseline.Core.RCL.Xperience
npm install XperienceCommunity.Baseline.Core.Admin.Xperience
```

Please see [the detailed overview of all nuget packages and dependencies](../general/modules-architecture-overview.md) if you wish to install packages on other projects.

## 2. CI/CD Addition

In your startup, add the following bits of code:

```csharp

// Modify as needed
// NOTE: While you create your own ApplicationUser variant, if you use a Generic User Model other than Core.Model.User as your User type,
// then you will NOT be able to leverage the Baseline.Account.RCL, and will need to clone that project, as you have to inject the
// IUserService<User> and IUserRepository<User> of the same type you define here, it won't work if the types miss-matched
// Additionally, if you use a model other than ApplicationUserWithNames, you'll want to implement and register your own 
var baselineInstallerOptions = new BaselineCoreInstallerOptions(
    AddMemberFields: true
    );

builder.Services.AddCoreBaseline<TUser>(
    installerOptions: baselineInstallerOptions,
    contentItemAssetOptionsConfiguration: (options) => {
        // If Installing the Starting Site Media is true, these are the configurations
        options.ContentItemConfigurations.Add(new ContentItemAssetOptions.ContentItemWithAssetsConfiguration("Generic.Image", [
            new ContentItemAssetOptions.AssetFieldIdentifierConfiguration("ImageFile", Core.Enums.ContentItemAssetMediaType.Image, "ImageTitle", "ImageDescription")
        ], PreCache: true));


        options.ContentItemConfigurations.Add(new ContentItemAssetOptions.ContentItemWithAssetsConfiguration("Generic.File", [
            new ContentItemAssetOptions.AssetFieldIdentifierConfiguration("FileFile", Core.Enums.ContentItemAssetMediaType.File, "FileTitle", "FileDescription")
        ], PreCache: true));

        /* Less used, you can uncomment out if you wish along with the AddStartingSiteElements options */
        /*
        options.ContentItemConfigurations.Add(new ContentItemAssetOptions.ContentItemWithAssetsConfiguration("Generic.Audio", [
            new ContentItemAssetOptions.AssetFieldIdentifierConfiguration("AudioFile", Core.Enums.ContentItemAssetMediaType.Audio, "AudioTitle", "AudioDescription")
        ], preCache: true));

        options.ContentItemConfigurations.Add(new ContentItemAssetOptions.ContentItemWithAssetsConfiguration("Generic.Video", [
            new ContentItemAssetOptions.AssetFieldIdentifierConfiguration("VideoFile", Core.Enums.ContentItemAssetMediaType.Video, "VideoTitle", "VideoDescription")
        ], preCache: true));
        */
    },
    mediaFileOptionsConfiguration: (options) => {

    },
    contentItemTaxonomyOptionsConfiguration: (options) => {

    },
    relationshipsExtendedOptionsConfiguration: (options) => {
        // In the future, there will probably be Language Syncing in the Relationships Extended, keep an eye out for it.
        //options.AllowLanguageSyncConfiguration = true;
        //var langSyncConfigs = new List<LanguageSyncClassConfiguration>();
        //options.LanguageSyncConfiguration = new LanguageSyncConfiguration(langSyncConfigs, []);
    },
    metaDataOptionsConfiguration: (options) => {

    },
    imageProcessingOptionsConfiguration: (options) => {
        // XperienceCommunity.ImageProcessing configuration  
    },
    persistantStorageConfiguration: new TempDataCookiePersistantStorageConfiguration("TEMPDATA", (configurations) => {
        // Configure TempData Cookie
    })
);

/// BASELINE CUSTOMIZATION - Starting Site - Add your own Page metadata Converter here
builder.Services.AddScoped<IWebPageToPageMetadataConverter, CustomWebPageToPageMetadataConverter>();

// BASELINE CUSTOMIZATION - Core - Override Baseline customization points if wanted
/*
builder.Services.AddScoped<IUserMetadataProvider, CustomUserMetadataProvider>();
builder.Services.AddScoped<IMediaFileMediaMetadataProvider, CustomMediaFileMediaMetadataProvider>();
builder.Services.AddScoped<IContentItemMediaCustomizer, CustomContentItemMediaCustomizer>();
builder.Services.AddScoped<IContentItemMediaMetadataQueryEditor, CustomContentItemMediaMetadataQueryEditor>();
builder.Services.AddScoped<ICustomTaxonomyFieldParser, CustomCustomTaxonomyFieldParser>();
*/

// Gzip compression handling for js.gz, css.gz and map.gz files and setting CacheControl headers based on ?v parameter detection
builder.Services.UseGzipAndCacheControlFileHandling();

```

Next, configure additional items the Baseline Core leverages:

```csharp

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

```

## 3. Member Roles Hookup

If you are NOT going to use the Baseline.Account module, add this:

```csharp
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
        options.Events.OnRedirectToAccessDenied = ctx => {
            var factory = ctx.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = factory.GetUrlHelper(new ActionContext(ctx.HttpContext, new RouteData(ctx.HttpContext.Request.RouteValues), new ActionDescriptor()));
            var url = urlHelper.Action("Signin", "Account") + new Uri(ctx.RedirectUri).Query;

            ctx.Response.Redirect(url);

            return Task.CompletedTask;
        };
        // These are not part of standard kentico but 
        if (builder.Environment.IsDevelopment()) {
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        } else {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        }
    });

    builder.Services.Configure<AdminIdentityOptions>(options => {
        // The expiration time span for admin. In production environment, set expiration according to best practices.
        options.AuthenticationOptions.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

    builder.Services.AddAuthorization();
}
```

If you ARE going to use Baseline.Account module, then use this instead:

```csharp
/// <summary>
/// BASELINE CUSTOMIZATION - Account - Configure this method for your own uses
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
```

## 4. Middleware hookup

The Baseline Core has 2 middlewares, one at the beginning (before Kentico) and one at the end (After Kentico).

```csharp
var app = builder.Build();

app.UseCoreBaseline();

// All Kentico's stuff
app.InitKentico();
...

app.UseCoreBaselineEnd();
```

## 5. View Import / Global Using

Below are some Tag Helpers and recommended global usings so you can easily get at the features and tools in the Baseline Core

_ViewImports.cshtml
```csharp
@* Baseline.Core *@
@using Core.Enums
@using Core.Models
@using Core.Repositories
@using Core.Services
@inject IUrlResolver UrlResolver
@addTagHelper *, XperienceCommunity.Baseline.Core.RCL
@addTagHelper *, XperienceCommunity.Baseline.Core.RCL.Xperience


@* MVC Caching *@
@using MVCCaching
@inject ICacheDependenciesScope CacheScope
@inject ICacheRepositoryContext CacheContext
@addTagHelper *, MVCCaching.Base.Core.Components

@* Localization *@
@using Microsoft.Extensions.Localization
@inject IHtmlLocalizer<OptionalResxResourceClass> HtmlLocalizer
@inject IStringLocalizer<OptionalResxResourceClass> StringLocalizer

```

GlobalUsing
```csharp
// Core
global using Core.Repositories;
global using Core.Services;
global using Core.Models;
global using Core.Extensions;
global using Core.Enums;
global using Core.Interfaces;

// Features
global using MVCCaching;
global using CSharpFunctionalExtensions;

// Other
global using System;
global using System.Collections.Generic;
global using System.Threading.Tasks;
```