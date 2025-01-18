# Installation for Xperience by Kentico

The Navigation system is contained in the nuget packages and is simple to hook up.

## 1. Add Nuget Packages

Install the `XperienceCommunity.Baseline.Navigation.RCL.Xperience` nuget package on your main MVC Site project.

```
npm install XperienceCommunity.Baseline.Navigation.RCL.Xperience
```

[See the Modules Architecture Overview](../general/modules-architecture-overview.md) if you wish to install individual packages on your libraries based on dependencies.

## 2. CI/CD Setup

On your IServiceCollection call the `AddBaselineNavigation`, passing in your `BaselineNavigationOptions`

You may also opt to add your own `IDynamicNavigationRepository` and `ISiteMapCustomizationService`

```csharp

// BASELINE CUSTOMIZATION - Navigation - Add navigation and configure here
builder.Services.AddBaselineNavigation(new Navigation.Models.BaselineNavigationOptions() {
    ShowPagesNotTranslatedInSitemapUrlSet = false
});

// Optional Overrides
// builder.Services.AddScoped<IDynamicNavigationRepository, CustomDynamicNavigationRepository>();
// builder.Services.AddScoped<ISiteMapCustomizationService, CustomSitemapCustomizationService>();
```

Lastly, if you wish to use the Navigation's built in [SitemapController](../../src/Navigation/Navigation.RCL/Features/Sitemap/SiteMapController.cs), you can use the `WebApplication.UseSitemapRoute` extension (with optional sitemap patterns).

```csharp
// BASELINE CUSTOMIZATION - Navigation - Enable Sitemap here
app.UseSitemapRoute(sitemapPatterns: ["sitemap.xml", "googlesitemap.xml"]);
```


## 3. Page Templates and enable Page Builder

For Navigation Items, in order to get the Mega Menu functionality, you also need to register the page template and the Page type

``` csharp
// BASELINE CUSTOMIZATION - Navigation - Add this for the Navigation Mega Menu Support
[assembly: RegisterPageTemplate(
    "Generic.Navigation_Default",
    "Navigation",
    typeof(NavigationPageTemplateProperties),
    "/Features/Navigation/PartialNavigation/NavigationPageTemplate.cshtml",
    ContentTypeNames = [Generic.Navigation.CONTENT_TYPE_NAME])]
```

```csharp
// Enable desired Kentico Xperience features
builder.Services.AddKentico(features => {
    features.UsePageBuilder(new PageBuilderOptions {
        ContentTypeNames =
        [
            // Enables Page Builder for content types using their generated classes
            
            ...
            // BASELINE CUSTOMIZATION - Navigation - If using Navigation content type, MUST add it here
            Generic.Navigation.CONTENT_TYPE_NAME,
        ]
    })
});

```