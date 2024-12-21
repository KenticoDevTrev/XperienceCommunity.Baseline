using Microsoft.Extensions.DependencyInjection.Extensions;
using Search.Repositories;
using Search.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionBaselineSearchExtensions
    {
        /// <summary>
        /// Adds the Baseline Search.  Note that for the ISearchRepository, it only implements a dummy version that will throw a not implemented exception.  You must choose a provider, then implement and inject your own version of the ISearchRepository
        /// 
        /// https://github.com/Kentico/xperience-by-kentico-lucene (Available through XperienceCommunity.Baseline.Search.Library.Xperience.Lucene and XperienceCommunity.Baseline.Search.Admin.Xperience.Lucene)
        /// https://github.com/Kentico/xperience-by-kentico-algolia
        /// https://github.com/Kentico/xperience-by-kentico-azure-ai-search
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBaselineSearch(this IServiceCollection services)
        {
            // Since this implementation is a dummy one and will throw a not implemented, only try add, if it exists then don't overwrite.
            services.TryAddScoped<ISearchRepository, SearchRepository>();
            return services;
        }
    }
}
