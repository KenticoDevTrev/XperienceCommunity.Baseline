using CMS.Websites;
namespace Core.Services
{
    public interface IMetaDataWebPageDataContainerConverter
    {
        Task<Result<PageMetaData>> GetDefaultMetadataLogic(IWebPageContentQueryDataContainer webPageContentQueryDataContainer);

        /// <summary>
        /// Used for Non Web Page Items (Content Items that will be manually presented as a web page through manual means)
        /// </summary>
        /// <param name="nonWebpageContentQueryDataContainer"></param>
        /// <param name="canonicalUrl"></param>
        /// <returns></returns>
        Task<Result<PageMetaData>> GetDefaultMetadataLogic(IContentQueryDataContainer nonWebpageContentQueryDataContainer, string? canonicalUrl);
        Task<Result<PageMetaData>> GetDefaultMetadataLogic(IWebPageFieldsSource webPageFieldSource);
        Task<Result<PageMetaData>> GetDefaultMetadataLogic(IContentItemFieldsSource nonWebpageContentItemFieldSource, string? canonicalUrl);
    }
}
