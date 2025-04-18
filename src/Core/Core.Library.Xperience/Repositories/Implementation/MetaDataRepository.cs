﻿using CMS.Websites;
using Generic;
using Kentico.Content.Web.Mvc;
using System.Data;

namespace Core.Repositories.Implementation
{
    public class MetaDataRepository(
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory,
        IUrlResolver _urlResolver,
        IPageContextRepository pageContextRepository,
        IContentItemToPageMetadataConverter contentItemToPageMetadataConverter,
        IIdentityService identityService,
        IContentQueryExecutor contentQueryExecutor,
        ICacheRepositoryContext cacheRepositoryContext,
        IProgressiveCache progressiveCache,
        MetadataOptions metadataOptions,
        IContentTranslationInformationRepository contentTranslationInformationRepository,
        IContentItemLanguageMetadataRepository contentItemLanguageMetadataRepository,
        IMetaDataWebPageDataContainerConverter metaDataWebPageDataContainerConverter,
        ILanguageIdentifierRepository languageIdentifierRepository) : IMetaDataRepository
    {
        private readonly IPageContextRepository _pageContextRepository = pageContextRepository;
        private readonly IContentItemToPageMetadataConverter _contentItemToPageMetadataConverter = contentItemToPageMetadataConverter;
        private readonly IIdentityService _identityService = identityService;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly MetadataOptions _metadataOptions = metadataOptions;
        private readonly IContentTranslationInformationRepository _contentTranslationInformationRepository = contentTranslationInformationRepository;
        private readonly IContentItemLanguageMetadataRepository _contentItemLanguageMetadataRepository = contentItemLanguageMetadataRepository;
        private readonly IMetaDataWebPageDataContainerConverter _metaDataWebPageDataContainerConverter = metaDataWebPageDataContainerConverter;
        private readonly ILanguageIdentifierRepository _languageIdentifierRepository = languageIdentifierRepository;

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

            if (results.TryGetFirst(out var webPageResult)) {
                return await GetMetaDataInternalAsync(webPageResult, thumbnail);
            }
            return Result.Failure<PageMetaData>("No webpage found by that tree culture identity");
        }

        public async Task<Result<PageMetaData>> GetMetaDataForReusableContentAsync(ContentCultureIdentity contentCultureIdentity, string? canonicalUrl, string? thumbnail = null)
        {
            if (!(await contentCultureIdentity.GetOrRetrieveContentCultureLookup(_identityService)).TryGetValue(out var contentLookup)) {
                return Result.Failure<PageMetaData>("No content item found by that Content Identity");
            }

            var builder = _cacheDependencyBuilderFactory.Create()
                .ContentItem(contentLookup.ContentId);

            var results = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var queryBuilder = new ContentItemQueryBuilder()

                // Get web page items from this content culture ID
                .ForContentTypes(subQuery => subQuery.OfReusableSchema(IBaseMetadata.REUSABLE_FIELD_SCHEMA_NAME).WithLinkedItems(_metadataOptions.MaxLinkedLevelsRetrievedForMetadata))
                .InLanguage(contentLookup.Culture.GetValueOrDefault("en"), useLanguageFallbacks: true);

                return await _contentQueryExecutor.GetResult(queryBuilder, (dataContainer) => {
                    return dataContainer;
                }, new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext));
            }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetMetaDataAsync", contentCultureIdentity.GetCacheKey(), _metadataOptions.MaxLinkedLevelsRetrievedForMetadata));

            if (results.TryGetFirst(out var contentItemResult)) {
                return await GetMetaDataContentItemInternalAsync(contentItemResult, canonicalUrl, thumbnail);
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
                    var identity = new TreeCultureIdentity(_languageIdentifierRepository.LanguageIdToName((int)row["ContentItemLanguageMetadataContentLanguageID"])) {
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
            if ((await _metaDataWebPageDataContainerConverter.GetDefaultMetadataLogic(node)).TryGetValue(out var metaDataFromBase)) {
                if (metaDataFromBase.Thumbnail.TryGetValue(out var foundThumbnail)) {
                    ogImage = foundThumbnail;
                }
                keywords = metaDataFromBase.Keywords;
                description = metaDataFromBase.Description;
                title = metaDataFromBase.Title;
                noIndex = metaDataFromBase.NoIndex;
                canonicalUrlValue = metaDataFromBase.CanonicalUrl;
            }

            // Handle canonical url
            if (canonicalUrlValue.GetValueOrDefault(string.Empty).AsNullOrWhitespaceMaybe().HasNoValue) {
                var translations = await _contentTranslationInformationRepository.GetWebpageTranslationSummaries(node.WebPageItemID, node.WebPageItemWebsiteChannelID);
                if (translations.OrderByDescending(x => x.LanguageName.Equals(_languageIdentifierRepository.LanguageIdToName(node.ContentItemCommonDataContentLanguageID), StringComparison.OrdinalIgnoreCase)).TryGetFirst(out var properItem)) {
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
            if ((await _contentItemToPageMetadataConverter.MapAndGetPageMetadata(node, basePageMetadata)).TryGetValue(out var metaData)) {
                return metaData;
            };
            return basePageMetadata;
        }

        private async Task<PageMetaData> GetMetaDataContentItemInternalAsync(IContentQueryDataContainer node, string? canonicalUrl, string? thumbnail = null)
        {
            Maybe<string> keywords = Maybe.None;
            Maybe<string> description = Maybe.None;
            Maybe<string> title = Maybe.None;
            Maybe<bool> noIndex = Maybe.None;
            Maybe<string> canonicalUrlValue = Maybe.None;
            Maybe<string> ogImage = thumbnail.AsNullOrWhitespaceMaybe();

            // Generate Base PageMetaData
            if ((await _metaDataWebPageDataContainerConverter.GetDefaultMetadataLogic(node, canonicalUrl)).TryGetValue(out var metaDataFromBase)) {
                if (metaDataFromBase.Thumbnail.TryGetValue(out var foundThumbnail)) {
                    ogImage = foundThumbnail;
                }
                keywords = metaDataFromBase.Keywords;
                description = metaDataFromBase.Description;
                title = metaDataFromBase.Title;
                noIndex = metaDataFromBase.NoIndex;
                canonicalUrlValue = metaDataFromBase.CanonicalUrl;
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
            if ((await _contentItemToPageMetadataConverter.MapAndGetPageMetadataReusableContent(node, basePageMetadata, canonicalUrl)).TryGetValue(out var metaData)) {
                return metaData;
            };
            return basePageMetadata;
        }


        private record ContentCultureLookup(Dictionary<int, TreeCultureIdentity> ContentCultureIdToIdentity, Dictionary<Guid, TreeCultureIdentity> ContentCultureGuidToIdentity);

    }
}
