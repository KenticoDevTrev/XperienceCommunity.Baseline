using CMS.Core;
using CMS.Websites;
using Kentico.Content.Web.Mvc.Routing;

namespace Core.Repositories.Implementation
{
    public class MappedContentItemRepository(IIdentityService identityService,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        IProgressiveCache progressiveCache,
        ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
        IContentQueryExecutor contentQueryExecutor,
        ISiteRepository siteRepository,
        ICacheRepositoryContext cacheRepositoryContext) : IMappedContentItemRepository
    {
        private readonly IIdentityService _identityService = identityService;
        private readonly IPreferredLanguageRetriever _preferredLanguageRetriever = preferredLanguageRetriever;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly ISiteRepository _siteRepository = siteRepository;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;

        public async Task<Result<object>> GetContentItem(ContentIdentity identity, string? language = null)
        {
            var lookups = await LookupToContentType();
            var classToLookup = await ClassIDToLookupInfo();
            var webIdLookup = await GetContentToWebpageLookups();
            var lookupInfo = Maybe<ContentItemLookupInfo>.None;
            var websiteChannelID = Maybe<int>.None;
            var contentItemId = Maybe<int>.None;
            var contentItemGuid = Maybe<Guid>.None;

            var whereCondition = new WhereCondition();
            var builder = _cacheDependencyBuilderFactory.Create();

            // Try to see if ID or GUID available, use those, otherwise will need to look up on ID
            if (identity.ContentGuid.TryGetValue(out var guid)
                && lookups.ByContentGuid.TryGetValue(guid, out var classByGuid)
                && classToLookup.TryGetValue(classByGuid.ClassId, out var lookupInfoByGuid)) {
                lookupInfo = lookupInfoByGuid;
                contentItemGuid = guid;
                websiteChannelID = classByGuid.WebsiteChannelID;
                builder.ContentItem(guid);
                if(webIdLookup.ContentItemGuidToWebPageGuid.TryGetValue(guid, out var webpageGuid)) {
                    builder.WebPage(webpageGuid);
                }
            } else if (
                  (identity.ContentID.TryGetValue(out var id) || (await identity.GetOrRetrieveContentID(_identityService)).TryGetValue(out id))
                && lookups.ByContentId.TryGetValue(id, out var classById)
                && classToLookup.TryGetValue(classById.ClassId, out var lookupInfoById)) {
                lookupInfo = lookupInfoById;
                contentItemId = id;
                websiteChannelID = classById.WebsiteChannelID;
                builder.ContentItem(id);
                if (webIdLookup.ContentItemIDToWebPageID.TryGetValue(id, out var webpageId)) {
                    builder.WebPage(webpageId);
                }
            }

            if (!lookupInfo.TryGetValue(out var lookupInfoVal)) {
                return Result.Failure<Result<object>>("Could not find Content Item ID");
            }

            string lang = !string.IsNullOrWhiteSpace(language) ? language : _preferredLanguageRetriever.Get();
            var contentType = lookupInfoVal.ClassType;
            var className = lookupInfoVal.ClassName;
            var isWebsite = lookupInfoVal.IsWebPage;
            var websiteName = websiteChannelID.TryGetValue(out var webChannelID) ? _siteRepository.ChannelNameById(webChannelID) : string.Empty;

            // Actual logic now to get and map item
            return await _progressiveCache.LoadAsync(async cs => {

                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                if (_contentQueryExecutor is not IContentQueryModelTypeMapper typeMapper) {
                    return Result.Failure<object>($"The IContentQueryExecutor is not of type IContentQueryModelTypeMapper, so can't map dynamically");
                }
                var typedMapper = typeMapper.GetType().GetMethod("Map")?.MakeGenericMethod(contentType) ?? null;
                if (typedMapper == null) {
                    return Result.Failure<object>($"The IContentQueryModelTypeMapper for some reason no longer has the Map<T> Method, can't proceed");
                }

                var getPageFull =  new ContentItemQueryBuilder().ForContentType(className, query => query
                        .If(isWebsite, webQuery => webQuery.ForWebsite(websiteName, includeUrlPath: true))
                        .If(contentItemId.HasValue, queryWhere => queryWhere.Where(where => where.WhereEquals(nameof(ContentItemFields.ContentItemID), contentItemId.GetValueOrDefault(0))))
                        .If(contentItemGuid.HasValue, queryWhere => queryWhere.Where(where => where.WhereEquals(nameof(ContentItemFields.ContentItemGUID), contentItemGuid.GetValueOrDefault(Guid.Empty))))
                        .WithLinkedItems(100, x => x.IncludeWebPageData(true))                        
                        .TopN(1)
                )
                .InLanguage(lang);

                var dataContainerResults = await _contentQueryExecutor.GetWebPageResult(getPageFull, (dataContainer) => dataContainer, new ContentQueryExecutionOptions() { ForPreview = _cacheRepositoryContext.PreviewEnabled(), IncludeSecuredItems = true });

                if (!dataContainerResults.TryGetFirst(out var firstItem)) {
                    return Result.Failure<object>($"No Web Page Item Found!");
                }

                var result = typedMapper.Invoke(typeMapper, [firstItem]);
                return result != null ? result : Result.Failure<object>("Mapped object was null.");
            }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "ContentItemRepository_GetContentItem", contentItemId.GetValueOrDefault(0), contentItemGuid.GetValueOrDefault(Guid.Empty), _cacheRepositoryContext.GetCacheKey()));
        }

        public async Task<Result<T>> GetContentItem<T>(ContentIdentity identity, string? language = null)
        {
            var results = await GetContentItem(identity, language);

            if (results.TryGetValue(out var foundItem, out var error)) {
                if (foundItem is T tItem) {
                    return tItem;
                } else {
                    return Result.Failure<T>($"Item found but is not of type {typeof(T).FullName}");
                }
            }
            return Result.Failure<T>(error);
        }

        private async Task<ContentItemDictionaryHolder> LookupToContentType()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(["contentitem|all", "webpageitem|all"]);
                }
                var contentItemToClassIDQuery = @$"select ContentItemID, ContentItemGUID, ContentItemContentTypeID, WebPageItemWebsiteChannelID from CMS_ContentItem 
left join CMS_WebPageItem on WebPageItemContentItemID = ContentItemID
where ContentItemContentTypeID is not null";
                var items = (await (new DataQuery() { CustomQueryText = contentItemToClassIDQuery }).GetDataContainerResultAsync())
                    .Select(x => (ContentItemID: (int)x.GetValue("ContentItemID"), ContentItemGuid: (Guid)x.GetValue("ContentItemGUID"), ContentItemContentTypeID: (int)x.GetValue("ContentItemContentTypeID"), WebPageItemWebsiteChannelID: (int?)x.GetValue("WebPageItemWebsiteChannelID")));

                return new ContentItemDictionaryHolder(
                    items.ToDictionary(key => key.ContentItemID, value => new ClassAndWebsiteChannel(value.ContentItemContentTypeID, value.WebPageItemWebsiteChannelID.AsMaybeStatic())),
                    items.ToDictionary(key => key.ContentItemGuid, value => new ClassAndWebsiteChannel(value.ContentItemContentTypeID, value.WebPageItemWebsiteChannelID.AsMaybeStatic()))
                    );

            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "ContentItemRepository_GetLookupDictionaries"));
        }

        private async Task<Dictionary<int, ContentItemLookupInfo>> ClassIDToLookupInfo()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(["cms.class|all"]);
                }
                var contentTypeByName = new Dictionary<string, Type>();
                foreach (var assembly in AssemblyDiscoveryHelper.GetAssemblies(true)) {
                    foreach (var type in assembly.GetTypes()) {
                        var attribute = type.GetCustomAttributes(typeof(RegisterContentTypeMappingAttribute), true);
                        if (attribute.Length > 0) {
                            contentTypeByName.TryAdd(((RegisterContentTypeMappingAttribute)attribute[0]).ContentTypeName.ToLowerInvariant(), type);
                        }
                    }
                }

                var classInfoQuery = $"select {nameof(DataClassInfo.ClassID)}, {nameof(DataClassInfo.ClassName)}, case when {nameof(DataClassInfo.ClassContentTypeType)} = 'Website' then 1 else 0 end as IsWebPage from CMS_Class where {nameof(DataClassInfo.ClassContentTypeType)} in ('Website', 'ReusableSchema')";
                var classLookup = (await (new DataQuery() { CustomQueryText = classInfoQuery }).GetDataContainerResultAsync())
                                        .Where(x => contentTypeByName.ContainsKey(((string)x.GetValue(nameof(DataClassInfo.ClassName))).ToLowerInvariant()))
                                        .ToDictionary(key => (int) key.GetValue(nameof(DataClassInfo.ClassID)), value => new ContentItemLookupInfo((string)value.GetValue(nameof(DataClassInfo.ClassName)), contentTypeByName[((string)value.GetValue(nameof(DataClassInfo.ClassName))).ToLowerInvariant()], (int)value.GetValue("IsWebPage") == 1));
                return classLookup;
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "ContentItemRepository_ClassIDToLookupInfo"));
        }

        private async Task<ContentToWebpageIdentifiers> GetContentToWebpageLookups()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency("webpageitem|all");
                }

                var results = await (new DataQuery() { CustomQueryText = @"select ContentItemID, ContentItemGUID, WebPageItemID, WebPageItemGUID from CMS_WebPageItem
 inner join CMS_ContentItem on ContentItemID = WebPageItemContentItemID" }).GetDataContainerResultAsync();

                return new ContentToWebpageIdentifiers(
                    ContentItemIDToWebPageID: results.ToDictionary(key => (int)key.GetValue(nameof(ContentItemFields.ContentItemID)), value => (int)value.GetValue(nameof(WebPageFields.WebPageItemID))),
                    ContentItemGuidToWebPageGuid: results.ToDictionary(key => (Guid)key.GetValue(nameof(ContentItemFields.ContentItemGUID)), value => (Guid)value.GetValue(nameof(WebPageFields.WebPageItemGUID)))
                );
            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "GetContentToWebpageLookups"));
        }

        private record ContentToWebpageIdentifiers(Dictionary<int, int> ContentItemIDToWebPageID, Dictionary<Guid, Guid> ContentItemGuidToWebPageGuid);

        private record ContentItemDictionaryHolder(Dictionary<int, ClassAndWebsiteChannel> ByContentId, Dictionary<Guid, ClassAndWebsiteChannel> ByContentGuid);

        private record ClassAndWebsiteChannel(int ClassId, Maybe<int> WebsiteChannelID);

        private record ContentItemLookupInfo(string ClassName, Type ClassType, bool IsWebPage);
    }


}
