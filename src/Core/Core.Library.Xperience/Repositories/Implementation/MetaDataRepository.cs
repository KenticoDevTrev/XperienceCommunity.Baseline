using CMS.Websites;
using Generic;
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
        ILanguageRepository languageRepository,
        IContentTranslationInformationRepository contentTranslationInformationRepository,
        IContentItemLanguageMetadataRepository contentItemLanguageMetadataRepository,
        IMediaRepository mediaRepository,
        IContentItemReferenceService contentItemReferenceService) : IMetaDataRepository
    {
        private readonly IPageContextRepository _pageContextRepository = pageContextRepository;
        private readonly IWebPageToPageMetadataConverter _webPageToPageMetadataConverter = webPageToPageMetadataConverter;
        private readonly IIdentityService _identityService = identityService;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly MetadataOptions _metadataOptions = metadataOptions;
        private readonly ILanguageRepository _languageRepository = languageRepository;
        private readonly IContentTranslationInformationRepository _contentTranslationInformationRepository = contentTranslationInformationRepository;
        private readonly IContentItemLanguageMetadataRepository _contentItemLanguageMetadataRepository = contentItemLanguageMetadataRepository;
        private readonly IMediaRepository _mediaRepository = mediaRepository;
        private readonly IContentItemReferenceService _contentItemReferenceService = contentItemReferenceService;

        public async Task<Result<PageMetaData>> GetMetaDataAsync(TreeCultureIdentity treeCultureIdentity, string? thumbnail = null)
        {
            if (!(await treeCultureIdentity.GetOrRetrievePageID(_identityService)).TryGetValue(out var pageId)) {
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

            if (results.FirstOrMaybe().TryGetValue(out var webPageResult)) {
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
            if ((await GetTranslationDictionary()).ContentCultureGuidToIdentity.TryGetValue(contentCultureGuid, out var identity)) {
                return await GetMetaDataAsync(identity, thumbnail);
            }
            return Result.Failure<PageMetaData>("Could not find page by that content culture GUID (make sure it's the ContentItemLanguageMetadataGUID)");
        }

        private async Task<ContentCultureLookup> GetTranslationDictionary()
        {
            return await _progressiveCache.LoadAsync(async cs => {

                if (cs.Cached) {
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
            if ((await _pageContextRepository.GetCurrentPageAsync()).TryGetValue(out var page)) {
                return await GetMetaDataAsync(page.TreeCultureIdentity, thumbnail);
            } else {
                return Result.Failure<PageMetaData>("No page in the current context");
            }
        }

        private async Task<PageMetaData> GetMetaDataInternalAsync(IWebPageContentQueryDataContainer node, string? thumbnail = null)
        {
            Maybe<string> keywords = Maybe.None;
            Maybe<string> description = Maybe.None;
            Maybe<string> title = Maybe.None;
            Maybe<bool> noIndex = Maybe.None;
            Maybe<string> canonicalUrlValue = Maybe.None;
            Maybe<string> ogImage = thumbnail.AsNullOrWhitespaceMaybe();

            // Generate Base PageMetaData
            if ((await GetDefaultMetadataLogic(node)).TryGetValue(out var metaDataFromBase)) {
                ogImage = ogImage.GetValueOrDefault(metaDataFromBase.Thumbnail);
                keywords = metaDataFromBase.Keywords;
                description = metaDataFromBase.Description;
                title = metaDataFromBase.Title;
                noIndex = metaDataFromBase.NoIndex;
                canonicalUrlValue = metaDataFromBase.CanonicalUrl;
            }

            // Handle canonical url
            if (canonicalUrlValue.GetValueOrDefault(string.Empty).AsNullOrWhitespaceMaybe().HasNoValue) {
                var translations = await _contentTranslationInformationRepository.GetWebpageTranslationSummaries(node.WebPageItemID, node.WebPageItemWebsiteChannelID);
                if (translations.OrderByDescending(x => x.LanguageName.Equals(_languageRepository.LanguageIdToName(node.ContentItemCommonDataContentLanguageID), StringComparison.OrdinalIgnoreCase)).FirstOrMaybe().TryGetValue(out var properItem)) {
                    canonicalUrlValue = properItem.Url;
                } else {
                    canonicalUrlValue = $"/{node.WebPageUrlPath}";
                }
            }

            // Use fallback of display name
            if (title.GetValueOrDefault("").AsNullOrWhitespaceMaybe().HasNoValue) {
                if ((await _contentItemLanguageMetadataRepository.GetOptimizedContentItemLanguageMetadata(node, true, true)).TryGetValue(out var langMetadata)) {
                    title = langMetadata.ContentItemLanguageMetadataDisplayName;
                } else {
                    title = node.ContentItemName;
                }
            }

            var basePageMetadata = new PageMetaData() {
                Title = title,
                Keywords = keywords,
                Description = description,
                Thumbnail = ogImage.TryGetValue(out var thumbUrl) ? _urlResolver.GetAbsoluteUrl(thumbUrl) : Maybe.None,
                NoIndex = noIndex,
                CanonicalUrl = canonicalUrlValue.TryGetValue(out var url) ? _urlResolver.GetAbsoluteUrl(url) : Maybe.None
            };

            // Allow customizations
            if ((await _webPageToPageMetadataConverter.MapAndGetPageMetadata(node, basePageMetadata)).TryGetValue(out var metaData)) {
                return metaData;
            };
            return basePageMetadata;
        }

        private async Task<Result<PageMetaData>> GetDefaultMetadataLogic(IWebPageContentQueryDataContainer node)
        {
            string? keywords = null;
            string? description = null;
            string? title = null;
            bool? noIndex = null;
            string? ogImage = null;
            var dataFound = false;

            try {
                if (node.GetValue<string>(nameof(IBaseMetadata.MetaData_Title)).AsNullOrWhitespaceMaybe().TryGetValue(out var metaDataTitle)) {
                    title = metaDataTitle;
                    dataFound = true;
                }
                if (node.GetValue<string>(nameof(IBaseMetadata.MetaData_Description)).AsNullOrWhitespaceMaybe().TryGetValue(out var metaDataDescription)) {
                    description = metaDataDescription;
                    dataFound = true;
                }
                if (node.GetValue<string>(nameof(IBaseMetadata.MetaData_Keywords)).AsNullOrWhitespaceMaybe().TryGetValue(out var metaDataKeywords)) {
                    keywords = metaDataKeywords;
                    dataFound = true;
                }
                if (_contentItemReferenceService.GetContentItemReferences(node, nameof(IBaseMetadata.MetaData_OGImage)).FirstOrMaybe().TryGetValue(out var metaDataSmall)) {
                    var medias = await _mediaRepository.GetContentItemAssets(metaDataSmall.Identifier.ToContentIdentity());
                    if (medias.FirstOrMaybe().TryGetValue(out var media)) {
                        ogImage = media.MediaPermanentUrl;
                    }
                }
                if (node.TryGetValue(nameof(IBaseMetadata.MetaData_NoIndex), out bool? metaDataNoIndex) && metaDataNoIndex.HasValue) {
                    noIndex = metaDataNoIndex.Value;
                    dataFound = true;
                }
            } catch (Exception) {

            }
            return Result.SuccessIf(dataFound, new PageMetaData() {
                Title = title.AsMaybe(),
                Description = description.AsMaybe(),
                Keywords = keywords.AsMaybe(),
                NoIndex = noIndex.AsMaybe(),
                Thumbnail = ogImage.AsNullOrWhitespaceMaybe(),
            }, "Does not have any MetaData values");

        }

        private record ContentCultureLookup(Dictionary<int, TreeCultureIdentity> ContentCultureIdToIdentity, Dictionary<Guid, TreeCultureIdentity> ContentCultureGuidToIdentity);

    }
}
