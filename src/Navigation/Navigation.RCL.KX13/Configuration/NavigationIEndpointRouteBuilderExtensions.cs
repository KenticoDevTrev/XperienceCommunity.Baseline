using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

namespace Navigation.RCL.Configuration
{
    public static partial class NavigationIEndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder UseSitemapRoute(this IEndpointRouteBuilder endpoints, IEnumerable<string>? sitemapPatterns = null)
        {
            // Defaults
            var urlPatterns = sitemapPatterns ?? new string[] { "sitemap.xml", "googlesitemap.xml" };

            foreach (string pattern in urlPatterns)
            {
                endpoints.MapControllerRoute(
                    name: $"Sitemap_{AlphaOnlyRegex().Replace(pattern, "")}",
                    pattern: pattern,
                    defaults: new { controller = "Sitemap", action = "Index" }
                );
            }

            return endpoints;
        }

        [GeneratedRegex("[A-Za-z]")]
        private static partial Regex AlphaOnlyRegex();
    }
}
