# Core Module Tools

Below are a list of all the various Tools that the Core Module provides for Developers.


## Services and Repositories

Below are some Services and Repositories that have not already been mentioned in more detail by a specific documentation.

- [ILogger](../../src/Core/Core.Models/Services/ILogger.cs): Kentico Agnostic version of it's own Logging Service
- [IUrlResolver](../../src/Core/Core.Models/Services/IUrlResolver.cs): Two methods to make Urls Absolute based on the current request, and also to ResolveUrls (remove those blasted tildes!!!)
- [ISiteRepository](../../src/Core/Core.Models/Repositories/ISiteRepository.cs): Current Channel and Website Channel retrieval, as well as ID to String and String to ID conversions (Kentico Agnostic)

### Xperience by Kentico Only

Below are some Xperience by Kentico only Repositories and services.  Most are used for my own purposes to fulfill other roles.  These are all highly optimized and cached, use if you wish!

- [IContentItemLanguageMetadataRepository](../../src/Core/Core.Library.Xperience/Repositories/IContentItemLanguageMetadataRepository.cs): The data in the CMS_ContentItemLanguageMetadata is not readily available, but it is the only way to get the Localized Display Name (which can't be edited once a document is created...) and the Last Modified, which are used in certain applications such as Sitemaps and fallbacks for Page Metadata/Navigation name.
- [IContentTranslationInformationRepository](../../src/Core/Core.Library.Xperience/Repositories/IContentTranslationInformationRepository.cs): For a given page or content item, gives you what langauges it's currently translated into.  This is useful with the ILanguageRepository if you need to manually find a fallback (in cached scenarios), or if you want things like "translate this page" to be aware of what translations exist to display to users.
- [IContentTypeRetriever](../../src/Core/Core.Library.Xperience/Repositories/IContentTypeRetriever.cs): Given the identifier, gets the class name (can be used with IMappedContentItemRepository or for whatever you wish to use it for)
- [ILanguageIdentifierRepository](../../src/Core/Core.Library.Xperience/Repositories/ILanguageIdentifier.cs): Just translate LanguageID to name and vice versa.
- [ILanguageRepository](../../src/Core/Core.Library.Xperience/Repositories/ILanguageRepository.cs)
  - `GetLanguagueToSelect`: Performs the Language fall-back logic given the list of languages (used in 'retrieve and cache' everything where we need to do fallback manually)
  - `GetInstanceDefaultLanguage`: Kentico Default Language (Content Language)
  - `DefaultLanguageForWebsiteChannel`: Gets the default language for the current or given channel
  - `GetLanguageUrlPrefix`: Gets the URL prefix if it's not the default.
- [IMappedContentItemRepository](../../src/Core/Core.Library.Xperience/Repositories/IMappedContentItemRepository.cs): Allows you to try to retrieve the given content item if it's of the given type (object will just return the mapped object).  Handles dynamic lookup of it's class for proper mapping, and also includes WebPage data if it's a webpage.
- [IWebPageToPageMetadataConverter](../../src/Core/Core.Library.Xperience/Repositories/IWebPageToPageMetadataConverter.cs): Used in the Page Metadata system, retrieves the Page Metadata with other lookups.
- [IBaselineUserMapper](../../src/Core/Core.Library.Xperience/Repositories/IBaselineUserMapper.cs): Customization point, implement your own to set the User.MetaData based on your Kentico ApplicationUser, and also set the ApplicationUser from your own metadata in reverse.
- [IContentItemMediaCustomizer](../../src/Core/Core.Library.Xperience/Repositories/IContentItemMediaCustomizer.cs): Customization point, allows you to parse and retrieve additional information when retrieving media from Content Items (so return your own `IMediaMetada`)
- [IContentItemMediaMetadataQueryEditor](../../src/Core/Core.Library.Xperience/Repositories/IContentItemMediaMetadataQueryEditor.cs): Customization point, allows you to modify the queries for your Media Content Item Assets so you can include additional fields and data.
- [IMediaFileMediaMetadataProvider](../../src/Core/Core.Library.Xperience/Repositories/IMediaFileMediaMetadataProvider.cs): Customization point, allows you to parse and retrieve additional information when retrieving media from the Media Library (so return your own `IMediaMetada`)
- [IContentItemReferenceService](../../src/Core/Core.Library.Xperience/Repositories/IContentItemReferenceService.cs): Can retrieve the original Content Item GUID array from a selector (instead of the parsed model).
- [ICustomTaxonomyFieldParser](../../src/Core/Core.Library.Xperience/Repositories/ICustomTaxonomyFieldParser.cs): Takes the Taxonomy Field for the given data container and converts them to ObjectIdentity, used internally mainly.
- [IMetaDataWebPageDataContainerConverter](../../src/Core/Core.Library.Xperience/Repositories/IMetaDataWebPageDataContainerConverter.cs): Used mainly internally to get the Page Metadata from an IWebPageContentQueryDataContainer, but may be useful for other applications such as listing pages and such.
-

## Other Extension Methods

- [StringExtensions](../../src/Core/Core.Models/Extensions/StringExtensions.cs)
  - `AsNullOrWhitespaceMaybe`: Returns a `Maybe.None` if the string is null or whitespace
  - `SplitAndRemoveEntries`: Shortcut for `string.Split(delimiterArray, StringSplitOptions.RemoveEmptyEntries)`
  - `RemoveTildeFromFirstSpot`: Shortcut, removes tilde if there (useful in URLs)
  - `MaxLength`: Limits the string to the max length, adding an elipses, useful for summaries.
  - `ToAttributeId`: Converts the given text to an HTML css/id safe attribute (with 'id-' prefixed), useful for client side marking and linking.
  - Media Url String Helpers
    - `ParseGuidFromMediaUrl`: Tries to get the Guid value from `/getMedia/GUID/etc` Urls
    - `ParseGuidFromAssetUrl`: Tries to get the ContentItemGuid and FieldGuid value from the `/getContentAsset/ContentItemGUID/FieldGuid/etc` Urls
    - `IsMediaOrContentItemUrl` / `IsMediaUrl` / `IsContentAssetUrl` / `IsAttachmentUrl`: Return true if the url matches these formats
    - `GetContentAssetUrlLanguage`: Looks for the language URL parameter and gets the value if found.
- [BaselineObjectExtensionMethods](../../src/Core/Core.Models/Extensions/BaselineObjectExtensionMethods.cs)
  - `TryGetValue`: given a nullable object (or a string which can be null) it adds the `.TryGetValue` nomenclature to it.
- [IEnumerableExtensions](../../src/Core/Core.Models/Extensions/IEnumerableExtensions.cs)
  - `FirstOrMaybe`: If the first exists, returns a Maybe of that, otherwise Maybe.None
  - `TryGetFirst`: If the first exists, returns true and sets the out parameter to the first item.
  - `WithEmptyAsNone`: Similar to FirstOrMaybe but handles a null IEnumerable itself.
  - `StringJoin`: shortcut for string.Join(separator, array);
- [MaybeExtensions](../../src/Core/Core.Models/Extensions/MaybeExtensions.cs)
  - `AsMaybeStatic`: Same as AsMaybe, but returns a copied value since some cases you cannot use the normal AsMaybe()
  - `AsNullable____`: Reverse of converting a Null to Maybe, this converts a Maybe to a Nullable value, useful for serialization
  - `GetValueOrDefault`: Same as Maybe.GetValueOrDefault(), but for nullable objects
  - `WithDefaultAsNone`: Returns a Maybe.None if the value is the type's default value.
  - `WithMatchAsNone`: Returns Maybe.None if the value matches the one you provide.
  - `GetValueOrDefaultIfEmpty`: Checks not only if the Maybe\<string\> has a value, but that it's not empty, shouldn't need as long as you always cast empty strings as Maybe.Nones.
  - `HasNonEmptyValue`: Not widely used, again for Maybe\<string\> with empty value, checks if it has a value that isn't empty.
  - `TryGetValueNonEmpty`: Not widely used, again for Maybe\<string\> with empty value, performs a try get value on it with an empty check
  - `AsMaybeIfTrue`: Returns a Maybe.None if the condition function returns false.
  - `GetValueOrMaybe`: On a dictionary, returns a Maybe if the value is found, this was before the `Dictionary.TryGetValue` was widely available
  - `TryGetValue<T, TSpecificValType>`: Does a TryGetValue on the Maybe, however allows you to set a sub-type in case the Mabye is a DTO model and you really want the child property.
- [ResultExtensions](../../src/Core/Core.Models/Extensions/ResultExtensions.cs)
  - `AsMaybeIfSuccessful`: Converts a Result to a Maybe, again not widely used.
- [IHtmlHelperExtensions](../../src/Core/Core.RCL/Extensions/IHtmlHelperExtensions.cs)
  - `RawWithWrapper`: Will render the raw content, and wrap it if it isn't already wrapped (doesn't start with a `\<`)
  - `AddFileVersionToPath`: Adds a file version to the given URL (uses same service as `asp-append-version`), sometimes needed when `asp-append-version` can't be used
- [TagHelperAttributeListExtensions](../../src/Core/Core.RCL/Extensions/TagHelperAttributeListExtensions.cs)
  - `AddorAppendAttribute`: Adds the given value if not there, or appends it to the end of the existing value (useful for style or classes)
  - `AddorReplaceEmptyAttribute`: Adds the given value if not there OR if there is an attribute but it's empty (otherwise does not add it).  Useful for fallback values.
- [IHeaderDictionaryExtensions](../../src/Core/Core.Library/Extensions/IHeaderDictionaryExtensions.cs)
  - `AddOrReplace`: Adds the value, or replaces it if it exists.
- [IHtmlLocalizerExtensions](../../src/Core/Core.Library/Extensions/IHtmlLocalizerExtensions.cs)
  - `GetHtmlStringOrDefault`: If the given localization key isn't found, returns the default you provide.
- [IStringLocalizerExtensions](../../src/Core/Core.Library/Extensions/IStringLocalizerExtensions.cs)
  - `GetStringOrDefault`: If the given localization key isn't found, returns the default you provide.
- [RoutedWebPageExtensions](../../src/Core/Core.RCL.Xperience/Extensions/RoutedWebPageExtensions.cs)
  - `ToPageIdentity`: Simply there with an absolute and exception for those migrating from Kentico Xperience 13 to Xperience by Kentico where they before could do the TreeNode.ToPageIdentity, tells them how to do it now.
  - `ToTreeCultureIdentity`: Converts the RoutedWebPage into a TreeCultureIdentity for lookups (note: PathChannelLookup does not exist right now on it).
  - `ToTreeIdentity`: Converts the RoutedWebPage into a TreeIdentity for lookups (note: PathChannelLookup does not exist right now on it).
- [ICacheDependencyBuilderExtensions](../../src/Core/Core.Library.Xperience/Extensions/ICacheDependencyBuilderExtensions.cs)
  - (To many to list): These extensions correlate with Xperience by Kentico [Cache Dependency Keys](https://docs.kentico.com/developers-and-admins/development/caching/cache-dependencies), and work with the `ICacheDependencyBuilder builder = ICacheDependencyBuilderFactory.Create()`;
- [ContentTypeQueryParametersExtensions](../../src/Core/Core.Library.Xperience/Extensions/ContentTypeQueryParameters.cs)
  - `In_____Identity/In_____Identities`: Handles a single or array of Identity objects and sets the proper Where Conditions
  - `Path`: Adds the Path filter to the ContentTypeQueryParameters query.

## Special Models

These models are utility models.

- [DTOWithPermissions](../../src/Core/Core.Library.Xperience/Models/DTOWithPermissions.cs): Wrapper 

## Enums

[**CacheMinuteTypes**](../../src/Core/Core.Models/Enums/CacheMinuteTypes.cs): Provides some standard caching types, that you can call and use the `.ToTimeSpan()` or `.ToDouble()` when leveraging for your Cache Durations.  

Example:
```csharp
var results = await _progressiveCache.LoadAsync(async cs => {
  ...
}, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "MyIdentifier", someParameter));
```

## Components

### ConfigurationHelper

This tool helps you display configuration information to your content editors.  It only shows in edit mode, and can help with things like 'Hey, you need to edit this properly and set X value' or 'This page template needs properties set'

```html
@if(string.IsNullOrWhitespace(Model.WidgetProperties.Header)) {
  <configuration-helper x-instruction="Edit this widget and set a header!" x-needs-attention="true" x-mode="Inline" x-visible="true" >
}
// OR
<configuration-helper x-instruction="You can add a header if you edit the widget" x-needs-attention="true" x-mode="Inline" x-visible="(string.IsNullOrWhitespace(Model.WidgetProperties.Header))" >
```

You can also use the [PageBuilderErrorViewExtension](../../src/Core/Core.RCL/Features/PageBuilderError/PageBuilderErrorViewExtension.cs) on a view component to render a simple Configuration Helper message for users if it is not configured properly.

```csharp

public IViewComponentResult Invoke(Array<BannerItems> xBanners) {
  if(!xBanners.Any()) {
    return this.PageBuilderMessage("Must select some banners!");
  }
  return View("/Components/Banners/Banners.cshtml", xBanners);
}

```


### Async Script Helpers

While in the baseline, this is more tied in with the Starter Site, but the Baseline contains the `AsyncScriptFunctions` and `AsyncScriptLoader` tag helpers which add bits of javascript and logic to easily allow you to load ALL javascript after the page load, so no blocking.

```html

<body>
  <head>
    <head>
    @* window.OnScriptsLoaded = All javascript that depends on any library should use window.OnScriptsLoaded(function() { }, identifier) so it will run after the javascript is loaded, identifier is optional to ensure only ran once *@
    @* window.LoadScript = Loading javascript, should be window.LoadScript({src: string, header?: boolean, crossorigin?: string, appendAtEnd?: bool}) *@
    <cache enabled=true expires-after="@CacheMinuteTypes.VeryLong.ToTimeSpan()">
        <vc:async-script-functions />
    </cache>
    </head>
    <body>

    <script>
        OnScriptsLoaded(function () {
            @*
                These are to initialize various functionality pieces
            GlobalMethods.InitializeOtherTools();
            GlobalMethods.InitializeAccessability();
            GlobalMethods.InitializeCanvas();
            GlobalMethods.InitializeRedirectTabs();
                *@
        });
    </script>

    <script>
      /* Will only run once, even if multiple times it is found */
        OnScriptsLoaded(function () {
            GlobalMethods.OnlyRunOnce();
        }, "GlobalOnlyRunOnce");
    </script>

    <script>
        /* Will only run once */
        OnScriptsLoaded(function () {
            GlobalMethods.OnlyRunOnce();
        }, "GlobalOnlyRunOnce");
    </script>

    

     <cache enabled=true expires-after="@CacheMinuteTypes.VeryLong.ToTimeSpan()">
      @* This will run last after all scripts loaded, and will run the Preload methods
         Content should be:

         window.ScriptsLoaded = true;
         for (var queuedScripts = window.PreloadQueue || [], i = 0; i < queuedScripts.length; i++)
         queuedScripts[i]();
      *@
       <vc:async-script-loader x-script-runner-path="@(Html.AddFileVersionToPath("/js/individual/scripts/run-queued-scripts.min.js.gz"))" />
    </cache>
    </body>
 </html>

```

## Middleware

### Gzip and Cache Control File Handling

For performance, Google wants you to cache files for a very, very long time, especially CSS and Javascript files, and use gzip when possible.

[The front end dev system](../front-end-development.md) already handles bundling and gzipping, but you often need to enable this functionality.

the `IServiceCollection.UseGzipAndCacheControlFileHandling()` does two things:

1. Enables handling of .gzip files.
2. Any file that has a ?v= parameter on it (from either `asp-append-version` or `IHtmlHelperExtensions.AddFileVersionToPath`) it will automatically set the max age to 1 year (otherwise 24 hours).

Using these two systems, you'll have very small .gzip files, with a ?v= tag so it will clear on new builds, with very long max ages to make google Happy.
