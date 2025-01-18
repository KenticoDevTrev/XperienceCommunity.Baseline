using Navigation.Models;
using Navigation.Services;

namespace MVC.Services.Implementation
{
    public class CustomSitemapCustomizationService : ISiteMapCustomizationService
    {
        public Task<Result<IEnumerable<SitemapNode>>> CustomizeCasting(string contentTypeCodename, IEnumerable<IContentQueryDataContainer> items)
        {
            // Can customize specific content types for casting
            return Task.FromResult<Result<IEnumerable<SitemapNode>>>(Result.Failure<IEnumerable<SitemapNode>>("Not implemented yet, so just ignore!"));
        }

        public Task<IEnumerable<SitemapNode>> GetAdditionalSitemapNodes()
        {
            return Task.FromResult<IEnumerable<SitemapNode>>([]);
        }
    }
}
