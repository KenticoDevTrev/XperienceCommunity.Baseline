namespace Core.Repositories
{
    public interface IContentItemLanguageMetadataRepository
    {
        Task<Result<ContentItemLanguageMetadataSummary>> GetOptimizedContentItemLanguageMetadata(IContentQueryDataContainer contentItem, bool? webPagesOnly = null, bool? published = null);

        Task<Result<ContentItemLanguageMetadataSummary>> GetOptimizedContentItemLanguageMetadata(IContentItemFieldsSource contentItem, bool? webPagesOnly = null, bool? published = null);

        Task<Result<ContentItemLanguageMetadataSummary>> GetOptimizedContentItemLanguageMetadata(int contentItemID, string languageCode, bool? webPagesOnly = null, bool? published = null);

        Task<Result<ContentItemLanguageMetadataSummary>> GetOptimizedContentItemLanguageMetadata(int contentItemID, int languageId, bool? webPagesOnly = null, bool? published = null);
    }
}
