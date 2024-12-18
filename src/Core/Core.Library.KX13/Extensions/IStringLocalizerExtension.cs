namespace Microsoft.Extensions.Localization
{
    public static class IStringLocalizerExtensions
    {
        /// <summary>
        /// Returns the GetString result, or the default value if not found or is empty
        /// </summary>
        /// <param name="stringLocalizer">The String Localizer</param>
        /// <param name="name">The Key Name</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>The value</returns>
        public static string GetStringOrDefault(this IStringLocalizer<SharedResources> stringLocalizer, string name, string defaultValue)
        {
            var value = stringLocalizer.GetString(name);
            if (value == null || value.ResourceNotFound || string.IsNullOrWhiteSpace(value.Value) || value.Value.Equals(name)) {
                return defaultValue;
            }
            return value.Value;
        }
    }
}
