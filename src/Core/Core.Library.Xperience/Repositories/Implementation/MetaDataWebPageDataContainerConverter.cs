using CMS.ContentEngine.Internal;
using CMS.Websites;
using CMS.Websites.Routing;
using Generic;
using System.Text.Json;

namespace Core.Repositories.Implementation
{
    public class MetaDataWebPageDataContainerConverter(IContentItemReferenceService contentItemReferenceService,
        IClassContentTypeAssetConfigurationRepository classContentTypeAssetConfigurationRepository,
        IProgressiveCache progressiveCache,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
        ILanguageIdentifierRepository languageIdentifierRepository,
        IContentTranslationInformationRepository contentTranslationInformationRepository,
        IContentTypeRetriever contentTypeRetriever,
        IContentQueryExecutor contentQueryExecutor,
        IWebsiteChannelContext websiteChannelContext) : IMetaDataWebPageDataContainerConverter
    {
        private readonly IContentItemReferenceService _contentItemReferenceService = contentItemReferenceService;
        private readonly IClassContentTypeAssetConfigurationRepository _classContentTypeAssetConfigurationRepository = classContentTypeAssetConfigurationRepository;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider = contentLanguageInfoProvider;
        private readonly ILanguageIdentifierRepository _languageIdentifierRepository = languageIdentifierRepository;
        private readonly IContentTranslationInformationRepository _contentTranslationInformationRepository = contentTranslationInformationRepository;
        private readonly IContentTypeRetriever _contentTypeRetriever = contentTypeRetriever;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;

        public async Task<Result<PageMetaData>> GetDefaultMetadataLogic(IWebPageContentQueryDataContainer webPageContentQueryData)
        {
            string? keywords = null;
            string? description = null;
            string? title = null;
            bool? noIndex = null;
            string? ogImage = null;
            string? url = null;
            var dataFound = false;
            // TODO: just try to do a type cast if it's an actual right object, didn't realize these do map
            // ex: if(webPageContentQueryData is IBaseMetadata baseMetadata)
            try {
                if (webPageContentQueryData.GetValue<string>(nameof(IBaseMetadata.MetaData_Title)).AsNullOrWhitespaceMaybe().TryGetValue(out var metaDataTitle)) {
                    title = metaDataTitle;
                    dataFound = true;
                }
                if (webPageContentQueryData.GetValue<string>(nameof(IBaseMetadata.MetaData_Description)).AsNullOrWhitespaceMaybe().TryGetValue(out var metaDataDescription)) {
                    description = metaDataDescription;
                    dataFound = true;
                }
                if (webPageContentQueryData.GetValue<string>(nameof(IBaseMetadata.MetaData_Keywords)).AsNullOrWhitespaceMaybe().TryGetValue(out var metaDataKeywords)) {
                    keywords = metaDataKeywords;
                    dataFound = true;
                }
                if (_contentItemReferenceService.GetContentItemReferences(webPageContentQueryData, nameof(IBaseMetadata.MetaData_OGImage)).TryGetFirst(out var metaDataSmall)) {
                    // Get class name based on the ContentItemGuid
                    if ((await _contentTypeRetriever.GetContentType(metaDataSmall.Identifier.ToContentIdentity())).TryGetValue(out var contentType)
                        && (await _classContentTypeAssetConfigurationRepository.GetClassNameToContentTypeAssetConfigurationDictionary()).TryGetValue(contentType.ToLowerInvariant(), out var configurations)) {
                        // Grab the first Image media field found as the OG image
                        if (configurations.AssetFields
                            .Where(x => x.MediaType == ContentItemAssetMediaType.Image || x.MediaType == ContentItemAssetMediaType.Unknown)
                            .OrderByDescending(x => x.MediaType == ContentItemAssetMediaType.Image)
                            .FirstOrMaybe()
                            .TryGetValue(out var assetField)) {
                            var language = _languageIdentifierRepository.LanguageIdToName(webPageContentQueryData.ContentItemCommonDataContentLanguageID);
                            if ((await GetImageMetadataField(metaDataSmall.Identifier, contentType, assetField.AssetFieldName, language)).TryGetValue(out var metaData)) {
                                ogImage = $"/getcontentasset/{metaDataSmall.Identifier}/{assetField.FieldGuid}/{metaData.Name}{metaData.Extension}?language={language}";
                            }
                        }
                    }
                }
                if (webPageContentQueryData.TryGetValue(nameof(IBaseMetadata.MetaData_NoIndex), out bool? metaDataNoIndex) && metaDataNoIndex.HasValue) {
                    noIndex = metaDataNoIndex.Value;
                    dataFound = true;
                }
                var translations = await _contentTranslationInformationRepository.GetWebpageTranslationSummaries(webPageContentQueryData.WebPageItemID, webPageContentQueryData.WebPageItemWebsiteChannelID);
                if (translations.OrderByDescending(x => x.LanguageName.Equals(_languageIdentifierRepository.LanguageIdToName(webPageContentQueryData.ContentItemCommonDataContentLanguageID), StringComparison.OrdinalIgnoreCase)).TryGetFirst(out var properItem)) {
                    url = properItem.Url;
                } else {
                    url = $"/{webPageContentQueryData.WebPageUrlPath}";
                }
            } catch (Exception) {

            }
            return Result.SuccessIf(dataFound, new PageMetaData() {
                Title = title.AsNullOrWhitespaceMaybe(),
                Description = description.AsNullOrWhitespaceMaybe(),
                Keywords = keywords.AsNullOrWhitespaceMaybe(),
                NoIndex = noIndex.AsMaybe(),
                Thumbnail = ogImage.AsNullOrWhitespaceMaybe(),
                CanonicalUrl = url.AsNullOrWhitespaceMaybe()
            }, "Does not have any MetaData values");

        }

        private async Task<Result<ContentItemAssetMetadata>> GetImageMetadataField(Guid assetContentItemGuid, string className, string fieldName, string language)
        {

            // Swallow exception, the preview throws an error if it's from a search index build.
            bool isPreview = false;
            try {
                isPreview = _websiteChannelContext.IsPreview;
            } catch (NullReferenceException) { }

            var results = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"contentitem|bycontenttype|{className}");
                }
                var query = new ContentItemQueryBuilder().ForContentType(className, configure =>
                configure
                    .Columns(nameof(ContentItemFields.ContentItemGUID), fieldName)
                ).InLanguage(language, useLanguageFallbacks: true);


                return (await _contentQueryExecutor.GetResult(query, x => x, new ContentQueryExecutionOptions() { ForPreview = isPreview, IncludeSecuredItems = true }))
                    .GroupBy(x => x.ContentItemGUID)
                    .ToDictionary(key => key.Key, value => {
                        try {
                            return JsonSerializer.Deserialize<ContentItemAssetMetadata>(value.First().GetValue<string>(fieldName)).AsMaybe().TryGetValue(out var metaData) ? metaData : null;
                        } catch (Exception) {

                        }
                        return null;
                    });
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetImageMetadataField", className, language, isPreview));

            return results.TryGetValue(assetContentItemGuid, out var contentItemAssetMetadata) && contentItemAssetMetadata != null ? contentItemAssetMetadata : Result.Failure<ContentItemAssetMetadata>("Could not find or parse");
        }
    }
}
