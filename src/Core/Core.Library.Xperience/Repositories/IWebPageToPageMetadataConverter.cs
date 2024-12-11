using CMS.Websites;

namespace Core.Repositories
{
    public interface IWebPageToPageMetadataConverter
    {
        /// <summary>
        /// Sadly you'll need to implement your own of this as mapping and getting data is specific to each content type.  Use the IContentQueryResultMapper to map to your strongly typed class based on the ContentTypeName
        /// </summary>
        /// <param name="webPageContentQueryDataContainer"></param>
        /// <returns></returns>
        Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageContentQueryDataContainer webPageContentQueryDataContainer);
    }
}
