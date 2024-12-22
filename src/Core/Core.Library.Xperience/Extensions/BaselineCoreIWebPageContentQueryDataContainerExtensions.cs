namespace CMS.Websites
{
    public static class BaselineCoreIWebPageContentQueryDataContainerExtensions
    {
        public static TreeIdentity ToTreeIdentity(this IWebPageContentQueryDataContainer webpageData) => new () {
            PageID = webpageData.WebPageItemID,
            PageGuid = webpageData.WebPageItemGUID,
            PageName = webpageData.WebPageItemName,
            PathChannelLookup = new PathChannel(webpageData.WebPageItemTreePath, webpageData.WebPageItemWebsiteChannelID)
        };

        public static TreeCultureIdentity ToTreeCultureIdentity(this IWebPageContentQueryDataContainer webpageData, ILanguageIdentifierRepository languageIdentifierRepository) => new(languageIdentifierRepository.LanguageIdToName(webpageData.ContentItemCommonDataContentLanguageID)) {
            PageID = webpageData.WebPageItemID,
            PageGuid = webpageData.WebPageItemGUID,
            PageName = webpageData.WebPageItemName,
            PathChannelLookup = new PathChannel(webpageData.WebPageItemTreePath, webpageData.WebPageItemWebsiteChannelID)
        };

        public static ContentIdentity ToContentIdentity(this IWebPageContentQueryDataContainer webpageData) => new() {
            ContentID = webpageData.ContentItemID,
            ContentGuid = webpageData.ContentItemGUID,
            ContentName = webpageData.ContentItemName
        };
    }
}
