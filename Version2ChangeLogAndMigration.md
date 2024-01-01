# Baseline Version 2
We're finally there, the refactored version 2 of the Baseline!

**Minimum Requirements**

 1. Kentico Xperience 13.0.130*
 3. .Net 8.0 solution
 
 *Eventually Xperience by Kentico will be supported 

## Change Overview
From Version 1, this version had the following items modified / changed:

1. Refactored View Component Properties to be prefixed with "x" so properties are x- prefixed `<vc:my-view-component x-property-one x-property-two />` 
2. Refactor Tag Helpers to have prefix "bl-" so easy to see available Baseline tag helpers
3. Replaced Classes with Records where possible, and made sure Records were immutable.
4. Replaced `Node` and `Document` with `Tree`, `TreeCulture`, `Content`, `ContentCulture` since Xperience by Kentico will have a distinction between Content Items (Content Hub Item) and Tree Items (WebPage), with some cross play.
5. Added Obsolete tags to any Node/Document usage
6. Updated to .net 8 (and add in new syntactical sugar)
7. Remove `[assembly: Register]` tags out of code and into readme with instructions on how to include
8. New Starter Site (no jQuery)
9. Other Items

# Changelog / Migration

## 1. Prefixing x- for view component properties

**BreadcrumbsJsonViewComponent**

- Changed nodeId to xPageId
- Changed IncludeDefaultBreadcrumb to xIncludeDefaultBreadcrumb

**BreadcrumbsViewComponent:**

- Changed nodeId to xPageId
- Changed IncludeDefaultBreadcrumb to xIncludeDefaultBreadcrumb

**BreadcrumbsManualViewComponent**

- Changed breadcrumbs to xBreadcrumbs
- Changed includeDefaultBreadcrumb to xIncludeDefaultBreadcrumb

**BreadcrumbsJsonManualViewComponent**

- Changed breadcrumbs to xBreadcrumbs
- Changed includeDefaultBreadcrumb to xIncludeDefaultBreadcrumb

**SecondaryNavigationViewComponent**

- Changed navigationProperties to xNavigationProperties

**TabParentViewComponent**

- Changed page to xPage

**PageMetaDataViewComponent**

- change documentId to xContentCultureId

**ManualPageMetaDataViewComponent**

- changed metaData to xMetaData

**MainNavigationViewComponent**

- changed NavigationParentPath to xNavigationParentPath
- changed CssClass to xCssClass
- changed includeScreenReaderNav to xIncludeScreenReaderNav

**ScreenReaderNavigationViewComponent**

- changed navigationItems to xNavigationItems
- changed navigationId to xNavigationId,
- changed navShortcutSelector to xNavShortcutSelector

**BreadcrumbsManualViewComponent**

- changed breadcrumbs to xBreadcrumbs
- changed includeDefaultBreadcrumbs to xIncludeDefaultBreadcrumbs

**ConfigurationHelperViewComponent**

- changed instructions to xInstructions
- changed needsAttention to xNeedsAttention
- changed mode to xMode
- changed visible to xVisible

## 2. bl- prefix for tag helpers
`navitem-references` => `bl-navitem-references`
`navitem-class` => `bl-navitem-class`
`navitem-link` => `bl-navitem-link`
`navigation-item` => `bl-navigation-item`

For the Navigation Page Selector, although it's a tag helper, it's treated roughly as a view component, so it is refactored as such:
`navigation-page-selector` => `bl:navigation-page-selector`
- `current-page-path` => `x-current-page-path`
- `parent-class` => `x-parent-class`

ex: `<bl:navigation-page-selector x-current-page-path="@Model.CurrentPagePath" x-parent-class="@Model.NavWrapperClass" />`

## 3. Replacing Class with Records
There shouldn't be any refactoring required, unless you tried to modify a Class property that is now an Immutable Record, in which case you will need to use the `with` keyword to mutate the record.

For example, what was:
`myCategoryItem.CategoryDisplayName = "I'm Changing it!";`
will now need to be:
`myCategoryItem = myCategoryItem with { CategoryDisplayName = "I'm Changing it!" };`

This is to prevent immutable objects from being modified which may modify a cached value.

## 4/5 Change To New Identity Models
Identities have been adjusted to be geared towards the Xperience by Kentico implementation.  Many of these identities are simply name changes, see below as the reference, showing how the data matches:
`KX13` => `Baseline` <= `Xperience by Kentico`

`NodeIdentity` => `TreeIdentity` <= `WebPageItem`
- NodeId => PageID <= WebPageItemID
- NodeGuid => PageGuid <= WebPageItemGuid
- NodeAliasPathAndSiteId => PathAndChannelId <= WebPageItemTreePathAndWebPageItemWebsiteChannelId

`NodeIdentity` => `ContentIdentity` <= `ContentItem`
- NodeId => ContentID <= ContentItemID
- NodeGuid => ContentGuid <= ContentItemGuid
- NodeAliasPathAndSiteId => PathAndChannelId* <= WebPageItemTreePathAndWebPageItemWebsiteChannelId

*in Xperience by Kentico, will find the WebPageItem and then look up the Content Item correlated to it

`DocumentIdentity` => `ContentCultureIdentity` <= `ContentItemCommonData`
- DocumentId => ContentCultureID <= ContentItemCommonDataID
- DocumentGuid => ContentCultureGuid <= ContentItemCommonDataGuid
- NodeAliasPathAndMaybeCultureAndSiteId => PathAndMaybeCultureAndChannelId <= WebPageItemTreePathAndCultureAndChannelId
- [] => MaybeContentIDAndMaybeCulture** <= ContentItemIDAndCulture

**in Xperience by Kentico, There is a Culture version of a Content Item, so this field allows to be found by this.  In Kentico Xperience 13, the "ContentID" is the NodeID, and the Culture is the language code

`DocumentIdentity` => `TreeCultureIdentity(CultureCode)` <= `WebPageItem + Culture`
	-DocumentId => PageID *** (NodeID) <= WebPageItemID
	-DocumentGuid => PageGuid (NodeGuid) <= WebPageItemGuid
	-NodeAliasPathAndMaybeCultureAndSiteId => PathAndChannelId (NodeAliasPath and SiteID) <= WebPageItemTreePathAndChannelId

**IMPORTANT: REFACTORING REQUIRED FOR TreeCultureIdentity**
For switching between `DocumentIdentity` to `TreeCultureIdentity`, in Xperience by Kentico there isn't a direct link between a culture version of a content item and a Web Page Item, while in Kentico Xperience 13 every Cultured Item (Document) has a Web Page (Node).

For this reason, the TreeCultureIdentity (saying it must be a Web Page/Node) leverages the WebPage Identifiers and a required Culture that is part of the constructor.  This means you will need to refactor KX13 implementations that previous used `DocumentIdentity` and now use `TreeCultureIdentity` so it will look up by the `NodeID/NodeGuid/NodeAliasPath` and culture, instead of the `DocumentID` and `DocumentGuid`.


**IMetaDataRepository**

- Changed documentId to contentCultureId
- Changed documentGuid to contentCultureGuid

**NavigationItem && NavigationItemBuilder**

- Changed LinkPageGUID to LinkPageGuid
- Changed LinkDocumentGUID to LinkContentCultureGuid
- changed LinkDocumentID to LinkContentCultureID

**IPageContextRepository**
- GetPageAsync(TreeCultureIdentity identity) replaces GetPageAsync(DocumentIdentity), however the logic is different and may require refactoring to transition.


## 6 .net 8 Syntactical Sugar
These changes will not impact your existing code, they mainly involved using the new simplified constructors for classes, using the `[]` initializer instead of `new ______()`, and some other recommended refactorings.

## 7 Assembly Tag Removal
Users should have control over what Kentico items they wish to register or not.  By having the `[assembly: Register...]` tags in code, users did not have the ability to pick or choose.  So these have been moved into the `kenticoAssemblyTags.md` of the various `_____.RCL.KX13` and `_____.XperienceModels.KX13` projects.

You'll want to manually add these assembly tags somewhere in your solution, probably in your main `MVC` project (for page templates, widgets, or components)  or your `XperienceModels` (for page types) since that will be tied into a specific version of Kentico/your implementation

 - `/Account/Account.RCL.KX13/kenticoAssemblyTags.md` contains all the page template and authorization assembly tags for the various account page templates.
 - `/Account/Account.XperienceModels.KX13/kenticoAssemblyTags.md` contains the Account Page Type Registration
 - `/Core/Core.RCL.KX13/kenticoAssemblyTags.md` contains the extra Form Components for decimal, double, and GUID
 - `/Ecommerce/Ecommerce.XperienceModels.KX13/kenticoAssemblyTags.md` contains the Ecommerce Page Type
 - (Note that the `HBS.GenericEcommerce` package is not in the Baseline's control and will register it's page templates for the `Ecommerce` page automatically)
 - `/Navigation/Navigation.Library.KX13/kenticoAssemblyTags.md` contains the global event module registration
 - `/Navigation/Navigation.RCL.KX13/kenticoAssemblyTags.md` contains the page templates for the Navigation Item page type (for widget based mega menus)
 - `/Navigation/Navigation.XperienceModels.KX13/kenticoAssemblyTags.md` contains the page type registration for the `Navigation` class
 - `/Search/Search.RCL.KX13/kenticoAssemblyTags.md` contains the Search Page Template
 - `/TabbedPages/TabbedPages.RCL.KX13/kenticoAssemblyTags.md` contains the Tab and Tab Parent Page Templates
 - `/TabbedPages/TabbedPages.XperienceModels.KX13/kenticoAssemblyTags.md` contains the registration of the `Tab` Page Type

## New Starter Site
There will be a new Starter Site that will leverage version 2, and will be .net 8.0

There will also be the added change of excluding jQuery by default, since this is now supported by Kentico, and jQuery is often a drag on site performance.

## 9 "Other"
Here are some other changes that do not fit a main category:

`ObjectQueryExtensions`
- Changed to namespace `Core.Extensions`
