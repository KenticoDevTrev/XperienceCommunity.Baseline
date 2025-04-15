using CMS.Websites;
namespace Core.Repositories.Implementation
{
    public class DefaultContentItemToPageMetadataConverter : IContentItemToPageMetadataConverter,

#pragma warning disable CS0618 // Type or member is obsolete, will remove later
        IWebPageToPageMetadataConverter
#pragma warning restore CS0618 // Type or member is obsolete, will remove later
    {
        public Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageContentQueryDataContainer webPageContentQueryDataContainer, PageMetaData baseMetaData)
        {
            return Task.FromResult(Result.Failure<PageMetaData>("If you want to customize, must implement and inject your own. Use IContentQueryResultMapper.Map to cast the contentQueryDataContainer to your type"));
        }

        public Task<Result<PageMetaData>> MapAndGetPageMetadataReusableContent(IContentQueryDataContainer contentQueryDataContainer, PageMetaData baseMetaData, string? canonicalUrl)
        {
            return Task.FromResult(Result.Failure<PageMetaData>("If you want to customize, must implement and inject your own. Use IContentQueryResultMapper.Map to cast the contentQueryDataContainer to your type"));
        }
    }
}
