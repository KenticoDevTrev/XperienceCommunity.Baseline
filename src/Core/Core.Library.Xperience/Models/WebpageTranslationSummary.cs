namespace Core.Models
{
    public record WebpageTranslationSummary(int WebPageItemID, int ContentItemID, Maybe<int> ContentItemLanguageMetadataID, Maybe<int> ContentItemCommonDataID, string Url, string TreePath, string LanguageName, bool TranslationExists, bool IsDefaultLanguage, bool IsHome);
}
