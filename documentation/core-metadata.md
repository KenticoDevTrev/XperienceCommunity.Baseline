# Page Metadata

Web Pages need metadata in order help search enginers and users find and share your pages.

The Baseline has a `PageMetaData` model and various retrieval systems to help get and render this information, in many cases automatically.

## PageMetaData

The `PageMetaData` class contains the core elements of your page.

- Title
- Keywords
- Description
- Thumbnail (url of the OG Image)
- CanonicalUrl
- NoIndex (if true, then robots should not scan)

You can create your own model, or retrieve it based on the current page and/or the specified page.


## Xperience by Kentico IBaseMetadata Reusable Schema

Since Xperience by Kentico doesn't have the Metadata OOB, the baseline includes the IBaseMetadata which you can apply to any page type, this will provide the fields needed for rendering.

## IMetaDataRepository

This retriever allows you to get the current page or the specified page's MetaData.  There is an optional Thumbnail property that you can set to add the thumbnail, since SEO image was not readily available OOB in KX13, and in many cases this image may be something custom with each content type (ie a Blog Article's Hero Image, or a Product's image).

### Kentico Xperience 13

Data is retrieved through a fallback:

1. Check DocumentCustomData for...
    - MetaData_ThumbnailSmall
    - MetaData_Keywords
    - MetaData_Description
    - MetaData_Title
    - MetaData_NoIndex
2. Then, if nothing there, fall back to...
    - CMS_Document.DocumentPageKeyWords
    - CMS_Document.DocumentPageDescription
    - CMS_Document.DocumentPageTitle
3. Lastly, if no PageTitle, it would fall back to...
    - CMS_Document.DocumentName

It would use the UrlRetriever to get the Relative Url as the canonical link.

### Xperience by Kentico

1. Checks if the page is of type `IBaseMetadata` for...
    - MetaData_Title
    - MetaData_Description
    - MetaData_Keywords
    - MetaData_NoIndex
    - MetaData_OGImage
2. Then, if nothing there, falls back to...
    - ContentItemLanguageMetadataDisplayName (for Title)

## Rendering Metadata (ViewComponents)

To render Metadata, the Baseline comes with 2 components:

**ManualPageMetadata**

This component allows you to pass a manually created `MetaData` model.

```html
<manual-page-metadata x-meta-data=@MyMetadata />
```

**PageMetaData**

This page automatically looks to the current page context and renders out the metadata (as long as the Manual Page Metadata was not already called)

```html
<page-meta-data />
```