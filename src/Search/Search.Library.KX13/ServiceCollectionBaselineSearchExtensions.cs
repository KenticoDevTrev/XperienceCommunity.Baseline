using Search.Repositories;
using Search.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionBaselineSearchExtensions
    {
        /// <summary>
        /// Adds the Baseline Search (Lucene)
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBaselineSearch(this IServiceCollection services, Action<BaselineSearchOptions>? options = null)
        {
            var searchOptions = new BaselineSearchOptions();
            options?.Invoke(searchOptions);
            services.AddSingleton(searchOptions)
                    .AddScoped<ISearchRepository, SearchRepository>();
            return services;
        }
    }
}
