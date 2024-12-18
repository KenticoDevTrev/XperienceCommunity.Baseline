using Microsoft.AspNetCore.Html;

namespace Microsoft.AspNetCore.Mvc.Localization
{
    public static class IHtmlLocalizerExtensions
    {
        /// <summary>
        /// Returns the GetString result, or the default value if not found or is empty
        /// </summary>
        /// <param name="stringLocalizer">The String Localizer</param>
        /// <param name="name">The Key Name</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>The value</returns>
        public static HtmlString GetHtmlStringOrDefault<T>(this IHtmlLocalizer<T> htmlLocalizer, string name, HtmlString defaultValue)
        {
            var value = htmlLocalizer.GetHtml(name);
            if(value == null || value.IsResourceNotFound || string.IsNullOrWhiteSpace(value.Value) || value.Value.Equals(name))
            {
                return defaultValue;
            }
            return new HtmlString(value.Value);
        }
    }
}
