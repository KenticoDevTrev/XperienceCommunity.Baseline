# Media Handling

Proper Media handling can make a big impact on your site.

**title** attributes help make sure your site is accessable, and SEO friendly

**width** and **height** attributes help prevent Layout Shift as the page loads.

**image resizing** can help make sure you don't serve too large of an image.

The Baseline has systems in place to help foster these systems.

## MediaItem, IMediaMetadata, and MediaMetadataImage

The baseline has 3 models that help you parse out your media.

`MediaItem` Is the Kentico Agnostic DTO for your Media Items.  It contains core properties of the media item, extensions, titles, etc.

It also has an `Maybe<IMediaMetadata> MetaData` property, which you can use to store additional information about your media.

For images, this often will be in the form of the `MediaMetadataImage` which contains the Width, Height, and possibly the Focal Percents (once supported).

## Xperience Configuration

In Xperience by Kentico, Assets are stored in a Field on a Content Type (in the Content Hub).  Because of this, media can exist pretty much **anywhere**, and same goes for information such as the Title or description of an image.

When you use the `IServiceCollection.AddCoreBaseline`, there are 2 optional properties that should be configured.

**MetadataOptions**: The level at which Linked Items are retrieved (default 2)

**ContentItemAssetOptions Action**: This configuration tells the system to find TItle, Description, and what Media types are stored for your Content Asset Items, and if these should be cached (retrieve ALL items and cache, for quicker lookup).  

Any Content Type can have an array of Asset Field Identifiers, so labeling these can be beneficial.

Keep in mind, I also have logic that will still find your Content Item Assets *even if you don't set this* (through parsing the Content Type Structure in the database), the Title and Description though won't have values. 

## Add Custom Metadata
 The Media Metadata is filled through one of two interfaces:
 
 `IMediaFileMetadatProvider` (For Media Files) 
 
 `IContentItemMediaMetadataProvider` (for Content Assets)

 You can overwrite these using [normal Implementation Override](customization-points.md#Implementation-Override), and with it you can grab additional metadata to attach to your Media Items.  The default only handles Images and adds the MediaMetadataImage to those images (including .svg via the viewbox)

## IMediaRepository

The `IMediaRepository` class provides easy helpers to get Content Item Assets and Media Files by their GUID identifier.  This is useful especially if you parse the Guid from a Media URL (see [StringExtensions.ParseGuidFrom____Url](../src/Core/Core.Models/Extensions/StringExtensions.cs)), you can get the MediaItem and it's metadata.

