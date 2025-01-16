using CMS.MediaLibrary;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Core.Repositories.Implementation
{
    public class MediaFileMediaMetadataProvider(IProgressiveCache progressiveCache, IUrlResolver urlResolver, IHttpClientFactory HttpClientFactory) : IMediaFileMediaMetadataProvider
    {
        private static readonly string[] _imageExtensions = ["png", "gif", "bmp", "jpg", "jpeg", "webp"];

        public IProgressiveCache ProgressiveCache { get; } = progressiveCache;
        public IUrlResolver UrlResolver { get; } = urlResolver;

        public async Task<Result<IMediaMetadata>> GetMediaMetadata(MediaFileInfo mediaFileInfo, MediaItem mediaItem)
        {
            var extension = mediaFileInfo.FileExtension.Trim('.').ToLower();
            if (_imageExtensions.Contains(extension)) { 
                return Result.Success<IMediaMetadata>(new MediaMetadataImage(mediaFileInfo.FileImageWidth, mediaFileInfo.FileImageHeight));
            }
            if (extension.Equals("svg")) {
                // attempt to read svg data and get viewport for width / height
                return await ProgressiveCache.LoadAsync(async cs => {
                    try {
                        var doc = XDocument.Parse(
                            await HttpClientFactory.CreateClient().GetStringAsync(
                                UrlResolver.GetAbsoluteUrl(mediaItem.MediaPermanentUrl)
                            )
                        );
                        var element = doc.XPathSelectElement("svg");
                        if (doc != null && doc.Root != null && doc.Root.Attributes().Where(x => x.Name.LocalName.Equals("viewBox", StringComparison.OrdinalIgnoreCase)).TryGetFirst(out var viewBox)) {
                            var splitVals = viewBox.Value.Split(' ');
                            if (splitVals.Length == 4
                                && int.TryParse(splitVals[0], out var widthStart)
                                && int.TryParse(splitVals[1], out var heightStart)
                                && int.TryParse(splitVals[2], out var widthEnd)
                                && int.TryParse(splitVals[3], out var heightEnd)
                            ) {
                                return new MediaMetadataImage((widthEnd - widthStart), (heightEnd - heightStart));
                            }
                        }
                    } catch(Exception ex) {
                        return Result.Failure<IMediaMetadata>($"Error retrieving or parsing Svg {mediaItem.MediaPermanentUrl}: {ex.Message}.");
                    }
                    return Result.Failure<IMediaMetadata>($"Could not parse Svg and get view port.");
                }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetSVGViewport", mediaFileInfo.FileGUID, mediaFileInfo.FileModifiedWhen));
            }

            return Result.Failure<IMediaMetadata>($"Extension {extension} not mappable to any IMediaMetadata");
        }
    }
}
