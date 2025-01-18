# Sitemap

Sitemaps are also a common part of websites and should be well supported.

The Navigation contains a [SiteMapController](../../src/Navigation/Navigation.RCL/Features/Sitemap/SiteMapController.cs) that leverages the `ISiteMapRepository` to generate a sitemap.

Becuase Kentico Xperience 13 had Sitemap settings baked in, where as Xperience by Kentico does not, the implementation of retrieving your SiteMapNodes is different.

Additionally, Xperience by Kentico has a little more advanced logic that also handles language sets, Kentico Xperience 13 does not.

## Rendering and `SitemapNode`

If you wish to build out any custom Sitemap rendering, you can use the `SitemapNode` class, and the static function `SitemapNode.GetSitemap(arrayOfSitemapNodes)` which will convert it to the proper format, including the proper xml declarations

```csharp

public async Task<ActionResult> IndexAsync()
{
    var nodes = new List<SitemapNode>();

    // Should customize if you want your own thing, options no longer supported.
    nodes.AddRange(await _siteMapRepository.GetSiteMapUrlSetAsync());

    // Now render manually, sadly the SimpleMVCSitemap disables output cache somehow
    return Content(SitemapNode.GetSitemap(nodes), "text/xml", Encoding.UTF8);
}

```

## Xperience by Kentico

For Xperience by Kentico, the default [ISiteMapRepository](../../src/Navigation/Navigation.Library.XperienceByKentico/Repositories/Implementations/SiteMapRepository.cs) implementation has the following logic for it's methods:

- **GetSiteMapUrlSetAsync**
    - Retrieves all WebPages that inherit the Resuable Field Schema `IBaseMetadata`, as well as any `Navigation` Page Types, filters out any non public items, and then either allows you to customize the casting through your own implementation of `ISiteMapCustomizationService` or uses the default logic to convert.  Additionally adds any custom SitemapNodes from `ISiteMapCustomizationService.GetAdditionalSitemapNodes` that you implement.
- **GetSiteMapUrlSetAsync(SiteMapOptions)**
    - NOT IMPLEMENTED, WILL THROW EXCEPTION.  Just no way with the APIs to accomplish this, and honestly probably not a great way to go about it anyway.

Remember, the default logic works great, but you may need to customize and overwrite the default [ISiteMapCustomizationService Implementation](../../src/Navigation/Navigation.Library.Xperience/Services/Implementations/DefaultSiteMapCustomizationService.cs) to achieve what you wish.

### ISiteMapService
If all else fails, remember you can also use the [ISiteMapService](../../src/Navigation/Navigation.Library.Xperience/Services/ISiteMapService.cs) to convert your own IContentQueryDataContainers, this is the same thing that is used in the default Sitemap logic to parse the items and calls the same customization points for parsing items.


## Kentico Xperience 13

For Kentico Xperience 13, the default [ISiteMapRepository](../../src/Navigation/Navigation.Library.KX13/Repositories/Implementations/SiteMapRepository.cs) implementation has the following logic for it's methods:

- **GetSiteMapUrlSetAsync**
    - Retrieves all TreeNodes on the current site where the `DocumentShowInMenu` is true.
- **GetSiteMapUrlSetAsync**
    - Takes the `SiteMapOptions` and retrieves and parses your pages based on these.

I'm not going to go into details on the properties on the SiteMapOptions as it's only available in Kentico Xperience 13, but you can [view the class and see the comments for all details.](../../src/Navigation/Navigation.Models/Models/SiteMapOptions.cs).