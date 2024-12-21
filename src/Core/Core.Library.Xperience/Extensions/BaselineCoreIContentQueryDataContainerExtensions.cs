namespace CMS.ContentEngine
{
    public static class BaselineCoreIContentQueryDataContainerExtensions
    {
        public static ContentIdentity ToContentIdentity(this IContentQueryDataContainer webpageData) => new() {
            ContentID = webpageData.ContentItemID,
            ContentGuid = webpageData.ContentItemGUID,
            ContentName = webpageData.ContentItemName
        };
    }
}
