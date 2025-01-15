using System.Text.RegularExpressions;
using System.Web;

namespace System
{
    public static partial class StringExtensions
    {
        /// <summary>
        /// Tries to get the Guid value from `/getmedia/GUID/etc` or `/getattachment/GUID` Urls
        /// </summary>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public static Result<Guid> ParseGuidFromMediaUrl(this string mediaUrl)
        {
            var splitMedia = mediaUrl.Trim('~').Split('?')[0].Split('#')[0].Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (splitMedia.Length >= 2 && Guid.TryParse(splitMedia[1], out var mediaGuid))
            {
                return mediaGuid;
            }
            return Result.Failure<Guid>("Could not find Guid value in url");
        }

        /// <summary>
        /// Tries to get the ContentItemGuid and FieldGuid value from the `/getcontentasset/ContentItemGUID/FieldGuid/etc` Urls
        /// </summary>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public static Result<AssetUrlGuids> ParseGuidFromAssetUrl(this string mediaUrl)
        {
            var splitMedia = mediaUrl.Trim('~').Split('?')[0].Split('#')[0].Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (splitMedia.Length >= 3 && Guid.TryParse(splitMedia[1], out var contentItemGuid) && Guid.TryParse(splitMedia[2], out var fieldGuid)) {
                return new AssetUrlGuids(contentItemGuid, fieldGuid);
            }
            return Result.Failure<AssetUrlGuids>("Could not find Guid values in url");
        }

        /// <summary>
        /// Returns true if the url has /getmedia or /getcontentasset
        /// </summary>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public static bool IsMediaOrContentItemUrl(this string mediaUrl)
        {
            var url = (mediaUrl ?? "").ToLower();
            return url.IndexOf("/getmedia") > -1 || url.IndexOf("/getcontentasset") > -1;
        }

        /// <summary>
        /// Returns true if the url has /getmedia
        /// </summary>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public static bool IsMediaUrl(this string mediaUrl)
        {
            return (mediaUrl ?? "").IndexOf("/getmedia", StringComparison.OrdinalIgnoreCase) > -1;
        }

        /// <summary>
        /// Returns true if the url has /getcontentasset
        /// </summary>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public static bool IsContentAssetUrl(this string mediaUrl)
        {
            return (mediaUrl ?? "").IndexOf("/getcontentasset", StringComparison.OrdinalIgnoreCase) > -1;
        }

        /// <summary>
        /// Returns true if the url has /getattachment
        /// </summary>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public static bool IsAttachmentUrl(this string mediaUrl)
        {
            return (mediaUrl ?? "").IndexOf("/getattachment", StringComparison.OrdinalIgnoreCase) > -1;
        }

        public static Maybe<string> GetContentAssetUrlLanguage(this string src)
        {
            if (!src.Contains('?')) {
                return Maybe.None;
            }
            var queryItems = src.Split('?')[0].Split('&', StringSplitOptions.RemoveEmptyEntries);
            var langItem = queryItems.Where(x => x.StartsWith("language", StringComparison.OrdinalIgnoreCase));
            return langItem.FirstOrMaybe().TryGetValue(out var langItemVal) && langItemVal.Contains('=') ? langItemVal.Split('=')[1].AsNullOrWhitespaceMaybe() : Maybe.None;
        }

        /// <summary>
        /// Removes HTML Tags from a string
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string RemoveHtmlTags(this string html) => HtmlTagRegex().Replace(HttpUtility.HtmlDecode(html), string.Empty);

        /// <summary>
        /// Returns a Maybe.None if the string is null or whitespace
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Maybe<string> AsNullOrWhitespaceMaybe(this string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return Maybe.From(value);
            }
            else
            {
                return Maybe<string>.None;
            }
        }

        /// <summary>
        /// Shortcut for `string.Split(delimiterArray, StringSplitOptions.RemoveEmptyEntries)`
        /// </summary>
        /// <param name="value"></param>
        /// <param name="delimiters"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitAndRemoveEntries(this string value, string delimiters = "|,;")
        {
            return value.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Shortcut, removes tilde if there.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RemoveTildeFromFirstSpot(this string value) =>
            value.Length > 0 && value[0] == '~'
            ? value[1..] : value;

        /// <summary>
        /// Limits the string to the max length, adding an elipses, useful for summaries.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <param name="elipses"></param>
        /// <param name="keepWholeWord"></param>
        /// <returns></returns>
        public static string MaxLength(this string value, int maxLength, string elipses = "...", bool keepWholeWord = false)
        {
            if (value.Length > maxLength)
            {
                if (keepWholeWord)
                {
                    var newWord = String.Empty;
                    foreach (var word in value.Split(' '))
                    {
                        if ((newWord + " " + word).Length <= maxLength)
                        {
                            newWord += " " + word;
                        }
                        else
                        {
                            return $"{newWord}{elipses}";
                        }
                    }
                    return $"{newWord}{elipses}";
                }
                else
                {
                    return string.Concat(value.AsSpan(0, maxLength), elipses);
                }
            }
            return value;
        }

        /// <summary>
        /// Converts the given text to an HTML css/id safe attribute (with 'id-' prefixed), useful for client side marking and linking.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToAttributeId(this string text)
        {
            var clean = Regex.Replace(text.Replace(" ", "-"), "[^a-zA-Z0-9-_]", "").ToLower();
            if (clean.Length > 0 && !char.IsLetter(clean[0]))
            {
                return "id-" + clean;
            }
            return clean;
        }

        [GeneratedRegex("<.+?>")]
        private static partial Regex HtmlTagRegex();
    }
}