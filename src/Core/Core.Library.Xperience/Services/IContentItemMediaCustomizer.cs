namespace Core.Interfaces
{
    public interface IContentItemMediaCustomizer
    {
        /// <summary>
        /// Allows you to customize the MediaItem, including setting the IMediaMetadata property or altering the title source.  Use in conjuction with a customization to the IContentItemMediaMetadataQueryEditor to retrieve columns needed.
        /// </summary>
        /// <param name="contentQueryDataContainer">The raw Query Data Container</param>
        /// <param name="assetMetadata">The Content Item Asset Metadata</param>
        /// <param name="mediaItem">The generated Media Item</param>
        /// <returns>The IMediaMetadata</returns>
        Task<Result<IMediaMetadata>> GetMediaMetadata(IContentQueryDataContainer contentQueryDataContainer, ContentItemAssetMetadata assetMetadata, MediaItem mediaItem);

        /// <summary>
        /// Allows you to customize the MediaItem itself, such as setting the Title from a differnet field.  This is called after the GetMediaMetadata.  Use in conjuction with a customization to the IContentItemMediaMetadataQueryEditor to retrieve columns needed.
        /// </summary>
        /// <param name="contentQueryDataContainer">The raw Query Data Container</param>
        /// <param name="assetMetadata">The Content Item Asset Metadata</param>
        /// <param name="mediaItem">The generated Media Item</param>
        /// <returns>The Media Item</returns>
        Task<MediaItem> CustomizeMediaItem(IContentQueryDataContainer contentQueryDataContainer, ContentItemAssetMetadata assetMetadata, MediaItem mediaItem);
    }
}
