namespace Core.Interfaces
{
    public interface IContentItemMediaMetadataProvider
    {
        Task<Result<IMediaMetadata>> GetMediaMetadata(IContentQueryDataContainer contentQueryDataContainer, ContentItemAssetMetadata assetMetadata, MediaItem mediaItem);
    }
}
