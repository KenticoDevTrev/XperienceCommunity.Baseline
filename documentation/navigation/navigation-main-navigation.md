# Main Navigation

While some sites in the past would leverage a completely dynamic navigation, in most cases you want control over your Navigation structure.

The Baseline Main Navigation system comes with a `Navigation` page type that you can leverage to build out your main navigation.  This is automatically added for Xperience by Kentico when you use the Navigation Module, or manually imported via the [Baseline_Generics.1.0.0.zip](../../starting-site/kx13/Baseline_Generics.1.0.0.zip) for Kentico Xperience 13.

## Navigation Page Type

Here are the features of the Navigation page type.

### Automatic and Manual Navigation

By Setting the `NavigationType` to `Automatic`, you are presented with a selector for a Page.  Selecting the page will tell the system to render that page's URL, retreiving the page's Name, URL, etc.  

**This also handles Localization**, so if the user is in a different language and that page is available in that language, it will use that language's name and url, if not it will fall back to the default language.

By setting the `NavigationType` to `Manual`, you are presented with a `Link Text` which (which allows HTML in case you want an icon with it), `Link Url` (the URL of the link), and `Link Target` of _self or _blank.

### Mega Menu Support

By checking `Is Mega Menu`, you will be able to go to the Page Builder and via the widget, add in any widgets you want into it.  This will then be rendered when the navigation displays (by default, Mega Menu is usually only supported on the first level, but you can change things up if you wish)


#### Mega Menu Navigation and _Layout-NoHeader.cshtml

Since most page renderings use the main _layout, and the layout usually contains the Navigation on it, when editing Navigation Items in the page builder, it's vital to use a layout that does NOT have the main navigation in it (otherwise it will try to render out the navigation item you're modifying).

The default Navigation Page Template uses the `/Views/Shared/_Layout-NoHeader.cshtml` which you'll need to create or include if you use this feature.  The starting site contains this file already.  If you wish to change where this file location is, you can overwrite the [/Features/Navigation/PartialNavigation/NavigationPageTemplate.cshtml](../../src/Navigation/Navigation.RCL.Xperience/Features/Navigation/PartialNavigation/NavigationPageTemplate.cshtml) with your own copy.

### Navigation Styling and Advanced Functionality

There is also an optional place to put a `Alternative Text` if you wish (usually un-needed as the text within a link is considered accessible enough, but if you choose to use an icon only for the link text of a manual one, this is a good idea to set this).

`On Click (Javascript)` allows you to put an onclick javascript command.

`Link CSS Class` adds any classes to the link's CSS classes.  You have control of the rendering so this is only for customization, i would not use this to make the links match your template.

### Nav Types (Groupings)
When retrieving navigation items, you can pass an array of Navigation Types (`TagName` / `CategoryName`).  These will look to either to the items in the `Navigation Groupings` field (Xperience by Kentico) or the `TreeCategories` (Kentico Xperience 13 via the [Relationships Extended Module](https://github.com/KenticoDevTrev/RelationshipsExtended)).

This is useful if you have a separate Mobile Navigation and some items should only render in the mobile vs. desktop version, or can be used to tag navigation that should only show in some other context.  Be aware though that if you specify taxonomy, it will exclude any navigation with *no* taxonomy set, so often if you use this, you must update all your Navigation Items (and language variants in Xperience by Kentico)

### Dyanamic Navigation - Kentico Xperience 13

There is an `Is Dynamic` which if checked, allows you to fill out a "Repeater" style form to dynamically select and render out these fields.  These will appear as children to the current navigation item.

### Dynamic Navigation - Xperience by Kentico

There is a `Is Dynamic` and `Dynamic Menu Identifier`, that when set will call the `IDynamicNavigationRepository` (which you should overwrite) with that menu identifier.  It's up to you to then check what the identifier is, and perform any logic you wish to retrieve and build an Array of `NavigationItem`s for the children.  An example would be `BlogArticles` as the Menu Identifier, and you retrieve recent blog articles and convert them to `NavigationItem`s

## Placing the Main Navigation

The Baseline comes with a `MainNavigationViewComponent` class that you pass your Path of the navigation items, the optional CSS Class identifiying it, and if you want the Screen Reader Navigation rendered as well.

The Screen Reader navigation is a keyboard-controlled hidden navigation specifically for keyboard users.  Keep in mind not all visually impaire users use keyboard of course, so you want to make sure your main navigation has the proper aria tags and such to handle hover-descriptions and mobile accessability.

```html
<header data-ktc-search-exclude>
    <cache expires-after="CacheMinuteTypes.VeryLong.ToTimeSpan()">
        @{
            CacheScope.Begin();
        }

        <vc:main-navigation x-navigation-parent-path="/Navigation" x-css-class="main-nav" x-include-screen-reader-nav="true"/>
        <cache-dependency cache-keys="@CacheScope.End()" />
    </cache>
    @*
    // Only if needed context in the main nav, this is outside the cache so you can cache the entire nav and then just highlight the current page.  An empty 
    <bl:navigation-page-selector x-parent-class="main-nav"></bl:navigation-page-selector>
    *@
</header>
```

### Footer or other applications

You can also elect to use this for other navigations, such as the Footer Navigation, but may require different styling so you may want to just clone the view component for Footer applications.

## Customizing the Main Navigation Rendering

Almost gaurenteed, you're going to want to modify the actual rendering of the Main Navigation to fit your site theme.

You do this largely through the [customization point](../general/customization-points.md) of View Overrides.  Simply add a `____.cshtml` to match these paths and names to override.

- [/Components/Navigation/MainNavigation/MainNavigation.cshtml](../../src/Navigation/Navigation.RCL.Xperience/Components/Navigation/MainNavigation/MainNavigation.cshtml): The wrapper around your navigation and where the screen reader is optionally rendered.
- [/Components/Navigation/MainNavigation/MainNavigationItem.cshtml](../../src/Navigation/Navigation.RCL.Xperience/Components/Navigation/MainNavigation/MainNavigationItem.cshtml): The First Level Navigation, handles logic for Mega Menu Rendering, single links and drop downs.
- [/Components/Navigation/MainNavigation/MainNavigationDropdownItem.cshtml](../../src/Navigation/Navigation.RCL.Xperience/Components/Navigation/MainNavigation/MainNavigationDropdownItem.cshtml): All levels below the first level, handles rendering items and their nested children.

## Customizing Everything

Keep in mind, you can simply clone the View Component and modify to your own desires and needs.  The view component is relatively simple and uses existing tools and interfaces available.