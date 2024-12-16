namespace Core.Services
{
    public interface IContentItemReferenceService
    {

        /// <summary>
        /// Gets the Content Item References, useful if using the Pages or Content Item Asset Selector and you want the ContentItemGuid Identifiers instead of the Content Type / Reusable Field Interface itself.
        /// </summary>
        /// <param name="fieldSource"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public IEnumerable<ContentItemReference> GetContentItemReferences(IContentQueryDataContainer itemData, string columnName);
    }
}
