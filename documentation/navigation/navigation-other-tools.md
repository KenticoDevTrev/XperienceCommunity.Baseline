# Other Tools

Below are a list of notable interfaces, models, and other systems.  Most of these are covered in more detail in the other Navigation documentation items.

## Interfaces

- [IBreadcrumbNavigation](../../src/Navigation/Navigation.Models/Repositories/IBreadcrumbRepository.cs)
  - **GetBreadcrumbsAsync**: Used to get breadcrumbs of the given TreeIdentity
  - **GetDefaultBreadcrumbAsync**: Gets the default breadcrumb (see [Breadcrumbs](navigation-breadcrumbs.md))
  - **BreadcrumbsToJsonLDAsync**: Converts an array of Breadcrumbs into the LD+Json model.
- [INavigationRepository](../../src/Navigation/Navigation.Models/Repositories/INavigationRepository.cs)
  - **GetNavItemsAsync**: 
