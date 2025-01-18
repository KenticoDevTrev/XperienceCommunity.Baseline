# Installation for Kentico Xperience 13

The Navigation system is mostly all contained in the nuget packages and is simple to hook up, *except* it does have a special page type `Generic.Navigation` which it uses for the Primary Navigation system.

Most navigation functionality is native to Kentico itself (including Sitemap fields), so this works off of the normal existing fields as much as possible.

## 1. Add Navigation Page Type

Log into the Kentico Admin, and proceed to Site -> Import Sites and Objects

Grab the [Baseline_Generics.1.0.0.zip](../../starting-site/kx13/Baseline_Generics.1.0.0.zip) file from this repository, and import it.

When selecting what Page Types to import, you will at minimum need the Navigation Page Type for this module.

## 2. Add Nuget Packages

Install the `XperienceCommunity.Baseline.Navigation.RCL.KX13` nuget package on your main MVC Site project.

[See the Modules Architecture Overview](../general/modules-architecture-overview.md) if you wish to install individual packages on your libraries based on dependencies.

## 3. CI/CD Setup

On your IServiceCollection call the `UseNavigation` (under `Navigation.Middleware` namespace).

This has an optional configuration where you can set up your own IBreadcrumbRepository or INavigationRepository, this honestly is the 'wrong' way of doing it as you can simply overwrite this using standard .net CI/CD

Optionally, you can also call `UseSitemap` which adds the sitemap interface hookups.

Lastly, if you wish to use the Navigation's built in [SitemapController](../../src/Navigation/Navigation.RCL/Features/Sitemap/SiteMapController.cs), you can use the `IEndpointRouteBuilder.UseSitemapRoute` extension (with optional sitemap patterns).

