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

## Add Custom Metadata
 The Media Metadata is filled through one of two interfaces:
 
 `IMediaFileMetadatProvider` (For Media Files) 
 
 `IContentItemMediaMetadataProvider` (for Content Assets)

 You can overwrite these using [normal Implementation Override](customization-points.md#Implementation-Override), and with it you can grab additional metadata to attach to your Media Items.

## IMediaRepository