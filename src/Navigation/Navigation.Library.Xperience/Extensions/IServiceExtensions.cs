using Account.Installers;
using Microsoft.Extensions.DependencyInjection;
using Navigation.Models;
using Navigation.Repositories;
using Navigation.Repositories.Implementations;
using Navigation.Services;
using Navigation.Services.Implementations;

namespace Navigation.Extensions
{
    public static class INavigationServiceExtensions
    {
        public static IServiceCollection AddBaselineNavigation(this IServiceCollection services, BaselineNavigationOptions options)
        {
            return services.AddScoped<IDynamicNavigationRepository, DynamicNavigationRepository>()
                .AddScoped<ISecondaryNavigationService, NavigationRepository>()
                .AddScoped<INavigationRepository, NavigationRepository>()
                .AddScoped<ISiteMapService, SiteMapRepository>()
                .AddScoped<ISiteMapRepository, SiteMapRepository>()
                .AddScoped<ISiteMapCustomizationService, DefaultSiteMapCustomizationService>() // Should be customized
                .AddScoped<IBreadcrumbRepository, BreadcrumbRepository>()
                .AddSingleton(options)
                .AddSingleton<BaselineNavigationModuleInstaller>();
        }
    }
}
