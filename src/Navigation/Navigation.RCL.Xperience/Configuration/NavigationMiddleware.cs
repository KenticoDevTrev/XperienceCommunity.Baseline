using Microsoft.AspNetCore.Builder;
using System.Text.RegularExpressions;

namespace Navigation.Middleware
{
    public static partial class IServiceCollectionNavigationMiddleware
    {
        /// <summary>
        /// Adds the sitemap routes
        /// </summary>
        /// <param name="endpoints">Your IEndpointRouteBuilder</param>
        /// <param name="sitemapPatterns">Where the sitemap should resolve, default is /sitemap.xml and /googlesitemap.xml</param>
        /// <returns></returns>
        public static WebApplication UseSitemapRoute(this WebApplication app, IEnumerable<string>? sitemapPatterns = null)
        {
            // Defaults
            var urlPatterns = sitemapPatterns ?? ["sitemap.xml", "googlesitemap.xml"];

            foreach (string pattern in urlPatterns)
            {
                app.MapControllerRoute(
                    name: $"Sitemap_{AlphaOnlyRegex().Replace(pattern, "")}",
                    pattern: pattern,
                    defaults: new { controller = "Sitemap", action = "Index" }
                );
            }

            return app;
        }

        [GeneratedRegex("[A-Za-z]")]
        private static partial Regex AlphaOnlyRegex();
    }
}
