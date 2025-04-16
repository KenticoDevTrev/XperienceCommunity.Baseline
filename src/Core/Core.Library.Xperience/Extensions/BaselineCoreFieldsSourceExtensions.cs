namespace CMS.Websites
{
    public static class BaselineCoreFieldsSourceExtensions
    {
        public static TreeIdentity ToTreeIdentity(this IWebPageFieldsSource webpageData) => new() {
            PageID = webpageData.SystemFields.WebPageItemID,
            PageGuid = webpageData.SystemFields.WebPageItemGUID,
            PageName = webpageData.SystemFields.WebPageItemName,
            PathChannelLookup = new PathChannel(webpageData.SystemFields.WebPageItemTreePath, webpageData.SystemFields.WebPageItemWebsiteChannelId)
        };

        public static TreeCultureIdentity ToTreeCultureIdentity(this IWebPageFieldsSource webpageData, ILanguageIdentifierRepository languageIdentifierRepository) => new(languageIdentifierRepository.LanguageIdToName(webpageData.SystemFields.ContentItemCommonDataContentLanguageID)) {
            PageID = webpageData.SystemFields.WebPageItemID,
            PageGuid = webpageData.SystemFields.WebPageItemGUID,
            PageName = webpageData.SystemFields.WebPageItemName,
            PathChannelLookup = new PathChannel(webpageData.SystemFields.WebPageItemTreePath, webpageData.SystemFields.WebPageItemWebsiteChannelId)
        };

        public static ContentIdentity ToContentIdentity(this IContentItemFieldsSource contentData) => new() {
            ContentID = contentData.SystemFields.ContentItemID,
            ContentGuid = contentData.SystemFields.ContentItemGUID,
            ContentName = contentData.SystemFields.ContentItemName
        };

        /// <summary>
        /// This will only fill the ContentCultureLookup, as the other fields are unavailable.
        /// </summary>
        /// <param name="contentData"></param>
        /// <param name="languageIdentifierRepository"></param>
        /// <returns></returns>
        public static ContentCultureIdentity ToContentIdentity(this IContentItemFieldsSource contentData, ILanguageIdentifierRepository languageIdentifierRepository) => new() {
            ContentCultureLookup = new ContentCulture(contentData.SystemFields.ContentItemID, languageIdentifierRepository.LanguageIdToName(contentData.SystemFields.ContentItemCommonDataContentLanguageID))
        };
    }
}
