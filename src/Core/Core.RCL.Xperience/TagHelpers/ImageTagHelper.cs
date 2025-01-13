using Core.Models;
using Core.Repositories;
using Core.Services;
using CSharpFunctionalExtensions;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Core.TagHelpers
{
    [HtmlTargetElement("img", Attributes = "bl-media")]
    public class ImageTagHelper(IMediaRepository mediaRepository) : TagHelper
    {
        private readonly IMediaRepository _mediaRepository = mediaRepository;

        public override int Order => -2;
        public MediaItem? blMedia { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            if (blMedia.TryGetValue(out var mediaItem)) {
                output.Attributes.SetAttribute("src", mediaItem.MediaPermanentUrl);
                output.Attributes.AddorReplaceEmptyAttribute("alt", mediaItem.MediaDescription.GetValueOrDefault(string.Empty));

                if (
                    mediaItem.MediaPermanentUrl.IsMediaOrContentItemUrl()
                        && mediaItem.MetaData.TryGetValue(out var metaData) && metaData is MediaMetadataImage imageMetadata
                        && imageMetadata.ImageProfiles.Any()) {
                    // Add data-attribute for ImageMediaMetadataTagHelper to handle
                    output.Attributes.Add("data-image-profiles", imageMetadata.ImageProfiles);
                }
            }
        }
    }


    [HtmlTargetElement("img", Attributes = "src")]
    public class ImageMediaMetadataTagHelper(IMediaRepository mediaRepository,
        IMediaTagHelperService mediaTagHelperService) : TagHelper
    {
        private readonly IMediaRepository _mediaRepository = mediaRepository;
        private readonly IMediaTagHelperService _mediaTagHelperService = mediaTagHelperService;

        [HtmlAttributeName("bl-image-profiles")]
        public IEnumerable<ImageProfile> ImageProfiles { get; set; } = [];

        public override int Order => -1;
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);

            if (output.Attributes.ContainsName("data-image-profiles")) {
                try {
                    ImageProfiles = (ImageProfile[])output.Attributes["data-image-profiles"].Value;
                } catch (Exception) {
                    // Ignore
                }
            }
            if(output.Attributes.TryGetAttribute("src", out var srcAttribute) 
                && !string.IsNullOrWhiteSpace(srcAttribute.Value?.ToString() ?? string.Empty)
                && (await _mediaRepository.GetMediaItemFromUrl(srcAttribute.Value?.ToString() ?? string.Empty)).TryGetValue(out var mediaItem)) { 
                _mediaTagHelperService.HandleMediaItemProfilesAndMetaData(output, mediaItem, "src", ImageProfiles);
            }
        }
    }

    [HtmlTargetElement("*", Attributes = "bl-background-image-profiles")]
    public class BackgroundMediaTagHelper(IMediaRepository mediaRepository,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        IMediaTagHelperService mediaTagHelperService) : TagHelper
    {
        private readonly IMediaRepository _mediaRepository = mediaRepository;
        private readonly IPreferredLanguageRetriever _preferredLanguageRetriever = preferredLanguageRetriever;
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
                && (bgImage ?? "").IsMediaOrContentItemUrl()
                && (await _mediaRepository.GetMediaItemFromUrl(bgImage ?? "")).TryGetValue(out var mediaItem)) {
                _mediaTagHelperService.HandleBackgroundImageProfiles(output, mediaItem, bgImage ?? "", ImageProfiles);
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

            if(output.Attributes.TryGetAttribute("srcset", out var attribute) &&
                (await _mediaRepository.GetMediaItemFromUrl(attribute.Value?.ToString() ?? string.Empty)).TryGetValue(out var mediaItem)) {
                _mediaTagHelperService.HandleMediaItemProfilesAndMetaData(output, mediaItem, "srcset", Array.Empty<ImageProfile>());
            }
        }
    }

    
}
