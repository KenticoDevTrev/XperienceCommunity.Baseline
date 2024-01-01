using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Navigation.Repositories.Implementations;
using System.Text.RegularExpressions;

namespace Navigation.Middleware
{
    public static partial class IServiceCollectionNavigationMiddleware
    {
        /// <summary>
        /// Adds Navigation Dependency Injection and options
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection UseNavigation(this IServiceCollection services) => services.UseNavigation((x) => { });
        public static IServiceCollection UseNavigation(this IServiceCollection services, Action<NavigationConfigurationOptions> options)
        {
            var optionValue = new NavigationConfigurationOptions();
            options.Invoke(optionValue);
            return services.AddScoped(typeof(IBreadcrumbRepository), optionValue.IBreadcrumbRepositoryImplementation)
                .AddScoped(typeof(INavigationRepository), optionValue.INavigationRepositoryImplementation);
        }

        /// <summary>
        /// Adds Dependency Injection for Sitemap Services, should also use UseSitemapRoute on your IEndpointRouteBuilder to leverage
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection UseSitemap(this IServiceCollection services) => services.UseSitemap((x) => { });
        public static IServiceCollection UseSitemap(this IServiceCollection services, Action<SitemapConfigurationOptions> options)
        {
            var optionValue = new SitemapConfigurationOptions();
            options.Invoke(optionValue);
            return services.AddScoped(typeof(ISiteMapRepository), optionValue.ISitemapRepositoryImplementation);
        }

        /// <summary>
        /// Adds the sitemap routes
        /// </summary>
        /// <param name="endpoints">Your IEndpointRouteBuilder</param>
        /// <param name="sitemapPatterns">Where the sitemap should resolve, default is /sitemap.xml and /googlesitemap.xml</param>
        /// <returns></returns>
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

    public class NavigationConfigurationOptions
    {
        public NavigationConfigurationOptions() { 

        }

        public Type IBreadcrumbRepositoryImplementation { get; set; } = typeof(BreadcrumbRepository);
        public Type INavigationRepositoryImplementation { get; set; } = typeof(NavigationRepository);
    } 

    public class SitemapConfigurationOptions
    {
        public SitemapConfigurationOptions() { }
        public Type ISitemapRepositoryImplementation { get; set; } = typeof(SiteMapRepository);
    }
}
