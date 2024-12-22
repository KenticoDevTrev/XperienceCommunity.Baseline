using CMS.Websites;

namespace Core.Repositories
{
    public interface IWebPageToPageMetadataConverter
    {
        /// <summary>
        /// This allows you to do custom mapping and adjustments from the base metadata.  This is useful if you want to add custom URL routing, titles, thumbnails, etc based on the type.
        /// </summary>
        /// <param name="webPageContentQueryDataContainer">The raw item</param>
        /// <param name="baseMetaData">This is the base metadata (what will be used if you do not modify and return a result). Leverages fields from the IBaseMetaData if available.</param>
        /// <returns></returns>
        Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageContentQueryDataContainer webPageContentQueryDataContainer, PageMetaData baseMetaData);
    }
}
