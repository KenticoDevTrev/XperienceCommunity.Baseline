using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.RegularExpressions;
using System.Web;

namespace Core.Services.Implementation
{
    public class MediaTagHelperService : IMediaTagHelperService
    {
        private readonly IMediaRepository _mediaRepository;

        public MediaTagHelperService(IMediaRepository mediaRepository)
        {
            _mediaRepository = mediaRepository;
        }
        public async Task<Result<MediaItem>> GetMediaItemFromUrl(string url)
        {
            if (url.IsMediaUrl() && url.ParseGuidFromMediaUrl().TryGetValue(out var mediaMediaGuid)
                && (await _mediaRepository.GetMediaItemAsync(mediaMediaGuid)).TryGetValue(out var mediaMediaItem)) {
                return mediaMediaItem;
            }
            if (url.IsContentAssetUrl() && url.ParseGuidFromAssetUrl().TryGetValue(out var mediaContentAssetGuid)
                && (await _mediaRepository.GetContentItemAsset(mediaContentAssetGuid.ContentItemGuid.ToContentIdentity(), mediaContentAssetGuid.FieldGuid, url.GetContentAssetUrlLanguage().AsNullableValue())).TryGetValue(out var contentMediaItem)) {
                return contentMediaItem;
            }
            return Result.Failure<MediaItem>("Url is not a /getmedia or /getcontentasset url.");
        }

        public void HandleBackgroundImageProfiles(TagHelperOutput output, MediaItem mediaItem, string fullUrl, IEnumerable<ImageProfile> imageProfiles)
        {
            // We now have our value to parse
            var getMediaUrl = Regex.Match(fullUrl, @"^url\([ '""]{0,1}((.*))['""]{0,1}\)");

            if (!getMediaUrl.Success || getMediaUrl.Groups.Count < 2) {
                return;
            }

            // Second group should be the URL
            var src = getMediaUrl.Groups[1].Value.TrimEnd('\'').TrimEnd('"').TrimStart('~');

            // add random id to element
            string id = Guid.NewGuid().ToString();
            output.Attributes.Add("data-imageprofile-id", id);

            Maybe<int> widthOverride = Maybe.None;
            Maybe<int> heightOverride = Maybe.None;
            Maybe<string> languageOverride = Maybe.None;


            // Overrides
            if (src.Contains('?')) {
                var splitSrc = src.Split('?')[1].Split('&');
                if (splitSrc.FirstOrMaybe(x => x.StartsWith("width", StringComparison.OrdinalIgnoreCase)).TryGetValue(out var widthParam)
                    && widthParam.Contains('=')
                    && int.TryParse(widthParam.Split('=')[1], out var width)) {
                    widthOverride = width;
                }
                if (splitSrc.FirstOrMaybe(x => x.StartsWith("height", StringComparison.OrdinalIgnoreCase)).TryGetValue(out var heightParam)
                    && heightParam.Contains('=')
                    && int.TryParse(heightParam.Split('=')[1], out var height)) {
                    heightOverride = height;
                }
                if (splitSrc.FirstOrMaybe(x => x.StartsWith("language", StringComparison.OrdinalIgnoreCase)).TryGetValue(out var languageParam)
                    && languageParam.Contains('=')) {
                    languageOverride = languageParam.Split('=')[1];
                }
            }
            int originalWidth = 0;
            int originalHeight = 0;


            if (mediaItem.MetaData.TryGetValue(out var mediaItemMetadata)
                && mediaItemMetadata is MediaMetadataImage imageMetadata) {
                decimal originalRatio = Convert.ToDecimal(imageMetadata.Width) / Convert.ToDecimal(imageMetadata.Height > 0 ? imageMetadata.Height : 1);
                // rare case where width is determined by height
                if (widthOverride.HasNoValue && heightOverride.HasValue) {
                    originalWidth = Convert.ToInt32(Math.Round(Convert.ToDecimal(heightOverride.Value) * originalRatio));
                } else {
                    originalWidth = widthOverride.GetValueOrDefault(imageMetadata.Width);
                    originalHeight = CalculateHeight(originalWidth, originalRatio);
                }
            }

            // need width and height to continue
            if (originalWidth <= 0 || originalHeight <= 0) {
                return;
            }

            var styleDictionary = GetStyleDictionary(output);

            // Handle image profile main override
            if (imageProfiles.FirstOrMaybe(x => x.MaxScreenWidth.HasNoValue).TryGetValue(out var defaultImageProfile)) {
                var ratio = Convert.ToDecimal(originalWidth) / Convert.ToDecimal(originalHeight);

                // Update source
                var height = CalculateHeight(defaultImageProfile.ImageRenderWidth, ratio);
                styleDictionary["background-image"] = $"url('{ReplaceWidthHeightOnSource(src, defaultImageProfile.ImageRenderWidth, height)}')";
                imageProfiles = imageProfiles.Except([defaultImageProfile]).ToList();
                output.Attributes.AddorReplaceEmptyAttribute("style", string.Join("; ", styleDictionary.Select(elem => $"{elem.Key}: {elem.Value}")));
            }

            // Handle image profiles
            if (imageProfiles.Where(x => x.MaxScreenWidth.HasValue).Any()) {
                decimal ratio = Convert.ToDecimal(originalWidth) / Convert.ToDecimal(originalHeight);
                output.PostContent.AppendHtml("<style>");

                foreach (var imageProfile in imageProfiles.Where(x => x.MaxScreenWidth.HasValue).OrderByDescending(x => x.MaxScreenWidth.Value)) {
                    var height = CalculateHeight(imageProfile.ImageRenderWidth, ratio);
                    output.PostContent.AppendHtml($"@media (max-width:{imageProfile.MaxScreenWidth.Value}px) {{ [data-imageprofile-id='{id}'] {{ background-image: url('{ReplaceWidthHeightOnSource(src, imageProfile.ImageRenderWidth, height)}') !important; }} }} {Environment.NewLine}");
                }
                output.PostContent.AppendHtml("</style>");
            }
        }

        public Dictionary<string, string?> GetStyleDictionary(TagHelperOutput output) {
            if(!output.Attributes.TryGetAttribute("style", out var styleAttr)) {
                return [];
            }

            return (styleAttr.Value?.ToString() ?? "").SplitAndRemoveEntries(";").Select(x => {
                var items = x.Trim().Split(':');
                return new Tuple<string, string?>(items[0].Trim(), items.Length > 0 ? items[1].Trim() : null);
            })
            .GroupBy(x => x.Item1)
            .ToDictionary(key => key.Key, value => value.First().Item2);
        }

        public void HandleMediaItemProfilesAndMetaData(TagHelperOutput output, MediaItem mediaItem, string sourceAttributeName, IEnumerable<ImageProfile> imageProfiles)
        {
            if (!output.Attributes.ContainsName(sourceAttributeName)) {
                return;
            }
            bool isGetMedia = false;

            var src = HttpUtility.HtmlDecode(output.Attributes[sourceAttributeName].Value?.ToString() ?? "");
            Maybe<MediaMetadataImage> metaDataFound = Maybe.None;

            Maybe<int> widthOverride = Maybe.None;
            Maybe<int> heightOverride = Maybe.None;
            if (src.Contains('?')) {
                var splitSrc = src.Split('?')[1].Split('&');
                if (splitSrc.FirstOrMaybe(x => x.StartsWith("width", StringComparison.OrdinalIgnoreCase)).TryGetValue(out var widthParam)
                    && widthParam.Contains('=')
                    && int.TryParse(widthParam.Split('=')[1], out var width)) {
                    widthOverride = width;
                }
                if (splitSrc.FirstOrMaybe(x => x.StartsWith("height", StringComparison.OrdinalIgnoreCase)).TryGetValue(out var heightParam)
                    && heightParam.Contains('=')
                    && int.TryParse(heightParam.Split('=')[1], out var height)) {
                    heightOverride = height;
                }
            }

            var mediaItemFound = Maybe<MediaItem>.None;
            if (mediaItemFound.TryGetValue(out var mediaItemFromFound)
                && mediaItemFromFound.MetaData.TryGetValue(out var metaDataFromFound)
                && metaDataFromFound is MediaMetadataImage imageMetadata) {
                isGetMedia = true;
                metaDataFound = imageMetadata with {
                    Width = widthOverride.GetValueOrDefault(imageMetadata.Width),
                    Height = heightOverride.GetValueOrDefault(imageMetadata.Height)
                };

                // Handle image profile main override
                if (imageProfiles.FirstOrMaybe(x => x.MaxScreenWidth.HasNoValue).TryGetValue(out var defaultImageProfile)
                    && imageMetadata.Height > 0
                    && imageMetadata.Width > 0) {
                    var ratio = Convert.ToDecimal(imageMetadata.Width) / Convert.ToDecimal(imageMetadata.Height);
                    metaDataFound = imageMetadata with {
                        Width = defaultImageProfile.ImageRenderWidth,
                        Height = Convert.ToInt32(Math.Round(defaultImageProfile.ImageRenderWidth / ratio))
                    };

                    // Update source
                    var height = CalculateHeight(metaDataFound.Value.Width, ratio);
                    output.Attributes.SetAttribute(sourceAttributeName, ReplaceWidthHeightOnSource(src, metaDataFound.Value.Width, height));

                    imageProfiles = imageProfiles.Except([defaultImageProfile]);
                }
            } else if (widthOverride.HasValue && heightOverride.HasValue) {
                metaDataFound = new MediaMetadataImage(width: widthOverride.Value, height: heightOverride.Value);
            }


            if (metaDataFound.TryGetValue(out var metaData)) {
                // Don't need to add alt on srcsets, but still need width and height
                if (!sourceAttributeName.Equals("srcset", StringComparison.OrdinalIgnoreCase)) {
                    output.Attributes.AddorReplaceEmptyAttribute("alt", mediaItem.MediaDescription.GetValueOrDefault(mediaItem.MediaTitle));
                }
                output.Attributes.AddorReplaceEmptyAttribute("width", metaData.Width.ToString());
                output.Attributes.AddorReplaceEmptyAttribute("height", metaData.Height.ToString());

                // Handle image profiles
                if (isGetMedia && imageProfiles.Where(x => x.MaxScreenWidth.HasValue).Any() && metaData.Width > 0 && metaData.Height > 0) {
                    decimal ratio = Convert.ToDecimal(metaData.Width) / Convert.ToDecimal(metaData.Height);
                    output.PreElement.AppendHtml("<picture>");

                    foreach (var imageProfile in imageProfiles.Where(x => x.MaxScreenWidth.HasValue).OrderBy(x => x.MaxScreenWidth.Value)) {
                        var height = CalculateHeight(imageProfile.ImageRenderWidth, ratio);
                        output.PreElement.AppendHtml($"<source media=\"(max-width:{imageProfile.MaxScreenWidth.Value}px)\" srcset=\"{ReplaceWidthHeightOnSource(src, imageProfile.ImageRenderWidth, height)}\" width=\"{imageProfile.ImageRenderWidth}\" height=\"{height}\" />");
                    }
                    output.PostElement.AppendHtml("</picture>");
                }
            }
        }

        public int CalculateHeight(int width, decimal widthToHeightRatio)
        {
            return Convert.ToInt32(Math.Round(Convert.ToDecimal(width) / widthToHeightRatio));
        }

        public string ReplaceWidthHeightOnSource(string url, int width, int height)
        {
            // Add 1.5 scale factor
            width = Convert.ToInt32(Math.Round(Convert.ToDecimal(width) * 1.5m));
            height = Convert.ToInt32(Math.Round(Convert.ToDecimal(height) * 1.5m));

            if (!url.Contains('?')) {
                return $"{url}?width={width}&height={height}";
            }

            var queryParams = url.Split('?')[1].Split('&').ToList();

            // Remove width and height
            queryParams.RemoveAll(x => x.StartsWith("width", StringComparison.OrdinalIgnoreCase) || x.StartsWith("height", StringComparison.OrdinalIgnoreCase));

            // Add width and height
            queryParams.Add($"width={width}");
            queryParams.Add($"height={height}");

            return $"{url.Split('?')[0]}?{string.Join("&", queryParams)}";
        }
    }
}
