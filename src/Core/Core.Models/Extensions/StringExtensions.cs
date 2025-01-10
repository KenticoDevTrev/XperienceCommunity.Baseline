using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        /// <summary>
        /// Tries to get the Guid value from `/getMedia/GUID/etc` Urls
        /// </summary>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public static Result<Guid> ParseGuidFromMediaUrl(this string mediaUrl)
        {
            var splitMedia = mediaUrl.Trim('~').Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (splitMedia.Length >= 2 && Guid.TryParse(splitMedia[1], out var mediaGuid))
            {
                return mediaGuid;
            }
            return Result.Failure<Guid>("Could not find Guid value in url");
        }

        /// <summary>
        /// Tries to get the ContentItemGuid and FieldGuid value from the `/getContentAsset/ContentItemGUID/FieldGuid/etc` Urls
        /// </summary>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public static Result<AssetUrlGuids> ParseGuidFromAssetUrl(this string mediaUrl)
        {
            var splitMedia = mediaUrl.Trim('~').Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (splitMedia.Length >= 3 && Guid.TryParse(splitMedia[1], out var contentItemGuid) && Guid.TryParse(splitMedia[2], out var fieldGuid)) {
                return new AssetUrlGuids(contentItemGuid, fieldGuid);
            }
            return Result.Failure<AssetUrlGuids>("Could not find Guid values in url");
        }

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

        public static IEnumerable<string> SplitAndRemoveEntries(this string value, string delimiters = "|,;")
        {
            return value.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        public static string RemoveTildeFromFirstSpot(this string value) =>
            value.Length > 0 && value[0] == '~'
            ? value[1..] : value;

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

        public static string ToAttributeId(this string text)
        {
            var clean = Regex.Replace(text.Replace(" ", "-"), "[^a-zA-Z0-9-_]", "").ToLower();
            if (clean.Length > 0 && !char.IsLetter(clean[0]))
            {
                return "id-" + clean;
            }
            return clean;
        }
    }
}