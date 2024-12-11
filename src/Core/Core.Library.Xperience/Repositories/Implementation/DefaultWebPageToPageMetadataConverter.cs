using CMS.Websites;
namespace Core.Repositories.Implementation
{
    public class DefaultWebPageToPageMetadataConverter : IWebPageToPageMetadataConverter
    {
        public Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageContentQueryDataContainer webPageContentQueryDataContainer)
        {
            return Task.FromResult(Result.Failure<PageMetaData>("Must implement and inject your own. Use IContentQueryResultMapper.Map to cast the contentQueryDataContainer to your type, and "));
        }
    }
}
