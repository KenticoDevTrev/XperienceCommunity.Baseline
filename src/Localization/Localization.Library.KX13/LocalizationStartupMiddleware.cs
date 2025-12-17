using Localization.KX13.Repositories.Implementations;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.Localizer;

namespace Localization
{
    public static class LocalizationStartupMiddleware
    {
        public static IServiceCollection UseLocalization(this IServiceCollection services, LocalizationConfiguration? localizationConfiguration)
        {
            var configuration = localizationConfiguration ?? new LocalizationConfiguration("en-US");
            services
                .AddScoped((serviceProvider) => configuration)
                .AddScoped<ILocalizedCategoryCachedRepository, LocalizedCategoryCachedRepository>();

            return services;
        }
    }
}
