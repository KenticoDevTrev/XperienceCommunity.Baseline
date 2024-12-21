using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using Core.Enums;
using Core.Models;
using Core.Services;
using Generic;
using Kentico.Xperience.Lucene.Core.Indexing;
using Lucene.Net.Documents;
using MVCCaching;
using Search.WebCrawler;
using System.Data;

namespace Search.Library.Xperience.Lucene.IndexStrategies
{
    public class BaselineBaseMetadataIndexingStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor,
        IProgressiveCache progressiveCache,
        IWebPageQueryResultMapper webPageQueryResultMapper,
        IContentItemReferenceService contentItemReferenceService,
        IMetaDataWebPageDataContainerConverter metaDataWebPageDataContainerConverter,
        BaselineSearchLuceneWebCrawlerService baselineSearchLuceneWebCrawlerService,
        BaselineSearchLuceneWebScraperSanitizer baselineSearchLuceneWebScraperSanitizer
        ) : DefaultLuceneIndexingStrategy
    {
        private readonly IWebPageQueryResultMapper _webPageMapper = webPageMapper;
        private readonly IContentQueryExecutor _queryExecutor = queryExecutor;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IWebPageQueryResultMapper _webPageQueryResultMapper = webPageQueryResultMapper;
        private readonly IContentItemReferenceService _contentItemReferenceService = contentItemReferenceService;
        private readonly IMetaDataWebPageDataContainerConverter _metaDataWebPageDataContainerConverter = metaDataWebPageDataContainerConverter;
        private readonly BaselineSearchLuceneWebCrawlerService _baselineSearchLuceneWebCrawlerService = baselineSearchLuceneWebCrawlerService;
        private readonly BaselineSearchLuceneWebScraperSanitizer _baselineSearchLuceneWebScraperSanitizer = baselineSearchLuceneWebScraperSanitizer;
        public const string CRAWLER_CONTENT_FIELD_NAME = "Content";

        public override async Task<Document?> MapToLuceneDocumentOrNull(IIndexEventItemModel item)
        {
            var document = new Document();

            string title = string.Empty;

            // IIndexEventItemModel could be a reusable content item or a web page item, so we use
            // pattern matching to get access to the web page item specific type and fields
            if (item is IndexEventWebPageItemModel indexedPage) {
                // Get basic navigation data from IBaseMetada and IBaseRedirect
                var result = await GetBaseMetadataCached(indexedPage.ItemID, indexedPage.WebsiteChannelName, indexedPage.LanguageName);
                if (result == null) {
                    return null;
                }
                
                if (!(await _metaDataWebPageDataContainerConverter.GetDefaultMetadataLogic(result)).TryGetValue(out var metadata)) {
                    return null;
                }

                var additionalData = await GetAdditionalDataWebPage(indexedPage.ItemID, indexedPage.WebsiteChannelName, indexedPage.ContentTypeID, indexedPage.ContentLanguageID);


                document.Add(new TextField(nameof(PageMetaData.Title), metadata.Title.GetValueOrDefault(additionalData?.ContentItemLanguageMetadataDisplayName ?? indexedPage.Name), Field.Store.YES));
                var contentFromHtml = string.Empty;
                
                if (metadata.Keywords.TryGetValue(out var keywords)) {
                    document.Add(new TextField(nameof(PageMetaData.Keywords), keywords, Field.Store.YES));
                }
                if (metadata.CanonicalUrl.TryGetValue(out var url)) {
                    document.Add(new StringField(nameof(PageMetaData.CanonicalUrl), url, Field.Store.YES));
                    // Scrape the page to add content to it.
                    string rawContent = await _baselineSearchLuceneWebCrawlerService.CrawlPage(url);
                    contentFromHtml = _baselineSearchLuceneWebScraperSanitizer.SanitizeHtmlDocument(rawContent);
                    document.Add(new TextField("HtmlContent", contentFromHtml, Field.Store.NO));
                }
                // Metadata and the Content Field
                if (metadata.Description.TryGetValue(out var description)) {
                    document.Add(new TextField(nameof(PageMetaData.Description), description, Field.Store.YES));
                    document.Add(new TextField(CRAWLER_CONTENT_FIELD_NAME, description, Field.Store.YES));
                } else if(!string.IsNullOrWhiteSpace(contentFromHtml)) {
                    document.Add(new TextField(CRAWLER_CONTENT_FIELD_NAME, contentFromHtml, Field.Store.YES));
                }

                if (metadata.Thumbnail.TryGetValue(out var thumbnail)) {
                    document.Add(new StringField(nameof(PageMetaData.Thumbnail), thumbnail, Field.Store.YES));
                }

                // Backwords compatibility with Baseline
                document.Add(new StringField(nameof(PageMetaData.NoIndex), metadata.NoIndex.GetValueOrDefault(false).ToString().ToLower(), Field.Store.YES));
                document.Add(new StringField("ContentType", item.ContentTypeName, Field.Store.YES));
                document.Add(new StringField("Id", item.ItemID.ToString(), Field.Store.YES));
                if(additionalData != null) {
                    document.Add(new StringField("Created", additionalData.ContentItemLanguageMetadataCreatedWhen.ToString(), Field.Store.YES));
                }

                // Get Content from web crawl

                return document;
            }

            if (item is IndexEventReusableItemModel reusablePage) {
                // Todo, if lucene ever supports reusable content types, then will need to do parsing, maybe if it has a content asset item or something?
                
            }
            return null;
        }

        private async Task<IWebPageContentQueryDataContainer?> GetBaseMetadataCached(int id, string channelName, string languageName)
        {
            var itemsByGuid = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"webpageitem|all");
                }

                var query = new ContentItemQueryBuilder().ForContentTypes(
                    config =>
                        config
                            .OfReusableSchema([IBaseMetadata.REUSABLE_FIELD_SCHEMA_NAME])
                            .ForWebsite(channelName, includeUrlPath: true)
                    )
                    .InLanguage(languageName);

                var result = await _queryExecutor.GetWebPageResult(query, x => x, new ContentQueryExecutionOptions() { ForPreview = false, IncludeSecuredItems = true });
                return result.ToDictionary(key => key.WebPageItemID, value => value);

            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "GetBaseMetadataCached", channelName, languageName));

            return itemsByGuid.TryGetValue(id, out var webPage) ? webPage : null;
        }

        private async Task<AdditionalDataHolder?> GetAdditionalDataWebPage(int id, string channelName, int contentTypeID, int contentLanguageID)
        {
            var itemsById = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"webpageitem|all");
                }
                var query = @"select WebPageItemID, ContentLanguageID, ContentItemLanguageMetadataDisplayName, ContentItemLanguageMetadataCreatedWhen from CMS_WebPageItem
inner join CMS_WebsiteChannel on WebsiteChannelID = WebPageItemWebsiteChannelID
inner join CMS_Channel on ChannelID = WebsiteChannelChannelID
inner join CMS_ContentItem on ContentItemID = WebPageItemContentItemID
left outer join CMS_ContentLanguage on 1=1
inner join CMS_ContentItemLanguageMetadata on ContentItemLanguageMetadataContentItemID = ContentItemID and ContentItemLanguageMetadataContentLanguageID = ContentLanguageID
where ContentItemLanguageMetadataLatestVersionStatus = 2 and ChannelName = @ChannelName and ContentItemContentTypeID = @ContentTypeID";
                return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, new QueryDataParameters() { { "@ChannelName", channelName }, { "@ContentTypeID", contentTypeID } }, QueryTypeEnum.SQLQuery))
                    .Tables[0].Rows.Cast<DataRow>()
                        .Select(x => new AdditionalDataHolder((int)x["WebPageItemID"], (int)x["ContentLanguageID"], DataHelper.GetStringValue(x, "ContentItemLanguageMetadataDisplayName", ""), DataHelper.GetDateTimeValue(x, "ContentItemLanguageMetadataCreatedWhen", DateTimeHelper.ZERO_TIME)))
                        .GroupBy(x => x.WebPageItemID)
                        .ToDictionary(key => key.Key, value => value.Select(x => x));
            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "GetBaseMetadataCached", channelName, contentTypeID, contentLanguageID));

            if(itemsById.TryGetValue(id, out var items) && items.Any()) {
                return items.OrderByDescending(x => x.ContentLanguageID == contentLanguageID).First();
            }

            return null;
        }

        private record AdditionalDataHolder(int WebPageItemID, int ContentLanguageID, string ContentItemLanguageMetadataDisplayName, DateTime ContentItemLanguageMetadataCreatedWhen);
    }
}
