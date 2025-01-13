namespace Microsoft.AspNetCore.Razor.TagHelpers
{
    public static class TagHelperAttributeListExtensions
    {
        public static void AddorReplaceEmptyAttribute(this TagHelperAttributeList attributes, string attributeName, string attributeValue)
        {
            if (!attributes.TryGetAttribute(attributeName, out var value) || value.Value.ToString().AsNullOrWhitespaceMaybe().HasNoValue) {
                attributes.SetAttribute(attributeName, attributeValue);
            }
        }

        public static void AddorAppendAttribute(this TagHelperAttributeList attributes, string attributeName, string attributeValue)
        {
            // If it has an existing value, then combine that with the given value.
            string value = $"{(attributes.ContainsName(attributeName) ? attributes[attributeName].Value?.ToString() ?? string.Empty : string.Empty)} {attributeValue}".Trim();
            attributes.SetAttribute(attributeName, value);
        }
    }
}
