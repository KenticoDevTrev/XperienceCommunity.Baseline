using CMS.MediaLibrary;

namespace Core.Services
{
    public interface IMediaFileMediaMetadataProvider
    {
        [Obsolete("Media Library is now Obsolete in Xperience by Kentico.  Migrate to Content Items")]
        public Task<Result<IMediaMetadata>> GetMediaMetadata(MediaFileInfo mediaFileInfo, MediaItem mediaItem);
    }
}
