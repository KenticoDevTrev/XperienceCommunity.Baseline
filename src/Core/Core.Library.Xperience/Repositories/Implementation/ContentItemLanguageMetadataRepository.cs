using CMS.ContentEngine.Internal;

namespace Core.Repositories.Implementation
{
    public class ContentItemLanguageMetadataRepository(ICacheRepositoryContext cacheRepositoryContext,
        IProgressiveCache progressiveCache,
        IInfoProvider<ContentItemLanguageMetadataInfo> contentItemLanguageMetadataInfoProvider,
        ILanguageIdentifierRepository languageIdentifierRepository) : IContentItemLanguageMetadataRepository
    {
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IInfoProvider<ContentItemLanguageMetadataInfo> _contentItemLanguageMetadataInfoProvider = contentItemLanguageMetadataInfoProvider;
        private readonly ILanguageIdentifierRepository _languageIdentifierRepository = languageIdentifierRepository;

        public Task<Result<ContentItemLanguageMetadataSummary>> GetOptimizedContentItemLanguageMetadata(IContentQueryDataContainer contentItem, bool? webPagesOnly = null, bool? published = null) => GetOptimizedContentItemLanguageMetadataInternal(contentItem.ContentItemID, contentItem.ContentItemCommonDataContentLanguageID, webPagesOnly ?? false, published.GetValueOrDefault(!_cacheRepositoryContext.PreviewEnabled()));

        public Task<Result<ContentItemLanguageMetadataSummary>> GetOptimizedContentItemLanguageMetadata(IContentItemFieldsSource contentItem, bool? webPagesOnly = null, bool? published = null) => GetOptimizedContentItemLanguageMetadataInternal(contentItem.SystemFields.ContentItemID, contentItem.SystemFields.ContentItemCommonDataContentLanguageID, webPagesOnly ?? false, published.GetValueOrDefault(!_cacheRepositoryContext.PreviewEnabled()));
        
        public Task<Result<ContentItemLanguageMetadataSummary>> GetOptimizedContentItemLanguageMetadata(int contentItemID, string languageCode, bool? webPagesOnly = null, bool? published = null) => GetOptimizedContentItemLanguageMetadataInternal(contentItemID, _languageIdentifierRepository.LanguageNameToId(languageCode), webPagesOnly ?? false, published.GetValueOrDefault(!_cacheRepositoryContext.PreviewEnabled()));
        
        public Task<Result<ContentItemLanguageMetadataSummary>> GetOptimizedContentItemLanguageMetadata(int contentItemID, int languageId, bool? webPagesOnly = null, bool? published = null) => GetOptimizedContentItemLanguageMetadataInternal(contentItemID, languageId, webPagesOnly ?? false, published.GetValueOrDefault(!_cacheRepositoryContext.PreviewEnabled()));

        private async Task<Result<ContentItemLanguageMetadataSummary>> GetOptimizedContentItemLanguageMetadataInternal(int contentItemID, int languageId, bool webPagesOnly, bool published)
        {
            var dictionary = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{ContentItemLanguageMetadataInfo.OBJECT_TYPE}|all");
                }

                var items = await _contentItemLanguageMetadataInfoProvider.Get()
                    .Columns([
                        nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataID),
                        nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataGUID),
                        nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataDisplayName),
                        nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataModifiedWhen),
                        nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID),
                        nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID)
                        ])
                    
                    .If(webPagesOnly, subQuery => subQuery.Where($"{nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID)} in (select WebPageItemContentItemID from CMS_WebPageItem)"))
                    .If(published, 
                            publishedQuery => publishedQuery.WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus), VersionStatus.Published),
                            currentVersionQuery => currentVersionQuery.WhereIn(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus),(int[]) [(int)VersionStatus.InitialDraft, (int)VersionStatus.Draft]).OrderBy(OrderDirection.Descending, nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataLatestVersionStatus))
                        )
                    .WhereEquals(nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID), languageId)
                    .GetEnumerableTypedResultAsync();
                return items.GroupBy(key => $"{key.ContentItemLanguageMetadataContentItemID}|{key.ContentItemLanguageMetadataContentLanguageID}")
                .ToDictionary(key => key.Key, value => value.Select(x => new ContentItemLanguageMetadataSummary(x.ContentItemLanguageMetadataID, x.ContentItemLanguageMetadataGUID, x.ContentItemLanguageMetadataDisplayName, x.ContentItemLanguageMetadataModifiedWhen)).First());
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "ContentItemMetadataSummaryDictionary", languageId, webPagesOnly, published));

            return dictionary.TryGetValue($"{contentItemID}|{languageId}", out var metadataSummary) ? metadataSummary : Result.Failure<ContentItemLanguageMetadataSummary>("Could not locate");
        }
    }
}
