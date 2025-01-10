# Core Module Tools

Below are a list of all the various Tools that the Core Module provides for Developers.


## Services and Repositories

Below are some Services and Repositories that have not already been mentioned in more detail by a specific documentation.

- [ILogger](../src/Core/Core.Models/Services/ILogger.cs): Kentico Agnostic version of it's own Logging Service
- [IUrlResolver](../src/Core/Core.Models/Services/IUrlResolver.cs): Two methods to make Urls Absolute based on the current request, and also to ResolveUrls (remove those blasted tildes!!!)

## Other Extension Methods

- [StringExtensions](../src/Core/Core.Models/Extensions/StringExtensions.cs)
  - `ParseGuidFromMediaUrl`: Tries to get the Guid value from `/getMedia/GUID/etc` Urls
  - `ParseGuidFromAssetUrl`: Tries to get the ContentItemGuid and FieldGuid value from the `/getContentAsset/ContentItemGUID/FieldGuid/etc` Urls
  - `AsNullOrWhitespaceMaybe`: Returns a `Maybe.None` if the string is null or whitespace
  - `SplitAndRemoveEntries`: Shortcut for `string.Split(delimiterArray, StringSplitOptions.RemoveEmptyEntries)`
  - `RemoveTildeFromFirstSpot`: Shortcut, removes tilde if there (useful in URLs)
  - `MaxLength`: Limits the string to the max length, adding an elipses, useful for summaries.
  - `ToAttributeId`: Converts the given text to an HTML css/id safe attribute (with 'id-' prefixed), useful for client side marking and linking.


## Enums

[**CacheMinuteTypes**](../src/Core/Core.Models/Enums/CacheMinuteTypes.cs): Provides some standard caching types, that you can call and use the `.ToTimeSpan()` or `.ToDouble()` when leveraging for your Cache Durations.  

Example:
```csharp
var results = await _progressiveCache.LoadAsync(async cs => {
  ...
}, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "MyIdentifier", someParameter));
```

