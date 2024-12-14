using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System.Data;

namespace Core.Repositories.Implementation
{
    public class MetaDataRepository(
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory,
        IUrlResolver _urlResolver,
        IPageContextRepository pageContextRepository,
        IWebPageToPageMetadataConverter webPageToPageMetadataConverter,
        IIdentityService identityService,
        IContentQueryExecutor contentQueryExecutor,
        ICacheRepositoryContext cacheRepositoryContext,
        IProgressiveCache progressiveCache,
        MetadataOptions metadataOptions,
        ILanguageRepository languageRepository) : IMetaDataRepository
    {
        private readonly IPageContextRepository _pageContextRepository = pageContextRepository;
        private readonly IWebPageToPageMetadataConverter _webPageToPageMetadataConverter = webPageToPageMetadataConverter;
        private readonly IIdentityService _identityService = identityService;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly MetadataOptions _metadataOptions = metadataOptions;
        private readonly ILanguageRepository _languageRepository = languageRepository;

        public async Task<Result<PageMetaData>> GetMetaDataAsync(TreeCultureIdentity treeCultureIdentity, string? thumbnail = null)
        {
            if(!(await treeCultureIdentity.GetOrRetrievePageID(_identityService)).TryGetValue(out var pageId)) {
                return Result.Failure<PageMetaData>("No page found by that PageID");
            }
            
            var builder = _cacheDependencyBuilderFactory.Create()
               .WebPageOnLanguage(pageId, treeCultureIdentity.Culture);

            var results = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var queryBuilder = new ContentItemQueryBuilder()
                // Get web page items from this content culture ID
                .ForContentTypes(subQuery => subQuery.ForWebsite([pageId], includeUrlPath: true).WithLinkedItems(_metadataOptions.MaxLinkedLevelsRetrievedForMetadata))
                .InLanguage(treeCultureIdentity.Culture, useLanguageFallbacks: true);

                return await _contentQueryExecutor.GetWebPageResult(queryBuilder, (dataContainer) => {
                    return dataContainer;
                }, new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext));
            }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetMetaDataAsync", treeCultureIdentity.GetCacheKey(), _metadataOptions.MaxLinkedLevelsRetrievedForMetadata));

            if(results.FirstOrMaybe().TryGetValue(out var webPageResult)) {
                return await GetMetaDataInternalAsync(webPageResult, thumbnail);
            }
            return Result.Failure<PageMetaData>("No webpage found by that tree culture identity");
        }

        public async Task<Result<PageMetaData>> GetMetaDataAsync(int contentCultureId, string? thumbnail = null)
        {
            if ((await GetTranslationDictionary()).ContentCultureIdToIdentity.TryGetValue(contentCultureId, out var identity)) {
                return await GetMetaDataAsync(identity, thumbnail);
            }
            return Result.Failure<PageMetaData>("Could not find page by that content culture ID (make sure it's the ContentItemLanguageMetadataID)");
        }

        public async Task<Result<PageMetaData>> GetMetaDataAsync(Guid contentCultureGuid, string? thumbnail = null)
        {
            if((await GetTranslationDictionary()).ContentCultureGuidToIdentity.TryGetValue(contentCultureGuid, out var identity)) {
                return await GetMetaDataAsync(identity, thumbnail);
            }
            return Result.Failure<PageMetaData>("Could not find page by that content culture GUID (make sure it's the ContentItemLanguageMetadataGUID)");
        }

        private async Task<ContentCultureLookup> GetTranslationDictionary()
        {
            return await _progressiveCache.LoadAsync(async cs => {

                if(cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency("webpageitem|all");
                }

                var query = @"select ContentItemLanguageMetadataID, ContentItemLanguageMetadataGUID, WebPageItemID, ContentItemLanguageMetadataContentLanguageID from CMS_ContentItemLanguageMetadata
inner join CMS_ContentItem on ContentItemID = ContentItemLanguageMetadataContentItemID
inner join CMS_WebPageItem on WebPageItemContentItemID = ContentItemID";
                var results = await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery);

                var idToTree = new Dictionary<int, TreeCultureIdentity>();
                var guidToTree = new Dictionary<Guid, TreeCultureIdentity>();
                foreach (var row in results.Tables[0].Rows.Cast<DataRow>()) {
                    var identity = new TreeCultureIdentity(_languageRepository.LanguageIdToName((int)row["ContentItemLanguageMetadataContentLanguageID"])) {
                        PageID = (int)row[nameof(WebPageFields.WebPageItemID)]
                    };
                    idToTree.TryAdd((int)row["ContentItemLanguageMetadataID"], identity);
                    guidToTree.TryAdd((Guid)row["ContentItemLanguageMetadataGUID"], identity);
                }
                return new ContentCultureLookup(idToTree, guidToTree);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "MetadataRepository_GetTranslationDictionary"));
        }

        public async Task<Result<PageMetaData>> GetMetaDataAsync(string? thumbnail = null)
        {
            if ((await _pageContextRepository.GetCurrentPageAsync()).TryGetValue(out var page))
            {       
                return await GetMetaDataAsync(page.TreeCultureIdentity, thumbnail);
            }
            else
            {
                return Result.Failure<PageMetaData>("No page in the current context");
            }
        }

        private async Task<PageMetaData> GetMetaDataInternalAsync(IWebPageContentQueryDataContainer node, string? thumbnail = null)
        {
            string? keywords = null;
            string? description = null;
            string? title = null;
            Maybe<bool> noIndex = Maybe.None;
            string? canonicalUrlValue = null;
            string? thumbnailLarge = null;

            var metaDataResults = await _webPageToPageMetadataConverter.MapAndGetPageMetadata(node);

            if(metaDataResults.TryGetValue(out var metadataFromPage)) {
                thumbnail ??= metadataFromPage.Thumbnail.AsNullableValue();
                thumbnailLarge ??= metadataFromPage.ThumbnailLarge.AsNullableValue();
                keywords ??= metadataFromPage.Keywords.AsNullableValue();
                description ??= metadataFromPage.Description.AsNullableValue();
                title ??= metadataFromPage.Title.GetValueOrDefault(node.GetValue<string>("ContentItemLanguageMetadataDisplayName")).AsNullOrWhitespaceMaybe().AsNullableValue();
                noIndex = metadataFromPage.NoIndex;
                canonicalUrlValue = metadataFromPage.CanonicalUrl.AsNullableValue();
            }

            // Handle canonical url
            if (string.IsNullOrWhiteSpace(canonicalUrlValue))
            {
                canonicalUrlValue = node.WebPageUrlPath;
            }

            var metaData = new PageMetaData()
            {
                Title = title.AsNullOrWhitespaceMaybe(),
                Keywords = keywords.AsNullOrWhitespaceMaybe(),
                Description = description.AsNullOrWhitespaceMaybe(),
                Thumbnail = thumbnail.AsNullOrWhitespaceMaybe().TryGetValue(out var thumbUrl) ? _urlResolver.GetAbsoluteUrl(thumbUrl) : Maybe.None,
                ThumbnailLarge = thumbnailLarge.AsNullOrWhitespaceMaybe().TryGetValue(out var thumbLargeUrl) ? _urlResolver.GetAbsoluteUrl(thumbLargeUrl) : Maybe.None,
                NoIndex = noIndex,
                CanonicalUrl = canonicalUrlValue.AsNullOrWhitespaceMaybe().TryGetValue(out var url) ? _urlResolver.ResolveUrl(url) : Maybe.None
            };

            return metaData;
        }

        private record ContentCultureLookup(Dictionary<int, TreeCultureIdentity> ContentCultureIdToIdentity, Dictionary<Guid, TreeCultureIdentity> ContentCultureGuidToIdentity);
       
    }
}
