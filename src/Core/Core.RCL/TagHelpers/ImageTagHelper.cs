using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Core.TagHelpers
{
    [HtmlTargetElement("img", Attributes = "bl-media")]
    public class ImageTagHelper(IMediaRepository mediaRepository) : TagHelper
    {
        private readonly IMediaRepository _mediaRepository = mediaRepository;

        public override int Order => -20;
        public MediaItem? blMedia { get; set; }

        [HtmlAttributeName("bl-image-profiles")]
        public IEnumerable<ImageProfile> ImageProfiles { get; set; } = [];

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            if (blMedia.TryGetValue(out var mediaItem)) {
                output.Attributes.SetAttribute("src", mediaItem.MediaPermanentUrl);
                output.Attributes.AddorReplaceEmptyAttribute("alt", mediaItem.MediaDescription.GetValueOrDefault(mediaItem.MediaTitle));
                output.Attributes.Add("data-media-item", mediaItem);
                if (
                    mediaItem.MediaPermanentUrl.IsMediaOrContentItemUrl()
                        && mediaItem.MetaData.TryGetValue(out var metaData) && metaData is MediaMetadataImage imageMetadata
                        && (ImageProfiles.Any() || imageMetadata.ImageProfiles.Any())) {
                    // Add data-attribute for ImageMediaMetadataTagHelper to handle
                    output.Attributes.Add("data-image-profiles", ImageProfiles.Any() ? ImageProfiles : imageMetadata.ImageProfiles);
                }
            }
        }
    }


    [HtmlTargetElement("img")]
    public class ImageMediaMetadataTagHelper(IMediaRepository mediaRepository,
        IMediaTagHelperService mediaTagHelperService,
        MediaTagHelperOptions mediaTagHelperOptions) : TagHelper
    {
        private readonly IMediaRepository _mediaRepository = mediaRepository;
        private readonly IMediaTagHelperService _mediaTagHelperService = mediaTagHelperService;
        private readonly MediaTagHelperOptions _mediaTagHelperOptions = mediaTagHelperOptions;

        [HtmlAttributeName("bl-image-profiles")]
        public IEnumerable<ImageProfile> ImageProfiles { get; set; } = [];

        public override int Order => -10;
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);
            if (!_mediaTagHelperOptions.UseMediaTagHelper) {
                return;
            }

            if (output.Attributes.ContainsName("data-image-profiles")) {
                try {
                    ImageProfiles = (ImageProfile[])output.Attributes["data-image-profiles"].Value;
                    output.Attributes.Remove(output.Attributes["data-image-profiles"]);
                } catch (Exception) {
                    // Ignore
                }
            }

            if (output.Attributes.TryGetAttribute("src", out var srcAttribute)) {
                if (output.Attributes.ContainsName("data-media-item") && output.Attributes["data-media-item"].Value is MediaItem attributeMediaItem) {
                    output.Attributes.Remove(output.Attributes["data-media-item"]);
                    _mediaTagHelperService.HandleMediaItemProfilesAndMetaData(output, attributeMediaItem, "src", ImageProfiles);
                } else if (!string.IsNullOrWhiteSpace(srcAttribute.Value?.ToString() ?? string.Empty)
                    && (await _mediaRepository.GetMediaItemFromUrl(srcAttribute.Value?.ToString() ?? string.Empty)).TryGetValue(out var mediaItem)) {
                    _mediaTagHelperService.HandleMediaItemProfilesAndMetaData(output, mediaItem, "src", ImageProfiles);
                }
            }
        }
    }

    [HtmlTargetElement("*", Attributes = "bl-background-image-profiles")]
    public class BackgroundMediaTagHelper(IMediaRepository mediaRepository,
        IMediaTagHelperService mediaTagHelperService) : TagHelper
    {
        private readonly IMediaRepository _mediaRepository = mediaRepository;
        private readonly IMediaTagHelperService _mediaTagHelperService = mediaTagHelperService;

        [HtmlAttributeName("bl-background-image-profiles")]
        public IEnumerable<ImageProfile> ImageProfiles { get; set; } = [];

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (!ImageProfiles.Any()) {
                return;
            }

            var styleDictionary = _mediaTagHelperService.GetStyleDictionary(output);
            if (
                styleDictionary.TryGetValue("background-image", out var bgImage)
                && _mediaTagHelperService.GetImageUrlFromBackgroundStyle(bgImage?.ToString() ?? "").TryGetValue(out var url)
                && (await _mediaRepository.GetMediaItemFromUrl(url)).TryGetValue(out var mediaItem)) {
                _mediaTagHelperService.HandleBackgroundImageProfiles(output, mediaItem, url, ImageProfiles);
            }
        }
    }

    [HtmlTargetElement("source", Attributes = "srcset")]
    public class SourceMediaMetadataTagHelper(IMediaTagHelperService mediaTagHelperService, IMediaRepository mediaRepository) : TagHelper
    {
        private readonly IMediaTagHelperService _mediaTagHelperService = mediaTagHelperService;
        private readonly IMediaRepository _mediaRepository = mediaRepository;

        public override int Order => 100;
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (output.Attributes.TryGetAttribute("srcset", out var attribute) &&
                (await _mediaRepository.GetMediaItemFromUrl(attribute.Value?.ToString() ?? string.Empty)).TryGetValue(out var mediaItem)) {
                _mediaTagHelperService.HandleMediaItemProfilesAndMetaData(output, mediaItem, "srcset", []);
            }
        }
    }


}
