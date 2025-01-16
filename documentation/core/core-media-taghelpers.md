# Media Tag Helpers

The Baseline has a couple handy Tag Helpers, mainly those found in the [ImageTagHelpers](../../src/Core/Core.RCL/TagHelpers/ImageTagHelper.cs) and 
[OptimizedPictureTagHelper](../../src/Core/Core.RCL/TagHelpers/OptimizedPictureTagHelper.cs).

These help you accomplish various Performance and Accessibility metrics on your site.

## Usage

These tag helpers are in the Core.RCL package, so you can add them via:

`@addTagHelper *, XperienceCommunity.Baseline.Core.RCL`

## Image profiles System

Image profiles define how images should be rendered at different screen sizes.  This way you don't load a 1000 pixel banner image on a 320 pixel wide mobile browser.  Site performance tools will mark this, as well as your users will experience slower load times on mobile networks.

Multiple of our tag helpers leverage these `ImageProfile`s.  When you define them, you want to set your max width (the max-widthless constructor), this should be the largest the image can exist in your format rendering.

Then add additional widths (the 'max width') at the various screen sizes you want to support.  Usually these will be 1 pixel below your media breakpoints.

You can use an int-based Enum class to easily store these values if you wish.

This functionality may eventually be configurable in Xperience by Kentico (in which case the `MediaItem`'s metadata will have them).

## OptimizedPictureTagHelper

If you have any image that is static (part of the template, not in Xperience by Kentico), the [FrontEndDev](../general/front-end-development.md) system has an optimization workflow that takes any images in the `/images/source` location, and creates optimized versions of them.

To leverage, if you add the `bl-optimize` attribute to your image tag, it will check if the image URL starts with `/images/source` and if so, will automatically convert it to a `picture` tag with `sources` for the optimized versions, so browsers can select the more optimized element.

``` html
<img src="/images/sources/banner.jpg" bl-optimize />
```

Gets converted to

``` html
<picture>
    <source srcset="/images/webp/banner.webp" type="image/webp"    />
    <source srcset="/images/optimized/banner.jpg" type="image/jpeg"    />
    <img src="/images/source/banner.jpg" />
</picture>
```

## ImageTagHelper

The baseline uses a generic `MediaItem` to represent media.  You can take one of these media items and use it to generate your image tags by simply passing the media item to the `bl-media` attribute for `img` tags.

``` html
<img bl-media=@MyMediaItem>
```
Gets converted to

``` html
<img src="/getcontentasset/5aa6874d-96a3-4bd3-9f6b-eb0a28cabe45/a28a7e27-4593-40cf-8266-2ae4101723f5/Testing-Logo.png?language=en" alt="Here's some description" />
```

## ImageMediaMetadataTagHelper

This tag helper takes any `img` tag and checks to see if it's a media item (`/getmedia`, `/getcontentasset`, `/getattachment`), and from there gets the GUID value(s) and parses.

It then sets the alt tag appropriately (if it's not already set).

Additionally, if this image tag was generated through `bl-media` or you include `bl-image-profiles` values, it will apply those image profiles to the image (as well as rendering the optimize `webp` for Xperience by Kentico)

```html
@{
    var imageProfiles = new ImageProfile[] {
        new ImageProfile(1200), // Default Width (1024+)
        new ImageProfile(600, 1023), // 600 wide for screen sizes 640-1023
        new ImageProfile(300, 639), // 300 wide for screen sizes 0-639
    };
}
<img src="/getcontentasset/5aa6874d-96a3-4bd3-9f6b-eb0a28cabe45/a28a7e27-4593-40cf-8266-2ae4101723f5/Testing-Logo.png?language=en" bl-image-profiles="@imageProfiles" />
```

Gets converted to

```html
    <picture>
        <source media="(max-width:639px)" srcset="/getcontentasset/5aa6874d-96a3-4bd3-9f6b-eb0a28cabe45/a28a7e27-4593-40cf-8266-2ae4101723f5/Testing-Logo.png?language=en&width=375&height=375&format=webp" type="image/webp" width="300" height="300" />
        <source media="(max-width:1023px)" srcset="/getcontentasset/5aa6874d-96a3-4bd3-9f6b-eb0a28cabe45/a28a7e27-4593-40cf-8266-2ae4101723f5/Testing-Logo.png?language=en&width=750&height=750&format=webp" type="image/webp" width="600" height="600" />
        <img src="/getcontentasset/5aa6874d-96a3-4bd3-9f6b-eb0a28cabe45/a28a7e27-4593-40cf-8266-2ae4101723f5/Testing-Logo.png?language=en&amp;width=1500&amp;height=1500" alt="Here&#x27;s some description" width="1200" height="1200" />
    </picture>
```

## SourceMediaMetadataTagHelper

Less used, this provides the same functionality of the `ImageMediaMetadataTagHelper` except on `source` tags with `srcset`, ensuring the `alt` tags are properly set.  This isn't needed however in most cases.

## BackgroundMediaTagHelper
Similar to the ImageMediaMetadataTagHelper's Image Profile system, this tag helper can be applied to any tag via the `bl-background-image-profiles` (passing it an array of your `ImageProfiles`).

This will analyze the tag for a style tag of `background-image` (must be ON the tag itself), and if it finds it and the type is one that allows dynamic rendering, it will output a style tag that will alter the background-image based on the width profiles.

```html
@{
    var imageProfiles = new ImageProfile[] {
        new ImageProfile(1200), // Default Width (1024+)
        new ImageProfile(600, 1023), // 600 wide for screen sizes 640-1023
        new ImageProfile(300, 639), // 300 wide for screen sizes 0-639
    };
}
<div bl-background-image-profiles="@imageProfiles" style="background-image: url(/getcontentasset/5aa6874d-96a3-4bd3-9f6b-eb0a28cabe45/a28a7e27-4593-40cf-8266-2ae4101723f5/Testing-Logo.png?language=en); width: 100%; height:500px;">
    Some Background-imaged section
</div>
```

Gets converted to

```html

<div style="background-image: url(/getcontentasset/5aa6874d-96a3-4bd3-9f6b-eb0a28cabe45/a28a7e27-4593-40cf-8266-2ae4101723f5/Testing-Logo.png?language=en); width: 100%; height:500px;" data-imageprofile-id="6f61f515-a88e-463e-821d-f0ec0b53a779">
    <style>
        @media (max-width:1023px) { 
            [data-imageprofile-id='6f61f515-a88e-463e-821d-f0ec0b53a779'] {
                background-image: url('/getcontentasset/5aa6874d-96a3-4bd3-9f6b-eb0a28cabe45/a28a7e27-4593-40cf-8266-2ae4101723f5/Testing-Logo.png?language=en&width=750&height=750') !important; 
            } 
        } 
        @media (max-width:639px) { 
            [data-imageprofile-id='6f61f515-a88e-463e-821d-f0ec0b53a779'] { 
                background-image: url('/getcontentasset/5aa6874d-96a3-4bd3-9f6b-eb0a28cabe45/a28a7e27-4593-40cf-8266-2ae4101723f5/Testing-Logo.png?language=en&width=375&height=375') !important; 
            } 
        } 
    </style>
</div>
```
Note that in this case, WebP format is not used as there isn't a way to fall back to the original version if it's not compatible.

## Image Scale and options

Because of screen resolutions (and browser UI scaling), an image may be 300 pixels, be rendered in screen as 300 pixels, but due to scaling may actually be upscaled to a higher pixel level, resulting in blurry images.

To counteract this, the actual rendered image resolution can be scaled.

There is an `MediaTagHelperOptions` that you can set the MediaTagHelperOptions in the `IServiceCollection.AddCoreBaseline` which can control this Image Resolution Scaling.

For an image that is 400 pixels wide:

- 1 = 100%, 400 pixels wide and image set to be 400 pixels wide.
- 1.25 = 125%, 500 pixels wide rendered but image set to be 400 pixels wide.
- 1.5 = 150%, 600 pixel wide rendered but image set to be 400 pixels wide.

There are other options you can set in the `MediaTagHelperOptions`, including disabling this functionality all together on the img tags, and also controlling what image extensions support dynamic resizing.

## Widget / string HTML Content

Tag Helpers often do not work with rendered text through the `@Html.Raw()`.  While this is not in the Baseline yet, I will be adding a version of these that can parse and update HTML in a raw string.