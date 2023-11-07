public static class StringExtensions
{

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
        ? value.Substring(1) : value;

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
}
