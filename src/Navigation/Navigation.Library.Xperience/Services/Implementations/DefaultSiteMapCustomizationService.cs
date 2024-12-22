using CMS.ContentEngine;
using CSharpFunctionalExtensions;
using Navigation.Models;

namespace Navigation.Services.Implementations
{
    public class DefaultSiteMapCustomizationService : ISiteMapCustomizationService
    {
        public Task<Result<IEnumerable<SitemapNode>>> CustomizeCasting(string contentTypeCodename, IEnumerable<IContentQueryDataContainer> items)
        {
            return Task.FromResult(Result.Failure<IEnumerable<SitemapNode>>("Not implemented, should customize if you want to customzie the casting"));
        }

        public Task<Result<ContentItemQueryBuilder>> CustomizeQueryBuilder(string contentTypeCodename)
        {
            if(contentTypeCodename.Equals(Generic.Navigation.CONTENT_TYPE_NAME)) {
                // TODO: Implement so it contains the fields needed for this.
            }
            return Task.FromResult(Result.Failure<ContentItemQueryBuilder>("Not implemented, should customize if you want to customize the casting"));
        }
    }
}
