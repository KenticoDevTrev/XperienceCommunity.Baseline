using AngleSharp.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Core.Repositories.Implementation
{
    public class ContentItemMediaCustomizer(IProgressiveCache progressiveCache, IUrlResolver urlResolver, IHttpClientFactory HttpClientFactory) : IContentItemMediaCustomizer
    {
        private static readonly string[] _imageExtensions = ["png", "gif", "bmp", "jpg", "jpeg", "webp"];

        public IProgressiveCache ProgressiveCache { get; } = progressiveCache;
        public IUrlResolver UrlResolver { get; } = urlResolver;

        public Task<MediaItem> CustomizeMediaItem(IContentQueryDataContainer contentQueryDataContainer, ContentItemAssetMetadata assetMetadata, MediaItem mediaItem)
        {
            /*
            // Can clone and use DI to replace this to do customizations, including doing type casts.  This is often coupled with a custom IContentItemMediaMetadataQueryEditor implementation
            if (contentQueryDataContainer.ContentTypeName.Equals(Generic.File.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
                Custom.PdfFile file = ContentQueryResultMapper.Map<Generic.PdfFile>(contentQueryDataContainer);
                // access to data directly with cast.  Can also GetValue without this though.
                return mediaItem with { MediaTitle = file.FileName };
            }
            */

            return Task.FromResult(mediaItem);
        }

        public async Task<Result<IMediaMetadata>> GetMediaMetadata(IContentQueryDataContainer contentQueryDataContainer, ContentItemAssetMetadata assetMetadata, MediaItem mediaItem)
        {
            /*
            // Can clone and use DI to replace this to do customizations, including doing type casts.  This is often coupled with a custom IContentItemMediaMetadataQueryEditor implementation
            if (contentQueryDataContainer.ContentTypeName.Equals(Generic.File.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
                Custom.PdfFile file = ContentQueryResultMapper.Map<Generic.PdfFile>(contentQueryDataContainer);
                // access to data directly with cast.  Can also GetValue without this though.
                return new PdfMediaMetadata(Owner: file.PdfOwner);
            }
            */

            var extension = mediaItem.MediaExtension.Trim('.').ToLower();
            if (_imageExtensions.Contains(extension)) {
                // although hopefully in the future we can simply get the width and height from the ContentItemAssetMetadata, stop gap is to read the byte data and parse.
                return await ProgressiveCache.LoadAsync(async cs => {
                    try {
                        var imageBytes = await HttpClientFactory.CreateClient().GetByteArrayAsync(
                            UrlResolver.GetAbsoluteUrl(mediaItem.MediaPermanentUrl)
                        );
                        var (width, height) = ImageHelper.GetImageDimensions(imageBytes);
                        if (width == 0 && height == 0) {
                            return Result.Failure<IMediaMetadata>("Could not parse width height from image bytes");
                        }
                        return new MediaMetadataImage(width, height);
                    } catch (Exception ex) {
                        return Result.Failure<IMediaMetadata>($"Error retrieving or parsing Svg {mediaItem.MediaPermanentUrl}: {ex.Message}.");
                    }
                }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetMediaWidthHeight", assetMetadata.Identifier, assetMetadata.LastModified));
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
                        if (doc != null && doc.Root != null && doc.Root.Attributes().Where(x => x.Name.LocalName.Equals("viewBox", StringComparison.OrdinalIgnoreCase)).FirstOrMaybe().TryGetValue(out var viewBox)) {
                            var splitVals = viewBox.Value.Split(' ');
                            if (splitVals.Length == 4
                                && int.TryParse(splitVals[0], out var widthStart)
                                && int.TryParse(splitVals[1], out var heightStart)
                                && int.TryParse(splitVals[2], out var widthEnd)
                                && int.TryParse(splitVals[3], out var heightEnd)
                            ) {
                                return new MediaMetadataImage(widthEnd - widthStart, heightEnd - heightStart);
                            }
                        }
                    } catch (Exception ex) {
                        return Result.Failure<IMediaMetadata>($"Error retrieving or parsing Svg {mediaItem.MediaPermanentUrl}: {ex.Message}.");
                    }
                    return Result.Failure<IMediaMetadata>($"Could not parse Svg and get view port.");
                }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetSVGViewport", assetMetadata.Identifier, assetMetadata.LastModified));
            }

            return Result.Failure<IMediaMetadata>($"Extension {extension} not mappable to any IMediaMetadata");
        }
    }
}
