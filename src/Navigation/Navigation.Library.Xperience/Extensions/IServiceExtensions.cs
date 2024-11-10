using Microsoft.Extensions.DependencyInjection;
using Navigation.Repositories;
using Navigation.Repositories.Implementations;

namespace Navigation.Extensions
{
    public static class INavigationServiceExtensions
    {
        public static IServiceCollection AddBaselineNavigation(this IServiceCollection services)
        {
            return services.AddScoped<INavigationRepository, NavigationRepository>()
                .AddScoped<IDynamicNavigationRepository, DynamicNavigationRepository>();
        }
    }
}
