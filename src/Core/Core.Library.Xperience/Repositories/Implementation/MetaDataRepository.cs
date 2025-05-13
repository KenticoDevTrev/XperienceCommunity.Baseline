using CMS.Websites;
using Core.Models;
using Generic;
using Kentico.Content.Web.Mvc;
using System.Data;

namespace Core.Repositories.Implementation
{
    public class MetaDataRepository(
        IUrlResolver _urlResolver,
        IPageContextRepository pageContextRepository,
        IContentItemToPageMetadataConverter contentItemToPageMetadataConverter,
        IIdentityService identityService,
        IProgressiveCache progressiveCache,
        IContentTranslationInformationRepository contentTranslationInformationRepository,
        IContentItemLanguageMetadataRepository contentItemLanguageMetadataRepository,
        IMetaDataWebPageDataContainerConverter metaDataWebPageDataContainerConverter,
        ILanguageIdentifierRepository languageIdentifierRepository,
        IMappedContentItemRepository mappedContentItemRepository) : IMetaDataRepository
    {
        private readonly IPageContextRepository _pageContextRepository = pageContextRepository;
        private readonly IContentItemToPageMetadataConverter _contentItemToPageMetadataConverter = contentItemToPageMetadataConverter;
        private readonly IIdentityService _identityService = identityService;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IContentTranslationInformationRepository _contentTranslationInformationRepository = contentTranslationInformationRepository;
        private readonly IContentItemLanguageMetadataRepository _contentItemLanguageMetadataRepository = contentItemLanguageMetadataRepository;
        private readonly IMetaDataWebPageDataContainerConverter _metaDataWebPageDataContainerConverter = metaDataWebPageDataContainerConverter;
        private readonly ILanguageIdentifierRepository _languageIdentifierRepository = languageIdentifierRepository;
        private readonly IMappedContentItemRepository _mappedContentItemRepository = mappedContentItemRepository;

        public async Task<Result<PageMetaData>> GetMetaDataAsync(TreeCultureIdentity treeCultureIdentity, string? thumbnail = null)
        {
            // Get the core of the data, using PageContextRepository which will map all the fields properly using the IMappedContentRepository, including web page fields
            if (!(await _pageContextRepository.GetPageAsync<object>(treeCultureIdentity)).TryGetValue(out var pageContext)) { 
                return Result.Failure<PageMetaData>("No page found by that PageID");
            }
            
            if(pageContext.Data is IWebPageFieldsSource webPage) { 
                return await GetMetaDataInternalAsync(webPage, null, thumbnail);
            }

            return Result.Failure<PageMetaData>("Page was not a Web Page");
        }

        public async Task<Result<PageMetaData>> GetMetaDataForReusableContentAsync(ContentCultureIdentity contentCultureIdentity, string? canonicalUrl, string? thumbnail = null)
        {
            // Get the core of the data, using PageContextRepository which will map all the fields properly using the IMappedContentRepository, including web page fields
            if (
                !(await contentCultureIdentity.GetOrRetrieveContentCultureLookup(_identityService)).TryGetValue(out var lookup)
                || !(await _mappedContentItemRepository.GetContentItem(lookup.ContentId.ToContentIdentity(), lookup.Culture.TryGetValue(out var lang) ? lang : null)).TryGetValue(out var contentItemContext)) {
                return Result.Failure<PageMetaData>("No content item found by that Content Identity");
            }

            if (contentItemContext is IContentItemFieldsSource contentItem) {
                return await GetMetaDataInternalAsync(contentItem, canonicalUrl, thumbnail);
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
            if((await _pageContextRepository.GetCurrentPageAsync<object>()).TryGetValue(out var page)
                && page.Data is IWebPageFieldsSource webPage) {
                return await GetMetaDataInternalAsync(webPage, thumbnail);
            }
            if ((await _pageContextRepository.GetCurrentPageAsync()).TryGetValue(out var currentPage)) {
                return await GetMetaDataAsync(currentPage.TreeCultureIdentity, thumbnail);
            } else {
                return Result.Failure<PageMetaData>("No page in the current context");
            }
        }

        private async Task<PageMetaData> GetMetaDataInternalAsync(IContentItemFieldsSource node, string? canonicalUrl, string? thumbnail = null)
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

            // Handle canonical url
            if (canonicalUrlValue.GetValueOrDefault(string.Empty).AsNullOrWhitespaceMaybe().HasNoValue && node is IWebPageFieldsSource webPage) {
                var translations = await _contentTranslationInformationRepository.GetWebpageTranslationSummaries(webPage.SystemFields.WebPageItemID, webPage.SystemFields.WebPageItemWebsiteChannelId);
                if (translations.OrderByDescending(x => x.LanguageName.Equals(_languageIdentifierRepository.LanguageIdToName(webPage.SystemFields.ContentItemCommonDataContentLanguageID), StringComparison.OrdinalIgnoreCase)).TryGetFirst(out var properItem)) {
                    canonicalUrlValue = properItem.Url;
                } else {
                    canonicalUrlValue = $"/{webPage.SystemFields.WebPageUrlPath}";
                }
            }

            // Use fallback of display name
            if (title.GetValueOrDefault("").AsNullOrWhitespaceMaybe().HasNoValue) {
                if ((await _contentItemLanguageMetadataRepository.GetOptimizedContentItemLanguageMetadata(node, true, true)).TryGetValue(out var langMetadata)) {
                    title = langMetadata.ContentItemLanguageMetadataDisplayName;
                } else {
                    title = node.SystemFields.ContentItemName;
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
            if (node is IWebPageFieldsSource webPageFieldSource) {
                if ((await _contentItemToPageMetadataConverter.MapAndGetPageMetadata(webPageFieldSource, basePageMetadata)).TryGetValue(out var metaData)) {
                    return metaData;
                }
            } else {
                if ((await _contentItemToPageMetadataConverter.MapAndGetPageMetadataReusableContent(node, basePageMetadata, canonicalUrl)).TryGetValue(out var metaData)) {
                    return metaData;
                }
            }
                 
            return basePageMetadata;
        }
        private record ContentCultureLookup(Dictionary<int, TreeCultureIdentity> ContentCultureIdToIdentity, Dictionary<Guid, TreeCultureIdentity> ContentCultureGuidToIdentity);

    }
}
