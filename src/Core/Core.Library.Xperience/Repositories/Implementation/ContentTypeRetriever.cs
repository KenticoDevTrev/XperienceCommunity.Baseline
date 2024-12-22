using CMS.ContentEngine.Internal;
using CMS.Websites.Routing;
using System.Data;

namespace Core.Repositories.Implementation
{
    public class ContentTypeRetriever(IProgressiveCache progressiveCache,
        IWebsiteChannelContext websiteChannelContext) : IContentTypeRetriever
    {
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;
        #region "Content Class Lookups"

        public async Task<Result<string>> GetContentType(ContentIdentity identity)
        {
            var classIdToString = await GetClassIdToNameDictionary();
            var dictionary = await GetContentItemToClassIdDictionaries();
            if (identity.ContentID.TryGetValue(out var contentId) && dictionary.ByContentItemId.TryGetValue(contentId, out var classIdById) && classIdToString.TryGetValue(classIdById, out var classById)) {
                return classById;
            }
            if (identity.ContentGuid.TryGetValue(out var contentGuid) && dictionary.ByContentItemGuid.TryGetValue(contentGuid, out var classIdByGuid) && classIdToString.TryGetValue(classIdByGuid, out var classByGuid)) {
                return classByGuid;
            }
            if (identity.ContentName.TryGetValue(out var contentName) && dictionary.ByContentItemName.TryGetValue(contentName.ToLowerInvariant().Trim(), out var classIdByName) && classIdToString.TryGetValue(classIdByName, out var classByName)) {
                return classByName;
            }
            return Result.Failure<string>("Could not find class name for this ContentIdentity");
        }

        public async Task<Result<string>> GetContentType(ContentCultureIdentity identity)
        {
            var dictionary = await GetContentItemCultureToContentItemIdDictionaries();
            if (identity.ContentCultureID.TryGetValue(out var contentCultureId) && dictionary.ByContentItemCommonDataId.TryGetValue(contentCultureId, out var contentItemIdById)) {
                return await GetContentType(contentItemIdById.ToContentIdentity());
            } else if (identity.ContentCultureGuid.TryGetValue(out var contentCultureGuid) && dictionary.ByContentItemCommonDataGuid.TryGetValue(contentCultureGuid, out var contentItemIdByGuId)) {
                return await GetContentType(contentItemIdByGuId.ToContentIdentity());
            }
            return Result.Failure<string>("Could not find class name for this ContentCultureIdentity");
        }

        public async Task<Result<string>> GetContentType(TreeIdentity identity)
        {
            var dictionary = await GetTreeIdentityToContentItemIdDictionaries();
            if (identity.PageID.TryGetValue(out var pageId) && dictionary.ByWebPageItemId.TryGetValue(pageId, out var contentItemIdById)) {
                return await GetContentType(contentItemIdById.ToContentIdentity());
            }
            if (identity.PageGuid.TryGetValue(out var pageGuid) && dictionary.ByWebPageItemGuid.TryGetValue(pageGuid, out var contentItemIdByGuid)) {
                return await GetContentType(contentItemIdByGuid.ToContentIdentity());
            }
            if (identity.PageName.TryGetValue(out var pageName) && dictionary.ByWebPageItemName.TryGetValue(pageName, out var contentItemIdByName)) {
                return await GetContentType(contentItemIdByName.ToContentIdentity());
            }
            if (identity.PathChannelLookup.TryGetValue(out var pathChannel) && dictionary.ByWebPageItemPathChannel.TryGetValue(new PathChannel(pathChannel.Path, pathChannel.ChannelId.GetValueOrDefault(_websiteChannelContext.WebsiteChannelID)).GetCacheKey().ToLowerInvariant(), out var contentItemIdByPath)) {
                return await GetContentType(contentItemIdByPath.ToContentIdentity());
            }
            return Result.Failure<string>("Could not find class name for this TreeIdentity");
        }

        public Task<Result<string>> GetContentType(TreeCultureIdentity identity) => GetContentType((TreeIdentity)identity);

        #endregion

        #region "Content Type Dictionary Builders"

        private async Task<Dictionary<int, string>> GetClassIdToNameDictionary()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"cms.class|all");
                }
                var query = $"select {nameof(DataClassInfo.ClassID)}, {nameof(DataClassInfo.ClassName)} from CMS_Class";
                return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>()
                .ToDictionary(key => (int)key[nameof(DataClassInfo.ClassID)], value => (string)value[nameof(DataClassInfo.ClassName)]);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetClassIdToNameDictionary"));
        }

        private async Task<ContentItemToClassIdDictionaries> GetContentItemToClassIdDictionaries()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"contentitem|all");
                }
                var query = $"select {nameof(ContentItemInfo.ContentItemID)}, {nameof(ContentItemInfo.ContentItemGUID)}, {nameof(ContentItemInfo.ContentItemName)}, {nameof(ContentItemInfo.ContentItemContentTypeID)} from CMS_ContentItem where {nameof(ContentItemInfo.ContentItemContentTypeID)} is not null";
                var results = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                var byId = new Dictionary<int, int>();
                var byGuid = new Dictionary<Guid, int>();
                var byName = new Dictionary<string, int>();
                foreach (var dr in results) {
                    int classId = (int)dr[nameof(ContentItemInfo.ContentItemContentTypeID)];
                    byId.Add((int)dr[nameof(ContentItemInfo.ContentItemID)], classId);
                    byGuid.Add((Guid)dr[nameof(ContentItemInfo.ContentItemGUID)], classId);
                    byName.Add(((string)dr[nameof(ContentItemInfo.ContentItemName)]).ToLowerInvariant().Trim(), classId);
                }
                return new ContentItemToClassIdDictionaries(byId, byName, byGuid);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetContentItemToClassIdDictionaries"));
        }

        private async Task<ContentItemCultureToContentItemIdDictionaries> GetContentItemCultureToContentItemIdDictionaries()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"contentitem|all");
                }
                var query = $"select {nameof(ContentItemCommonDataInfo.ContentItemCommonDataID)}, {nameof(ContentItemCommonDataInfo.ContentItemCommonDataGUID)}, {nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID)} from CMS_ContentItemCommonData";
                var results = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                var byId = new Dictionary<int, int>();
                var byGuid = new Dictionary<Guid, int>();
                foreach (var dr in results) {
                    int contentItemId = (int)dr[nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID)];
                    byId.Add((int)dr[nameof(ContentItemCommonDataInfo.ContentItemCommonDataID)], contentItemId);
                    byGuid.Add((Guid)dr[nameof(ContentItemCommonDataInfo.ContentItemCommonDataGUID)], contentItemId);
                }
                return new ContentItemCultureToContentItemIdDictionaries(byId, byGuid);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetContentItemCultureToContentItemIdDictionaries"));
        }

        private async Task<TreeIdentityToContentItemIdDictionaries> GetTreeIdentityToContentItemIdDictionaries()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"webpageitem|all");
                }
                var query = $"select WebPageItemID, WebPageItemGUID, WebPageItemName, WebPageItemTreePath,  WebPageItemWebsiteChannelID, WebPageItemContentItemID from CMS_WebPageItem";
                var results = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                var byId = new Dictionary<int, int>();
                var byName = new Dictionary<string, int>();
                var byGuid = new Dictionary<Guid, int>();
                var byPathChannelKey = new Dictionary<string, int>();
                foreach (var dr in results) {
                    int contentItemId = (int)dr["WebPageItemContentItemID"];
                    byId.Add((int)dr["WebPageItemID"], contentItemId);
                    byGuid.Add((Guid)dr["WebPageItemGUID"], contentItemId);
                    byName.Add(((string)dr["WebPageItemName"]).ToLowerInvariant().Trim(), contentItemId);
                    byPathChannelKey.Add(new PathChannel(((string)dr["WebPageItemTreePath"]).ToLowerInvariant().Trim(), (int)dr["WebPageItemWebsiteChannelID"]).GetCacheKey().ToLowerInvariant(), contentItemId);
                }
                return new TreeIdentityToContentItemIdDictionaries(byId, byName, byGuid, byPathChannelKey);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetTreeIdentityToContentItemIdDictionaries"));
        }

        #endregion

        private class ContentItemToClassIdDictionaries(Dictionary<int, int> byContentItemId, Dictionary<string, int> byContentItemName, Dictionary<Guid, int> byContentItemGuid)
        {
            public Dictionary<int, int> ByContentItemId { get; } = byContentItemId;
            public Dictionary<string, int> ByContentItemName { get; } = byContentItemName;
            public Dictionary<Guid, int> ByContentItemGuid { get; } = byContentItemGuid;
        }

        private class ContentItemCultureToContentItemIdDictionaries(Dictionary<int, int> byContentItemCommonDataId, Dictionary<Guid, int> byContentItemCommonDataGuid)
        {
            public Dictionary<int, int> ByContentItemCommonDataId { get; } = byContentItemCommonDataId;
            public Dictionary<Guid, int> ByContentItemCommonDataGuid { get; } = byContentItemCommonDataGuid;
        }

        private class TreeIdentityToContentItemIdDictionaries(Dictionary<int, int> byWebPageItemId, Dictionary<string, int> byWebPageItemName, Dictionary<Guid, int> byWebPageItemGuid, Dictionary<string, int> byWebPageItemPathChannel)
        {
            public Dictionary<int, int> ByWebPageItemId { get; } = byWebPageItemId;
            public Dictionary<string, int> ByWebPageItemName { get; } = byWebPageItemName;
            public Dictionary<Guid, int> ByWebPageItemGuid { get; } = byWebPageItemGuid;
            public Dictionary<string, int> ByWebPageItemPathChannel { get; } = byWebPageItemPathChannel;
        }
    }
}
