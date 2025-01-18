# Secondary Navigation

Unlike main navigations, secondary navigations are almost always contextual, based on the web pages at and around the current page.

The Navigation module contains the [SecondaryNavigationViewComponent](../../src/Navigation/Navigation.RCL/Components/Navigation/SecondaryNavigation/SecondaryNavigationViewComponent.cs) which takes a `SecondaryNavigationProperties` and renders out the secondary navigation based on those properties.  Let's break them down.

## SecondaryNavigationProperties and configuration

Here's the properties, i'm just copying the property comments for this documentation.

- `Path`: The Path that the navigation properties build off of.  If empty or not provided, will use the current page's path (`IPageContextRepository.GetCurrentPageAsync`)
- `Level`: The level the navigation should start at.  0 = At the root (if LevelIsRelative is false), or 0 = Current page's level (if LevelIsRelative is true)
- `LevelIsRelative`: If the given Level is relative to the current page's level.  If true, then the Level dictates what parent is the start point.  A level of 2 will go up 2 levels. 
- `MinimumAbsoluteLevel`: How many levels down from the start of the navigation it should show entries.
- `CssClass`: The CSS class that will wrap this navigation.  useful both for styling and for the Navigation Page Selector
- `IncludeSecondaryNavSelector`: If true, will include the client-side javascript that sets the active
- `IncludeScreenReaderNav`: If true, will include the hidden screen reader navigation (with the ID being the CssClass)

Keep in mind the Level and LevelIsRelative is how you can build out navigations that show the parent and siblings.

LevelIsRelative = true + Level: 0 + MinimumAbsoluteLevel: 2 => Show the children and grandchildren

LevelIsRelative = true + Level: 1 + MinimumAbsoluteLevel: 2 => Show the siblings and children

LevelIsRelative = true + Level: 2 + MinimumAbsoluteLevel: 2 => Show the Parent and it's siblings items, and the current page and it's siblings

LevelIsRelative = false, Level: 0, MinimumAbsoluteLevel: 2 => Start at the root of the current page (highest ancestor) and show the navigation 2 levels down.

## Customizing the Secondary Navigation Rendering

Almost gaurenteed, you're going to want to modify the actual rendering of the Main Navigation to fit your site theme.

You do this largely through the [customization point](../general/customization-points.md) of View Overrides.  Simply add a `____.cshtml` to match these paths and names to override.

- [/Components/Navigation/SecondaryNavigation/SecondaryNavigation.cshtml](../../src/Navigation/Navigation.RCL/Components/Navigation/SecondaryNavigation/SecondaryNavigation.cshtml): The wrapper around your navigation and where the screen reader / Navigation Page Selector are optionally rendered.
- [/Components/Navigation/SecondaryNavigation/SecondaryNavigationItem.cshtml](../../src/Navigation/Navigation.RCL/Components/Navigation/SecondaryNavigation/SecondaryNavigation.cshtml): The First Level Navigation
- [/Components/Navigation/SecondaryNavigation/SecondaryNavigationDropdownItem.cshtml](../../src/Navigation/Navigation.RCL/Components/Navigation/SecondaryNavigation/SecondaryNavigation.cshtml): All levels below the first level, handles rendering items and their nested children.

## Customizing Everything

Keep in mind, you can simply clone the View Component and modify to your own desires and needs.  The view component is relatively simple and uses existing tools and interfaces available.

## Filtering Pages by Permissions (Xperience by Kentico Only)

The Xperience by Kentico version of the library comes with an additional `ISecondaryNavigationService` which has the `Task<IEnumerable<NavigationItem>> FilterAndConvertIWebPageContentQueryDataContainerItems(IEnumerable<IWebPageContentQueryDataContainer> webPageContentQueryDataContainers);` method.  You can use this to pass any IWebPageContentQueryDataContainer array and it will handle filtering out pages that the current user does not have authorization to view (using the [XperienceCommunity.MemberRoles](https://github.com/KenticoDevTrev/MembershipRoles_Temp) currently).