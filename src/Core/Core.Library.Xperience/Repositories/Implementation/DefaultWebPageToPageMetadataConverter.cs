using CMS.Websites;
namespace Core.Repositories.Implementation
{
    public class DefaultWebPageToPageMetadataConverter : IWebPageToPageMetadataConverter
    {
        public Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageContentQueryDataContainer webPageContentQueryDataContainer, PageMetaData baseMetaData)
        {
            return Task.FromResult(Result.Failure<PageMetaData>("If you want to customize, must implement and inject your own. Use IContentQueryResultMapper.Map to cast the contentQueryDataContainer to your type"));
        }
    }
}
