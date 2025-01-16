# Context Helpers

Are you in Edit Mode?  Live Mode? Preview Mode?  Live Preview Mode?

The `IBaselinePageBuilderContext` service is here to help.  This helps you retrieve what context it is to do any additional configuration.

There is also the `XperienceCommunity.DevTools.MVCCaching`'s `ICacheRepositoryContext` which similarly has the Preview mode.

## Conditional Tag Helpers on Starter Site

The starter site uses the [XperienceCommunity.DevTools.PageBuilderTagHelpers](https://github.com/KenticoDevTrev/xperience-page-builder-utilities) which gives you the page-builder-mode and page-data-context tag helpers, use these as needed to conditionally show or hide things.

```html
<page-builder-mode exclude="Live">
  <!-- will be displayed in Edit and LivePreview modes -->
  <h1>Hello!</h1>
</page-builder-mode>

<page-builder-mode include="LivePreview, Edit">
  <!-- will be displayed in Edit and LivePreview modes -->
  <h1>Hello!</h1>
</page-builder-mode>

<page-data-context>
  <!-- or <page-data-context initialized="true"> -->
  <!-- will be displayed only if the IPageDataContext is popualted -->
  <widget-zone />
</page-data-context>

<page-data-context initialized="false">
  <!-- will be displayed only if the IPageDataContext is not populated -->
  <div>widget placeholder!</div>
</page-data-context>
```