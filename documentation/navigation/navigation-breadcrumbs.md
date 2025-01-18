# Breadcrumbs

Another common element in site navigations is Breadcrumbs.

The baseline contains two components, [BreadcrumbsViewComponent](../../src/Navigation/Navigation.RCL/Components/Navigation/Breadcrumbs/BreadcrumbsViewComponent.cs) and [BreadcrumbsJsonViewComponent](../../src/Navigation/Navigation.RCL/Components/Navigation/BreadcrumbsJson/BreadcrumbsJsonViewComponent.cs)

## Breadcrumbs View Component

The breadcrumbs view component takes two optional properties.

The first is `x-page-id` which is the NodeID or WebPageItemID of the page, if you leave it empty it will use the `IPageContextRepository.GetCurrentPageAsync()` (recommended leave empty).

The other is `x-include-default-breadcrumb` (default true) which will include a "Home" breadcrumb.  This is configured through the [Localiaziation Strings](../core/core-localization.md) `generic.default.breadcrumbtext` and `generic.default.breadcrumburl`.  Use the Localization system in either Kentico Xperience 13 or the custom localization [XperienceCommunity.Localization](https://github.com/nittin-cz/xperience-community-localization) in Xperience by Kentico.

In Xperience by Kentico, You also can change what the Default breadcrumb text and URL is (and optionally add a different localization string) via the Navigation Channel Settings (Channel Management -> List of Channels -> (Your channel) -> Navigation Channel Settings)

```html
<!-- if using the breadcrumbs section exist in the _layout somewhere -->
@section breadcrumb {
    <vc:breadcrumbs />
}

<!-- or just wherever -->
 <vc:breadcrumbs />
```

### Manual Breadcrumbs

There is also an optional [BreadcrumbsManualViewComponent](../../src/Navigation/Navigation.RCL/Components/Navigation/BreadcrumbsJson/BreadcrumbsManualViewComponent.cs) which allows you to pass an array of `Breadcrumb` items if your breadcrumbs are manually created (like if you have a Controller rendered section).

### Customization

Almost gaurenteed, you're going to want to modify the actual rendering of the breadcrumbs to fit your site theme.

You do this largely through the [customization point](../general/customization-points.md) of View Overrides.  Simply add a `____.cshtml` to match these paths and names to override.

- [/Components/Navigation/Breadcrumbs/Breadcrumbs.cshtml](../../src/Navigation/Navigation.RCL/Components/Navigation/Breadcrumbs/Breadcrumbs.cshtml): The rendering of the breadcrumbs.

## Breadcrumbs LD+JSON

Adding JSON LT of the breadcrumbs is a good SEO item to have on your site, as it helps Google know where you page is in terms of other navigation.

The `BreadcrumbsJsonViewComponent` has the same configuration and logic as the `BreadcrumbsViewComponent`, but it renders itself out as the proper LD+JSON, place this in your header.

```html
<head>
    <vc:breadcrumbs-json />
</head>
```

### Manual Breadcrumbs LD+JSON

There is also an optional [BreadcrumbsJsonManualViewComponent](../../src/Navigation/Navigation.RCL/Components/Navigation/BreadcrumbsJson/BreadcrumbsJsonManualViewComponent.cs) which allows you to pass an array of `Breadcrumb` items if your breadcrumbs are manually created (like if you have a Controller rendered section).

