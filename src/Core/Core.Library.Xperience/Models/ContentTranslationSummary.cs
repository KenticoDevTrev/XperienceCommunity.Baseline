namespace Core.Models
{
    public record ContentTranslationSummary(int ContentItemID, Maybe<int> ContentItemLanguageMetadataID, Maybe<int> ContentItemCommonDataID, string LanguageName, bool TranslationExists, bool IsDefaultLanguage);
}
