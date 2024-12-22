using CMS.MediaLibrary;

namespace Core.Services
{
    public interface IMediaFileMediaMetadataProvider
    {
        public Task<Result<IMediaMetadata>> GetMediaMetadata(MediaFileInfo mediaFileInfo, MediaItem mediaItem);
    }
}
