﻿using CMS.ContentEngine;
using CMS.Core;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.PageBuilder.Web.Mvc;
using System.Data;

namespace Core.Repositories.Implementation
{
    public class PageContextRepository(IWebPageDataContextRetriever webPageDataContextRetriever,
        IPageBuilderDataContextRetriever pageBuilderDataContextRetriever,
        IProgressiveCache progressiveCache,
        ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
        ICacheRepositoryContext cacheRepositoryContext,
        IUrlResolver urlResolver,
        IIdentityService identityService,
        ILanguageRepository languageFallbackRepository,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        IWebsiteChannelContext websiteChannelContext,
        IContentQueryExecutor contentQueryExecutor,
        ISiteRepository siteRepository
        ) : IPageContextRepository
    {
        private readonly IWebPageDataContextRetriever _webPageDataContextRetriever = webPageDataContextRetriever;
        private readonly IPageBuilderDataContextRetriever _pageBuilderDataContextRetriever = pageBuilderDataContextRetriever;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly IUrlResolver _urlResolver = urlResolver;
        private readonly IIdentityService _identityService = identityService;
        private readonly ILanguageRepository _languageFallbackRepository = languageFallbackRepository;
        private readonly IPreferredLanguageRetriever _preferredLanguageRetriever = preferredLanguageRetriever;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly ISiteRepository _siteRepository = siteRepository;

        public async Task<Result<PageIdentity>> GetCurrentPageAsync() => _webPageDataContextRetriever.TryRetrieve(out var data) ? await GetPageInternal(data.WebPage.WebPageItemID, data.WebPage.LanguageName) : Result.Failure<PageIdentity>("There is no current webpage context");


        public async Task<Result<PageIdentity>> GetPageAsync(TreeIdentity identity) => (await identity.GetOrRetrievePageID(_identityService)).TryGetValue(out var webPageItemId) ? await GetPageInternal(webPageItemId) : Result.Failure<PageIdentity>("No Web Page Item Found with that identity");

        public async Task<Result<PageIdentity>> GetPageAsync(TreeCultureIdentity identity) => (await identity.GetOrRetrievePageID(_identityService)).TryGetValue(out var webPageItemId) ? await GetPageInternal(webPageItemId, identity.Culture.ToLowerInvariant()) : Result.Failure<PageIdentity>("No Web Page Item Found with that identity");

        [Obsolete("Not supported in Xperience by Kentico, use GetPageAsync(TreeIdentity)")]
        public Task<Result<PageIdentity>> GetPageAsync(NodeIdentity identity)
        {
            throw new NotImplementedException("Not supported in Xperience by Kentico, use GetPageAsync(TreeIdentity)");
        }

        [Obsolete("Not supported in Xperience by Kentico, use GetPageAsync(TreeCultureIdentity)")]
        public Task<Result<PageIdentity>> GetPageAsync(DocumentIdentity identity)
        {
            throw new NotImplementedException("Not supported in Xperience by Kentico, use GetPageAsync(TreeIdentity)");
        }

        public Task<bool> IsEditModeAsync() => Task.FromResult(_pageBuilderDataContextRetriever.Retrieve().EditMode);

        public Task<bool> IsPreviewModeAsync() => Task.FromResult(_websiteChannelContext.IsPreview);

        private async Task<Result<PageIdentity>> GetPageInternal(int webPageItemID, string? language = null)
        {
            var lang = (language ?? _preferredLanguageRetriever.Get()).ToLowerInvariant();
            var lookupDictionary = await GetPageIdentityByWebpageIdAndLanguage();

            // Use cached version if possible
            if (_cacheDependencyBuilderFactory.Create(false).AddKey("webpageitem|all").DependenciesNotTouchedSince(new TimeSpan(0, 0, 30))) {
                if (lookupDictionary.TryGetValue(webPageItemID, out var langToIdentity)
                    &&
                    (await _languageFallbackRepository.GetLanguagueToSelect(langToIdentity.Keys, lang, true)).TryGetValue(out var langToUse)) {

                    // Absolute URL must be done outside of caching
                    var item = langToIdentity[langToUse];
                    return item with { AbsoluteUrl = _urlResolver.GetAbsoluteUrl(item.RelativeUrl) };
                }
                return Result.Failure<PageIdentity>("No Web Page Item Found with that identity");
            } else {
                // Individual lookup
                var builder = _cacheDependencyBuilderFactory.Create()
                    .WebPageOnLanguage(webPageItemID, lang);

                var item = await _progressiveCache.LoadAsync(async cs => {
                    if (cs.Cached) {
                        cs.CacheDependency = builder.GetCMSCacheDependency();
                    }
                    // individual lookup
                    var query = _baseQuery + $" and WebPageItemID = {webPageItemID}";
                    var items = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>()
                        .ToDictionary(key => ((string)key["ContentLanguageName"]).ToLowerInvariant(), DataRowToPageIdentity
                            );

                    if ((await _languageFallbackRepository.GetLanguagueToSelect(items.Keys, lang, true)).TryGetValue(out var langToUse)) {
                        return items[langToUse];
                    }

                    return Result.Failure<PageIdentity>("No Web Page Item Found with that identity");
                }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "GetPageInternal", webPageItemID, language));

                // Handle absolute URL after
                return item.TryGetValue(out var itemVal) ? itemVal with { AbsoluteUrl = _urlResolver.GetAbsoluteUrl(itemVal.RelativeUrl) } : item;
            }
        }

        private async Task<Dictionary<int, Dictionary<string, PageIdentity>>> GetPageIdentityByWebpageIdAndLanguage()
        {
            return await _progressiveCache.LoadAsync(async cs => {

                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency("webpageitem|all");
                }

                var query = _baseQuery;
                return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>()
                .GroupBy(x => (int)x["WebPageItemID"])
                .ToDictionary(key => key.Key, value =>
                    value.ToDictionary(key => ((string)key["ContentLanguageName"]).ToLowerInvariant(),
                        value => DataRowToPageIdentity(value)
                        )
                    );
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetPageIdentityByWebpageIdAndLanguage"));
        }
        private string _baseQuery = @"
select 
ContentItemLanguageMetadataDisplayName,
MetaData_PageName,
WebPageItemName, 
WebPageItemID,
WebPageItemGUID,
ContentItemID,
ContentItemName,
ContentItemGUID,
ContentItemLanguageMetadataID,
ContentItemLanguageMetadataGUID,
WebPageItemTreePath,
ContentLanguageName,
WebPageUrlPath,
WebsiteChannelChannelID,
ClassName
from CMS_WebPageItem
inner join CMS_ContentItem on WebPageItemContentItemID = ContentItemID
inner join CMS_ContentItemLanguageMetadata on ContentItemLanguageMetadataContentItemID = ContentItemID
inner join CMS_ContentLanguage on ContentLanguageID = ContentItemLanguageMetadataContentLanguageID
inner join CMS_WebsiteChannel on WebsiteChannelID = WebPageItemWebsiteChannelID
inner join CMS_Class on ClassID = ContentItemContentTypeID
inner join CMS_WebPageUrlPath on WebPageUrlPathWebPageItemID = WebPageItemID and WebPageUrlPathContentLanguageID = ContentLanguageID
inner join CMS_ContentItemCommonData on ContentItemCommonDataContentItemID = ContentItemID and ContentItemCommonDataContentLanguageID = ContentItemLanguageMetadataContentLanguageID
where ContentItemCommonDataIsLatest = 1 and WebPageUrlPathIsLatest = 1
";

        private PageIdentity DataRowToPageIdentity(DataRow value) => new(
                            name: value.Field<string?>("MetaData_PageName").GetValueOrDefault((string)value["ContentItemLanguageMetadataDisplayName"]),
                            alias: (string)value["WebPageItemName"],
                            pageID: (int)value["WebPageItemID"],
                            pageGuid: (Guid)value["WebPageItemGUID"],
                            contentID: (int)value["ContentItemID"],
                            contentName: (string)value["ContentItemName"],
                            contentGuid: (Guid)value["ContentItemGUID"],
                            contentCultureID: (int)value["ContentItemLanguageMetadataID"],
                            contentCultureGuid: (Guid)value["ContentItemLanguageMetadataGUID"],
                            path: (string)value["WebPageItemTreePath"],
                            culture: (string)value["ContentLanguageName"],
                            relativeUrl: "/" + (string)value["WebPageUrlPath"],
                            absoluteUrl: string.Empty, // Needs to be generated after cache
                            level: ((string)value["WebPageItemTreePath"]).Split("/", StringSplitOptions.RemoveEmptyEntries).Length,
                            channelID: (int)value["WebsiteChannelChannelID"],
                            pageType: (string)value["ClassName"]
                            );

        public async Task<Result<PageIdentity<T>>> GetCurrentPageAsync<T>() => await GetTypedPageIdentityResult<T>(await GetCurrentPageAsync());

        public async Task<Result<PageIdentity<T>>> GetPageAsync<T>(TreeIdentity identity) => await GetTypedPageIdentityResult<T>(await GetPageAsync(identity));

        public async Task<Result<PageIdentity<T>>> GetPageAsync<T>(TreeCultureIdentity identity) => await GetTypedPageIdentityResult<T>(await GetPageAsync(identity));

        private async Task<Result<PageIdentity<T>>> GetTypedPageIdentityResult<T>(Result<PageIdentity> pageIdentityResult)
        {
            if (!pageIdentityResult.TryGetValue(out var pageIdentity, out var error)) {
                return Result.Failure<PageIdentity<T>>(error);
            }

            if (!(await GetFullPageModel(pageIdentity)).TryGetValue(out var fullPageModel, out var modelError)) {
                return Result.Failure<PageIdentity<T>>(modelError);
            }

            if (fullPageModel is T typedModel) {
                return new PageIdentity<T>(typedModel, pageIdentity);
            }

            return Result.Failure<PageIdentity<T>>("The original model is not of that type");
        }

        private async Task<Result<object>> GetFullPageModel(PageIdentity pageIdentity) {

            var builder = _cacheDependencyBuilderFactory.Create()
                .WebPage(pageIdentity.PageGuid);

            var className = pageIdentity.PageType;
            var webChannelName = _siteRepository.ChannelNameById(pageIdentity.ChannelID);
            if (!GetContentTypeByName().TryGetValue(className.ToLowerInvariant(), out var contentType)
                || string.IsNullOrWhiteSpace(webChannelName)) {
                return Result.Failure<object>($"Could not find a matching RegisterContentTypeMapping class for {className} or could not parse Website Channel Name from ID {pageIdentity.ChannelID}");
            }

            return await _progressiveCache.LoadAsync(async cs => {

                if(cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                if (_contentQueryExecutor is not IContentQueryModelTypeMapper typeMapper) {
                    return Result.Failure<object>($"The IContentQueryExecutor is not of type IContentQueryModelTypeMapper, so can't map dynamically");
                }
                var typedMapper = typeMapper.GetType().GetMethod("Map")?.MakeGenericMethod(contentType) ?? null;
                if (typedMapper == null) {
                    return Result.Failure<object>($"The IContentQueryModelTypeMapper for some reason no longer has the Map<T> Method, can't proceed");
                }

                var getPageFull = new ContentItemQueryBuilder().ForContentType(pageIdentity.PageType, query => query
                        .ForWebsite(webChannelName, includeUrlPath: true)
                        .WithLinkedItems(100)
                        .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemID), pageIdentity.PageID))
                        .TopN(1)
                        )
                    .InLanguage(pageIdentity.Culture);

                var dataContainerResults = await _contentQueryExecutor.GetWebPageResult(getPageFull, (dataContainer) => dataContainer, new ContentQueryExecutionOptions() { ForPreview = _cacheRepositoryContext.PreviewEnabled(), IncludeSecuredItems = true });

                if (!dataContainerResults.FirstOrMaybe().TryGetValue(out var firstItem)) {
                    return Result.Failure<object>($"No Web Page Item Found!");
                }

                var result = typedMapper.Invoke(typeMapper, [firstItem]);
                return result != null ? result : Result.Failure<object>("Mapped object was null.");
            }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "PageContextRepository_GetFullPageModel", pageIdentity.GetCacheKey(), _cacheRepositoryContext.GetCacheKey()));
        }

        private Dictionary<string, Type> GetContentTypeByName()
        {
            return _progressiveCache.Load(cs => {
                var contentTypeByName = new Dictionary<string, Type>();
                foreach (var assembly in AssemblyDiscoveryHelper.GetAssemblies(true)) {
                    foreach (var type in assembly.GetTypes()) {
                        var attribute = type.GetCustomAttributes(typeof(RegisterContentTypeMappingAttribute), true);
                        if (attribute.Length > 0) {
                            contentTypeByName.TryAdd(((RegisterContentTypeMappingAttribute)attribute[0]).ContentTypeName.ToLowerInvariant(), type);
                        }
                    }
                }
                return contentTypeByName;
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "PageContextRepository_GetContentTypeByName"));
        }


       
    }


}
