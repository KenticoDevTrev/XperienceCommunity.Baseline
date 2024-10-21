using AngleSharp.Text;
using CMS.ContentEngine;
using Core.Enums;
using Core.Interfaces;
using Core.Services;
using MVCCaching;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Core.Repositories.Implementation
{
    public class CustomContentItemMediaMetadataProvider(IProgressiveCache progressiveCache, IUrlResolver urlResolver, IHttpClientFactory HttpClientFactory, IContentQueryResultMapper contentQueryResultMapper) : IContentItemMediaMetadataProvider
    {
        private static readonly string[] _imageExtensions = ["png", "gif", "bmp", "jpg", "jpeg", "webp"];

        public IProgressiveCache ProgressiveCache { get; } = progressiveCache;
        public IUrlResolver UrlResolver { get; } = urlResolver;
        public IContentQueryResultMapper ContentQueryResultMapper { get; } = contentQueryResultMapper;

        public async Task<Result<IMediaMetadata>> GetMediaMetadata(IContentQueryDataContainer contentQueryDataContainer, ContentItemAssetMetadata assetMetadata, MediaItem mediaItem)
        {
            if (contentQueryDataContainer.ContentTypeName.Equals(Generic.File.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
                var file = ContentQueryResultMapper.Map<Generic.File>(contentQueryDataContainer);
                // access to data directly with cast.  Can also GetValue without this though.
            }

            // Can clone and modify as you wish, including doing type casts.
            var extension = mediaItem.MediaExtension.Trim('.').ToLower();
            if (_imageExtensions.Contains(extension)) {
                // although hopefully in the future we can simply get the width and height from the ContentItemAssetMetadata, stop gap is to read the byte data and parse.
                return await ProgressiveCache.LoadAsync(async cs => {
                    try {
                        var imageBytes = await HttpClientFactory.CreateClient().GetByteArrayAsync(
                            UrlResolver.GetAbsoluteUrl(mediaItem.MediaPermanentUrl)
                        );
                        var widthHeight = ImageHelper.GetImageDimensions(imageBytes);
                        if (widthHeight.width == 0 && widthHeight.height == 0) {
                            return Result.Failure<IMediaMetadata>("Could not parse width height from image bytes");
                        }
                        return new MediaMetadataImage(widthHeight.width, widthHeight.height);
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
                                return new MediaMetadataImage((widthEnd - widthStart), (heightEnd - heightStart));
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
