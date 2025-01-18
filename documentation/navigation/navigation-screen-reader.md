# Screen Reader Navigation

Making your site accessible is not only noble, but also can land you in legal problems if it's not.

Along with an Accessibility quick navigation, you want your other navigations to be usable.

For those who use Keyboard navigation, the Navigation module comes with a [ScreenReaderNavigationViewComponent](../../src/Navigation/Navigation.RCL/Components/Navigation/ScreenReaderNavigation/ScreenReaderNavigationViewComponent.cs) 

This component takes an array of Navigation Items, an ID, and optionally a shortcut Selector that will hook up the enter/space command and bring them to the first navigation item.

```html

<!-- Some ADA menu at the top of the page elsewhere -->
 <div id="screen-reader-nav">
    <a class="visually-hidden-focusable" href="#main-content" role="button">Skip to main content.</a>
    <a class="visually-hidden-focusable" href="#accessible-menu" id="ada-nav-skipto" role="button">Skip to navigation.</a>
</div>

...

<vc:screen-reader-navigation x-navigation-items=@mainNavItems x-navigation-id="accessible-menu" x-nav-shortcut-selector="#ada-nav-skipto">
```

## Usage anywhere

Both the Main Navigation and Secondary Navigation systems allow you to include the Screen Reader automatically (using the `NavWrapperClass` for it's ID property), however you can use this wherever you like.