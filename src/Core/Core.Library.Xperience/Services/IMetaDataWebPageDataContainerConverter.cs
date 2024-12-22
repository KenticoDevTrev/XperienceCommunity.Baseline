using CMS.Websites;
namespace Core.Services
{
    public interface IMetaDataWebPageDataContainerConverter
    {
        Task<Result<PageMetaData>> GetDefaultMetadataLogic(IWebPageContentQueryDataContainer webPageContentQueryDataContainer);
    }
}
