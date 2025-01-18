using CMS.ContentEngine;
using CSharpFunctionalExtensions;
using Navigation.Models;

namespace Navigation.Services.Implementations
{
    public class DefaultSiteMapCustomizationService : ISiteMapCustomizationService
    {
        public Task<Result<IEnumerable<SitemapNode>>> CustomizeCasting(string contentTypeCodename, IEnumerable<IContentQueryDataContainer> items)
        {
            // Implement your own logic if you want to cast content types a certain way, most cases the default behavior should suffice
            return Task.FromResult(Result.Failure<IEnumerable<SitemapNode>>("Not implemented, should customize if you want to customzie the casting"));
        }

        public Task<IEnumerable<SitemapNode>> GetAdditionalSitemapNodes()
        {
            // Implement your own logic to add custom site map items to the normal `ISiteMapRepository.GetSiteMapUrlSetAsync
            return Task.FromResult<IEnumerable<SitemapNode>>([]);
        }
    }
}
