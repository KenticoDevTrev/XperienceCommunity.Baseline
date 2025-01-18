using TabbedPages.Repositories;
using TabbedPages.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionTabbedPagesExtensions
    {
        public static IServiceCollection AddTabbedPages(this IServiceCollection services) =>
            services.AddScoped<ITabRepository, TabRepository>();
    }
}
