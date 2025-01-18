# Authorization

The Baseline comes included with the [XperienceCommunity.Authorization](https://github.com/KenticoDevTrev/KenticoAuthorization) System, which can validate the current user against the requested Controller Routes or Pages served by the Page Builder.

For Kentico Xperience 13 this uses the Out of the Box Permissions system.

For Xperience by Kentico, this currently uses my [XperienceCommunity.MemberRoles](https://github.com/KenticoDevTrev/MembershipRoles_Temp) system.

Please see documentation on [the Usage of the Authorization system](https://github.com/KenticoDevTrev/KenticoAuthorization?tab=readme-ov-file#usage).

## Integrations with other Systems

The Authorization system (in the Xperience by Kentico at least) is also integrated with:

- Navigation Module's Main Navigation and Secondary Navigation items returned from the `INavigationRepository` are filtered by the current user's authorization to those pages.  Additionally it's `ISiteMapRepository` also filters out any pages that are not public from the sitemap.
- Search's Module's `ISearchRepository` filters the return page results against the current user's authorization.
- TabbedPages Module's `ITabRepository` filters out any tabs that the current user isn't authorized to see.