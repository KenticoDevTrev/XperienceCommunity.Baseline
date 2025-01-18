# NavigationItem and Tag Helpers

The [NavigationItem](../../src/Navigation/Navigation.Models/Models/NavigationItem.cs) is the Kentico Agnostic Navigation class used in both Main and Secondary Navigations (and also in some other applications).

This contains various identifiers of the page (which can be used in code or client side for identifying the current page), the properties needed for it to render, as well as a list of Children for any sub items.

Although you should never really need to build out your own NavigationItems (thanks largely to the `INavigationRepository`), you can look at our implementation as how we built it in case you ever need to generate yourself, or simply just create them manually since they are just a simple record type.

## Tag Helpers

There's various Tag Helpers that help perform operations given a Navigation Item.  These are used primarily in our Main and Secondary Navigation rendering, but you can use them at will if you wish.

- [NavigationItemLinkTagHelper](../../src/Navigation/Navigation.RCL/TagHelpers/NavigationItemTagHelpers.cs)
  - `bl-navigation-item=@MyNavItem`: given the NavigationItem, adds the title, onclick, and href values if the NavigationItem has those properties.  Available on `a` tags
- [NavigationItemNavReferenceTagHelper](../../src/Navigation/Navigation.RCL/TagHelpers/NavigationItemTagHelpers.cs)
  - `bl-navigation-item=@MyNavItem bl-navitem-reference`: given the NavigationItem, adds a `data-navpath` and `data-navhref` parameter which can be used to help detect which matches the current page path or url.  Available on `li` or `a` tags
- [NavigationItemClassTagHelper](../../src/Navigation/Navigation.RCL/TagHelpers/NavigationItemTagHelpers.cs)
  - `bl-navigation-item=@MyNavItem bl-navitem-class`: given the NavigationItem, adds the navigation item's CSS Class (if present).  Available on `li`, `a` and `article` tags.
- [NavigationPageSelectorTagHelper](../../src/Navigation/Navigation.RCL/TagHelpers/NavigationItemClassTagHelper.cs): Discussed on it's own [documentation page](navigation-current-page).

To see Tag Helpers, please use the following:

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