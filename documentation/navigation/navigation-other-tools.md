# Other Tools

Below are a list of notable interfaces, models, and other systems.  Most of these are covered in more detail in the other Navigation documentation items.

## Interfaces

- [IBreadcrumbNavigation](../../src/Navigation/Navigation.Models/Repositories/IBreadcrumbRepository.cs)
  - **GetBreadcrumbsAsync**: Used to get breadcrumbs of the given TreeIdentity
  - **GetDefaultBreadcrumbAsync**: Gets the default breadcrumb (see [Breadcrumbs](navigation-breadcrumbs.md))
  - **BreadcrumbsToJsonLDAsync**: Converts an array of Breadcrumbs into the LD+Json model.
- [INavigationRepository](../../src/Navigation/Navigation.Models/Repositories/INavigationRepository.cs)
  - **GetNavItemsAsync**: Retrieves navigation based on `Navigation` Page type given the path.  If NavTypes are provided, filters via the `NavigationGroups` field (Xperience by Kentico) or `Tree Categories` in Kentico Xperience 13 (part of the Relationships Extended module)
  - **GetSecondaryNavItemsAsync**: Retrieves secondary navigation items.  Note that the `orderBy` and `whereCondition` are no longer supported in Xperience by Kentico.  This is leveraged by the `SecondaryNavigationViewComponent`.
  - **GetAncestorPathAsync**: Helper to get the Ancestor path given the properties.
- [ISiteMapRepository](../../src/Navigation/Navigation.Models/Repositories/INavigationRepository.cs)
  - **GetSiteMapUrlSetAsync**: Retrieves the default Sitemap Set, for logic see [the sitemap documentation](navigation-sitemap.md)


## View Components

See the various Features, all are listed there.

## TagHelpers

See [NavigationItem and Tag Helpers](navigation/navigation-navigation-item.md)  To use the tag helpers and view components, add these:

Xperience by Kentico:

```html
@* BASELINE CUSTOMIZATION - Navigation *@
@addTagHelper *, XperienceCommunity.Baseline.Navigation.RCL
@addTagHelper *, XperienceCommunity.Baseline.Navigation.RCL.Xperience
```

Kentico Xperience 13:
```html
@* BASELINE CUSTOMIZATION - Navigation *@
@addTagHelper *, XperienceCommunity.Baseline.Navigation.RCL
@addTagHelper *, XperienceCommunity.Baseline.Navigation.RCL.KX13
```

## Cache Dependency Key Extension

The `ICacheDependencyKeyBuilder` has an extension method `.Navigation` which contains some additional custom cache keys that the Navigation System uses, please use this extension when caching for navigation.

