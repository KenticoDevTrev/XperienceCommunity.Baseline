using CMS.MediaLibrary;

namespace Core.Services
{
    [Obsolete("Media Items are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAssets(mediaFileGuid.Select(x => x.ToObjectIdentity())). Please see https://docs.kentico.com/guides/architecture/media-libraries-migration-guidance to migrate.")]
    public interface IMediaFileMediaMetadataProvider
    {
        [Obsolete("Media Items are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAssets(mediaFileGuid.Select(x => x.ToObjectIdentity())). Please see https://docs.kentico.com/guides/architecture/media-libraries-migration-guidance to migrate.")]
        public Task<Result<IMediaMetadata>> GetMediaMetadata(MediaFileInfo mediaFileInfo, MediaItem mediaItem);
    }
}
