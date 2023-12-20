using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

namespace Navigation.RCL.Configuration
{
    public static class NavigationIEndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder UseSitemapRoute(this IEndpointRouteBuilder endpoints, IEnumerable<string>? sitemapPatterns = null)
        {
            // Defaults
            var urlPatterns = sitemapPatterns ?? new string[] { "sitemap.xml", "googlesitemap.xml" };

            foreach (string pattern in urlPatterns)
            {
                endpoints.MapControllerRoute(
                    name: $"Sitemap_{Regex.Replace(pattern, "[A-Za-z]", "")}",
                    pattern: pattern,
                    defaults: new { controller = "Sitemap", action = "Index" }
                );
            }

            return endpoints;
        }
    }
}
