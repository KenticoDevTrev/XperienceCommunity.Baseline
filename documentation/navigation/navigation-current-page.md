# Current Page Tag Helper

In some cases, navigations are benefited by having some sort of 'indicator' of what page on the navigation you are currently on.

[NavigationPageSelectorTagHelper](../../src/Navigation/Navigation.RCL/TagHelpers/NavigationItemClassTagHelper.cs) is a special tag helper that adds a simple javascript snippet that does the following:

1. Will look for a `li` element matching the given (`x-current-page-path`) or the current page (`IPageContextRepository.GetCurrentPageAsync`)'s href or path, under the provided `x-parent-class`
2. If it finds it, it will add the css class `active` on it, as well as set the `aria-current=page` for screen readers

This allows you to cache the ENTIRE navigation, and add what the 'current' page is outside of the cache, which is much better than having to render the navigation for each page just so you can have a current indicator.

Keep in mind if this does not fit your needs, you can simply copy this code and make your own tag helper.

```html
 <header data-ktc-search-exclude>
     <cache expires-after="CacheMinuteTypes.VeryLong.ToTimeSpan()">
         @{
             CacheScope.Begin();
         }

         <vc:main-navigation x-navigation-parent-path="/navigation" x-include-screen-reader-nav="true" x-css-class="main-nav" />
         <cache-dependency cache-keys="@CacheScope.End()" />
     </cache>
     <!-- Selector outside of cache -->
     <bl:navigation-page-selector x-parent-class="main-nav"></bl:navigation-page-selector>
 </header>
```