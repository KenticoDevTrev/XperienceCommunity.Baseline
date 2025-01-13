using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Core.Services
{
    /// <summary>
    /// Service for Media Tag Helper for parsing.
    /// </summary>
    public interface IMediaTagHelperService
    {
        /// <summary>
        /// Calculates the Height given the ratio and the width given
        /// </summary>
        /// <param name="width"></param>
        /// <param name="widthToHeightRatio"></param>
        /// <returns></returns>
        int CalculateHeight(int width, decimal widthToHeightRatio);

        /// <summary>
        /// Gets the Dictionary of attributes from the style tag, empty dictionary if no style tag.
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        Dictionary<string, string?> GetStyleDictionary(TagHelperOutput output);

        /// <summary>
        /// If a style tag is found with the background-image and that image has a dynamic-able url (/getmedia, /getcontentasset),
        /// this will set up the javascript to alter the background-image to the Image Profiles given
        /// </summary>
        /// <param name="output">the Tag Helper Output (this will be modified)</param>
        /// <param name="mediaItem">The found Media Item (use IMediaRepository.GetMediaItemFromUrl)</param>
        /// <param name="fullUrl">The full Url (with height/width tags)</param>
        /// <param name="imageProfiles">Image Profiles you wish to use</param>
        void HandleBackgroundImageProfiles(TagHelperOutput output, MediaItem mediaItem, string fullUrl, IEnumerable<ImageProfile> imageProfiles);

        /// <summary>
        /// Handles Width, Height, Alt attributes on an image.  if image profiles provided, will also convert to a picture and image source set
        /// </summary>
        /// <param name="output">the Tag Helper Output (this will be modified)</param>
        /// <param name="mediaItem">The found Media Item (use IMediaRepository.GetMediaItemFromUrl)</param>
        /// <param name="sourceAttributeName">Where the url source attribute is, either src or srcset</param>
        /// <param name="imageProfiles">Image Profiles you wish to use</param>
        void HandleMediaItemProfilesAndMetaData(TagHelperOutput output, MediaItem mediaItem, string sourceAttributeName, IEnumerable<ImageProfile> imageProfiles);

        /// <summary>
        /// Replaces the width and height attributes on the given url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        string ReplaceWidthHeightOnSource(string url, int width, int height);

        /// <summary>
        /// Gets the value in the url('') of a background tag
        /// </summary>
        /// <param name="backgroundStyleValue"></param>
        /// <returns></returns>
        Result<string> GetImageUrlFromBackgroundStyle(string backgroundStyleValue);
    }
}
