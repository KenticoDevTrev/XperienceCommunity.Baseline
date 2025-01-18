# Baseline Core Installation (Kentico Xperience 13)

These are the instructions for the Baseline Core Mode Installation.  The Starter Sites already have everything installed, so if you are using the Starting Site, you can largely skip this.

If you are installing this on your own instance, I would highly recommend analyzing the Starting Site's [Startup.cs](../../starting-site/kx13/MVC/MVC/Program.cs) and [StartupConfigs.cs](../../starting-site/kx13/MVC/MVC/Configuration/StartupConfigs.cs) for inspiration.

## 1. Admin Nuget Packages

The Baseline systems does have a dependency on two custom module for the Kentico Admin (for Categories and Relationships).

1. Open you `CMSApp` solution
2. Install the [RelationshipsExtended](https://www.nuget.org/packages/RelationshipsExtended) NuGet Packages on your `CMSApp` project
3. Update the `RelationshipsExtended.Base` related project to the highest version (bug fixes)
4. Install the [XperienceCommunity.PageCustomDataControlExtender](https://www.nuget.org/packages/XperienceCommunity.PageCustomDataControlExtender)
6. Rebuild the `CMSApp`
7. Run the admin, and verify in the event log that the installation was successful.

Optionally, here are some other Admin-specific NuGet packages that are recommended:
* [XperienceCommunity.UrlRedirection.Admin 13.0.9](https://www.nuget.org/packages/XperienceCommunity.UrlRedirection.Admin) (if you use, must install the [XperienceCommunity.UrlRedirection 13.0.12](https://www.nuget.org/packages/XperienceCommunity.UrlRedirection) on the `MVC` project as well)
* [XperienceCommunity.CSVImport.Admin 13.0.0](https://www.nuget.org/packages/XperienceCommunity.CSVImport.Admin)
* [XperienceCommunity.SvgMediaDimensions](https://www.nuget.org/packages/XperienceCommunity.SvgMediaDimensions)

There are many other `XperienceCommunity` prefixed NuGet packages, but most of the rest are for the `MVC` application only.


## 2. Admin Site Objects

The next thing you'll need to install are some Admin level objects.

1. Grab the [Baseline_Generics.1.0.0.zip](../../starting-site/kx13/Baseline_Generics.1.0.0.zip) from this repository
2. In the Admin, go to Site -> Import Site or objects
3. Upload this package and import the objects.  Not all are required, but here's the breakdown:
    - Form Controls
      - All of them (used in some of the base page types)
    - Page Types
      - `Base Inherited Page`, `Redirect Only Inherited Page` are the only required types (contain metadata/redirect base page types to to inherit from to add this functionality)
      - `Navigation` is for the `Navigation` module and represents a Navigation Item for the Main Navigation, along with `Search` for the Search Page
      - `Account` is for the `Account` Module and has templates ready for your account pages (such as login, registration, etc)
      - `Ecommerce Page` is for the `Ecommerce` Module and has templates for checkout pages, along with `Product` page
      - `Tab` and `Tab Parent` are for the `TabbedParent` Module, and have page templates for them.
      - `Basic Page`, `Home` are for the Starting Site
      - `File` and `Folder` are additional page types you might find useful, and they mimic the Kentico 12 CMS.File and CMS.Folder (the folder counts as part of the URL path)
      - `Header` and `Footer` are optional and are for having a Page Builder controlled Header/Footer with widgets (uses the Partial Widget Page Functionality), similarly the `Shareable Content` is for the Shareable Content Widget that also uses the Partial Widget Page Functionality
    - Resource Strings
      - `Generic.InlineEditors.*` help required
      - `Generic.default.breadcrumb____` are for the breadcrumb if you plan on using the `Navigation` Module
    - Settings Keys
      - `Default Media Library` is for the Image Uploader widget (imported widget from Dancing Goat)
      - The rest are all part of the `Account` Module and only needed if you plan on using that module.
4. Resign Macros (System -> Macros -> Signatures -> Sign all Macros + Update macro Signatures)
5. For the Page Types uploaded, make sure they are assigned to your sites.

## 3. Site Nuget Packages

The Baseline Core packages are highly modularized, and each library has different dependencies, however all of them boil down to this package:

`XperienceCommunity.Baseline.Core.RCL.KX13`

on your project, run:

```
npm install XperienceCommunity.Baseline.Core.RCL.KX13
```

Please see [the detailed overview of all nuget packages and dependencies](../general/modules-architecture-overview.md) if you wish to install packages on other projects.


## 4. CI/CD Addition

In your startup, add the following bits of code:

```csharp
// Baseline services
services.AddCoreBaseline<ApplicationUser, User>(
    persistantStorageConfiguration: new TempDataCookiePersistantStorageConfiguration("TEMPDATA", (configurations) => {
        // Configure TempData Cookie
    }),
    imageTagHelperOptionsConfiguration: (options) => {
        // Image tag helper options
    }
);

// Relationships Extended
services.AddSingleton<IRelationshipExtendedHelper, RelationshipsExtendedHelper>();

// Admin redirect filter
services.AddSingleton<IStartupFilter>(new AdminRedirectStartupFilter(Configuration));

// Environment tag helper
services.AddSingleton<IPageBuilderContext, XperiencePageBuilderContext>();

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

            // Page template filters
services.AddPageTemplateFilters(Assembly.GetExecutingAssembly());


```

## 5. Middleware hookup

The Baseline Core has 1 middleware, one at the beginning (before Kentico).

```csharp
// IApplicationBuilder
app.UseCoreBaseline();

app.UseKentico();

```

## 6. View Import / Global Using

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
@addTagHelper *, XperienceCommunity.Baseline.Core.RCL.KX13


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