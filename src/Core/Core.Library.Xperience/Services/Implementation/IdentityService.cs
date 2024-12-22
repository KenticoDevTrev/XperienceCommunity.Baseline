using CMS.ContentEngine.Internal;
using CMS.Websites.Routing;
using System.Data;

namespace Core.Services.Implementation
{
    public class IdentityService(IProgressiveCache progressiveCache,
        ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
        IContentLanguageRetriever contentLanguageRetriever,
        IWebsiteChannelContext websiteChannelContext,
        ICacheReferenceService cacheReferenceService,
        ILanguageRepository languageFallbackRepository,
        IContentTypeRetriever contentTypeRetriever) : IIdentityService
    {
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider = contentLanguageInfoProvider;
        private readonly IContentLanguageRetriever _contentLanguageRetriever = contentLanguageRetriever;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;
        private readonly ICacheReferenceService _cacheReferenceService = cacheReferenceService;
        private readonly ILanguageRepository _languageFallbackRepository = languageFallbackRepository;
        private readonly IContentTypeRetriever _contentTypeRetriever = contentTypeRetriever;

        #region "Content Hydration"

        public async Task<Result<ContentIdentity>> HydrateContentIdentity(ContentIdentity identity)
        {
            if (identity.ContentID.HasValue && identity.ContentGuid.HasValue && identity.ContentName.HasValue)
            {
                return identity;
            }
            if (identity.ContentID.HasNoValue && identity.ContentGuid.HasNoValue && identity.ContentName.HasNoValue)
            {
                return Result.Failure<ContentIdentity>("No data given, can't hydrate");
            }

            var dictionaryResult = await GetContentIdentityHolder();
            if (dictionaryResult.TryGetValue(out var dictionary))
            {
                if (identity.ContentID.TryGetValue(out var id) && dictionary.Content.ById.TryGetValue(id, out var newIdentityFromId))
                {
                    return newIdentityFromId;
                }
                if (identity.ContentGuid.TryGetValue(out var guid) && dictionary.Content.ByGuid.TryGetValue(guid, out var newIdentityFromGuid))
                {
                    return newIdentityFromGuid;
                }
                if (identity.ContentName.TryGetValue(out var name) && dictionary.Content.ByName.TryGetValue(name, out var newIdentityFromName))
                {
                    return newIdentityFromName;
                }
                return Result.Failure<ContentIdentity>("Could not find content identity.");
            }
            else
            {
                // Can't use cached version, so just generate manually
                var queryParams = new QueryDataParameters();
                var query = $"select ContentItemID, ContentItemGuid, ContentItemName from CMS_ContentItem where 1=1";
                if (identity.ContentID.TryGetValue(out var id))
                {
                    query += $" and ContentItemID = @ContentItemID";
                    queryParams.Add(new DataParameter("@ContentItemID", id));
                }
                if (identity.ContentGuid.TryGetValue(out var guid))
                {
                    query += $" and ContentItemGuid = @ContentItemGuid";
                    queryParams.Add(new DataParameter("@ContentItemGuid", guid));
                }
                if (identity.ContentName.TryGetValue(out var name))
                {
                    query += $" and ContentItemName = @ContentItemName";
                    queryParams.Add(new DataParameter("@ContentItemName", name));
                }
                var item = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, queryParams, QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                if (item.FirstOrMaybe().TryGetValue(out var docRow))
                {

                    return new ContentIdentity()
                    {
                        ContentID = (int)docRow["ContentItemID"],
                        ContentGuid = (Guid)docRow["ContentItemGuid"],
                        ContentName = (string)docRow["ContentItemName"]
                    };
                }
                else
                {
                    return Result.Failure<ContentIdentity>("Could not find a content item with the given identity");
                }
            }
        }

        public async Task<Result<ContentCultureIdentity>> HydrateContentCultureIdentity(ContentCultureIdentity identity)
        {
            // If current identity is full, then just return it.
            if (identity.ContentCultureID.HasValue && identity.ContentCultureGuid.HasValue && identity.ContentCultureLookup.TryGetValue(out var contentCultureLookupCheck) && contentCultureLookupCheck.Culture.HasValue)
            {
                return identity;
            }
            if (identity.ContentCultureID.HasNoValue && identity.ContentCultureGuid.HasNoValue && identity.ContentCultureLookup.HasNoValue)
            {
                return Result.Failure<ContentCultureIdentity>("No data given, can't hydrate");
            }

            // Parse proper culture for lookups
            var currentChannelID = _websiteChannelContext.WebsiteChannelID;
            var defaultContentCulture = (await _contentLanguageRetriever.GetDefaultContentLanguage()).ContentLanguageName;
            var cultureFallback = currentChannelID == default ? defaultContentCulture : _cacheReferenceService.GetDefaultLanguageName(currentChannelID);
            var requestedCulture = identity.ContentCultureLookup.TryGetValue(out var contentCultureLookupForCulture) ? contentCultureLookupForCulture.Culture.GetValueOrDefault(cultureFallback) : cultureFallback;

            var cultureToFallback = await GetCultureToFallbackDictionary();

            var dictionaryResult = await GetContentIdentityHolder();
            if (dictionaryResult.TryGetValue(out var dictionary))
            {
                if (identity.ContentCultureID.TryGetValue(out var id) && dictionary.ContentCulture.ById.TryGetValue(id, out var newIdentityFromId))
                {
                    return newIdentityFromId;
                }
                if (identity.ContentCultureGuid.TryGetValue(out var guid) && dictionary.ContentCulture.ByGuid.TryGetValue(guid, out var newIdentityFromGuid))
                {
                    return newIdentityFromGuid;
                }
                if (identity.ContentCultureLookup.TryGetValue(out var contentCultureLookup) && dictionary.ContentCulture.ByContentIDAndCulture.TryGetValue(contentCultureLookup.ContentId, out var cultureToCultureIdentity))
                {
                    // Look through content culture variations by the given language, falling back to fallbacks
                    if((await _languageFallbackRepository.GetLanguagueToSelect(cultureToCultureIdentity.Keys, requestedCulture, true)).TryGetValue(out var cultureToUse)
                        && cultureToCultureIdentity.TryGetValue(cultureToUse.ToLowerInvariant(), out var cultureMatch)) {
                        return cultureMatch;
                    }
                    return Result.Failure<ContentCultureIdentity>("Could not find content culture identity in the language given or any valid fallback");
                }
                return Result.Failure<ContentCultureIdentity>("Could not find content culture identity.");
            }
            else
            {
                // Can't use cached version, so just generate manually, and use the API 

                var cultureToCheck = Maybe<string>.None;
                var queryParams = new QueryDataParameters();
                var query = @$"select ContentItemLanguageMetadataID, ContentItemLanguageMetadataGUID, ContentItemLanguageMetadataContentItemID, ContentLanguageName from CMS_ContentItemLanguageMetadata
inner join CMS_ContentLanguage on ContentLanguageID = ContentItemLanguageMetadataContentLanguageID where 1=1";
                if (identity.ContentCultureID.TryGetValue(out var id))
                {
                    query += $" and ContentItemLanguageMetadataID = @ContentItemLanguageMetadataID";
                    queryParams.Add(new DataParameter("@ContentItemLanguageMetadataID", id));
                }
                if (identity.ContentCultureGuid.TryGetValue(out var guid))
                {
                    query += $" and ContentItemLanguageMetadataGUID = @ContentItemLanguageMetadataGUID";
                    queryParams.Add(new DataParameter("@ContentItemLanguageMetadataGUID", guid));
                }
                if (identity.ContentCultureLookup.TryGetValue(out var contentCultureLookupForQuery))
                {
                    query += $" and ContentItemLanguageMetadataContentItemID = @ContentItemLanguageMetadataContentItemID";
                    queryParams.Add(new DataParameter("@ContentItemLanguageMetadataContentItemID", contentCultureLookupForQuery.ContentId));
                    cultureToCheck = contentCultureLookupForQuery.Culture.GetValueOrDefault(requestedCulture);
                }

                var items = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, queryParams, QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                switch (items.Count())
                {
                    case 0:
                        return Result.Failure<ContentCultureIdentity>("Could not find a content culture item with the given content culture identity");
                    case 1:
                        var firstRow = items.First();
                        return new ContentCultureIdentity()
                        {
                            ContentCultureID = (int)firstRow["DocumentID"],
                            ContentCultureGuid = (Guid)firstRow["DocumentGuid"],
                            ContentCultureLookup = new ContentCulture((int)firstRow["ContentItemLanguageMetadataContentItemID"], (string)firstRow["ContentLanguageName"])
                        };
                    default:
                        // multiple, must have been lookup on the ContentID, convert to dictionary by the language name
                        var itemsByLang = items.ToDictionary(key => ((string)key["ContentLanguageName"]).ToLowerInvariant(), value => new ContentCultureIdentity()
                        {
                            ContentCultureID = (int)value["DocumentID"],
                            ContentCultureGuid = (Guid)value["DocumentGuid"],
                            ContentCultureLookup = new ContentCulture((int)value["ContentItemLanguageMetadataContentItemID"], (string)value["ContentLanguageName"])
                        });

                        Maybe<string> lookupCulture = requestedCulture;

                        while (lookupCulture.HasValue)
                        {
                            if (itemsByLang.TryGetValue(lookupCulture.Value.ToLowerInvariant(), out var cultureMatch))
                            {
                                return cultureMatch;
                            }
                            lookupCulture = cultureToFallback.TryGetValue(lookupCulture.Value.ToLowerInvariant(), out var fallbackValue) ? fallbackValue : Maybe<string>.None;
                        }
                        return Result.Failure<ContentCultureIdentity>("Could not find content culture identity in the language given or any valid fallback");
                }
            }
        }

        public async Task<Result<TreeIdentity>> HydrateTreeIdentity(TreeIdentity identity)
        {
            if (identity.PageID.HasValue && identity.PageGuid.HasValue && identity.PageName.HasValue && identity.PathChannelLookup.TryGetValue(out var pathChannel) && pathChannel.ChannelId.HasValue)
            {
                return identity;
            }
            if (identity.PageID.HasNoValue && identity.PageGuid.HasNoValue && identity.PageName.HasNoValue && identity.PathChannelLookup.HasNoValue)
            {
                return Result.Failure<TreeIdentity>("No data given, can't hydrate");
            }

            var currentChannelID = _websiteChannelContext.WebsiteChannelID;

            var dictionaryResult = await GetContentIdentityHolder();
            if (dictionaryResult.TryGetValue(out var dictionary))
            {
                if (identity.PageID.TryGetValue(out var pageID) && dictionary.Tree.ById.TryGetValue(pageID, out var newIdentityFromId))
                {
                    return newIdentityFromId;
                }
                if (identity.PageGuid.TryGetValue(out var pageGuid) && dictionary.Tree.ByGuid.TryGetValue(pageGuid, out var newIdentityFromGuid))
                {
                    return newIdentityFromGuid;
                }
                if (identity.PageName.TryGetValue(out var pageName) && dictionary.Tree.ByName.TryGetValue(pageName.ToLowerInvariant(), out var newIdentityFromName))
                {
                    return newIdentityFromName;
                }
                if (identity.PathChannelLookup.TryGetValue(out var pathChannelLookup) && dictionary.Tree.ByPathChannelIDKey.TryGetValue(new PathChannel(pathChannelLookup.Path, pathChannelLookup.ChannelId.GetValueOrDefault(currentChannelID)).GetCacheKey().ToLowerInvariant(), out var identityFromPathChannel))
                {
                    return identityFromPathChannel;
                }
                return Result.Failure<TreeIdentity>("Could not find TreeIdentity");
            }
            else
            {
                // Can't use cached version, generate manually
                var queryParams = new QueryDataParameters();
                var query = @"select WebPageItemID, WebPageItemGUID, WebPageItemName, WebPageItemTreePath, WebPageItemWebsiteChannelID from CMS_WebPageItem where 1=1";
                if (identity.PageID.TryGetValue(out var id))
                {
                    query += $" and WebPageItemID = @WebPageItemID";
                    queryParams.Add(new DataParameter("@WebPageItemID", id));
                }
                if (identity.PageGuid.TryGetValue(out var guid))
                {
                    query += $" and WebPageItemGUID = @WebPageItemGUID";
                    queryParams.Add(new DataParameter("@WebPageItemGUID", guid));
                }
                if (identity.PageName.TryGetValue(out var pageName))
                {
                    query += $" and WebPageItemName = @WebPageItemName";
                    queryParams.Add(new DataParameter("@WebPageItemName", pageName));
                }
                if (identity.PathChannelLookup.TryGetValue(out var pathAndChannel))
                {
                    query += $" and WebPageItemTreePath = @WebPageItemTreePath and WebPageItemWebsiteChannelID = @WebPageItemWebsiteChannelID";
                    queryParams.Add(new DataParameter("@WebPageItemTreePath", pathAndChannel.Path));
                    queryParams.Add(new DataParameter("@WebPageItemWebsiteChannelID", pathAndChannel.ChannelId.GetValueOrDefault(currentChannelID)));
                }
                var item = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, queryParams, QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                if (item.FirstOrMaybe().TryGetValue(out var docRow))
                {
                    return new TreeIdentity()
                    {
                        PageID = (int)docRow["WebPageItemID"],
                        PageGuid = (Guid)docRow["WebPageItemGUID"],
                        PageName = (string)docRow["WebPageItemName"],
                        PathChannelLookup = new PathChannel(Path: (string)docRow["WebPageItemTreePath"], ChannelId: (int)docRow["WebPageItemWebsiteChannelID"])
                    };
                }
                else
                {
                    return Result.Failure<TreeIdentity>("Could not find a web page item with the given tree identity");
                }
            }


        }

        public async Task<Result<TreeCultureIdentity>> HydrateTreeCultureIdentity(TreeCultureIdentity identity) => (await HydrateTreeIdentity(identity)).TryGetValue(out var value, out var error) ? new TreeCultureIdentity(identity.Culture)
        {
            PathChannelLookup = value.PathChannelLookup,
            PageID = value.PageID,
            PageGuid = value.PageGuid,
            PageName = value.PageName
        } : Result.Failure<TreeCultureIdentity>(error);

        #endregion

        #region "Content Class Lookups"

        public Task<Result<string>> GetContentType(ContentIdentity identity) => _contentTypeRetriever.GetContentType(identity);

        public Task<Result<string>> GetContentType(ContentCultureIdentity identity) => _contentTypeRetriever.GetContentType(identity);

        public Task<Result<string>> GetContentType(TreeIdentity identity) => _contentTypeRetriever.GetContentType(identity);

        public Task<Result<string>> GetContentType(TreeCultureIdentity identity) => _contentTypeRetriever.GetContentType(identity);

        #endregion

        #region "Object Hydration"

        public async Task<Result<ObjectIdentity>> HydrateObjectIdentity(ObjectIdentity identity, string className)
        {
            if (identity.Id.HasNoValue && identity.Guid.HasNoValue && identity.CodeName.HasNoValue)
            {
                return Result.Failure<ObjectIdentity>("No identities provided from the given identity, cannot parse.");
            }

            var classInfo = _progressiveCache.Load(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = _cacheDependencyBuilderFactory.Create(false).ObjectType(DataClassInfo.OBJECT_TYPE).GetCMSCacheDependency();
                }
                return DataClassInfoProvider.GetDataClassInfo(className);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetClassInfoForHydration", className));

            if (classInfo == null)
            {
                return Result.Failure<ObjectIdentity>($"Class of {className} not found.");
            }

            var classObj = new InfoObjectFactory(className).Singleton;
            if (classObj is not BaseInfo baseClassObj)
            {
                return Result.Failure<ObjectIdentity>($"Class of {className} not a BaseInfo typed class, cannot parse.");
            }

            var dictionaryResult = await GetObjectIdentities(classInfo, baseClassObj);
            if (dictionaryResult.TryGetValue(out var dictionary))
            {
                if (identity.Id.TryGetValue(out var idVal) && dictionary.ById.TryGetValue(idVal, out var objectIdentityFromId))
                {
                    return objectIdentityFromId;
                }
                if (identity.CodeName.TryGetValue(out var codeNameVal) && dictionary.ByCodeName.TryGetValue(codeNameVal.ToLower(), out var objectIdentitFromCodeName))
                {
                    return objectIdentitFromCodeName;
                }
                if (identity.Guid.TryGetValue(out var guidVal) && dictionary.ByGuid.TryGetValue(guidVal, out var objectIdentityFromGuid))
                {
                    return objectIdentityFromGuid;
                }
                return Result.Failure<ObjectIdentity>($"Could not find any matching {className} objects for any of the identity's values:  [{identity.Id.GetValueOrDefault(0)}-{identity.CodeName.GetValueOrDefault(string.Empty)}-{identity.Guid.GetValueOrDefault(Guid.Empty)}]");
            }
            else
            {
                // Dependencies are touching too much, use non cached version
                string query = GetObjectIdentitySelectStatement(classInfo, baseClassObj);
                var typeInfo = baseClassObj.TypeInfo;
                var idColumnMaybe = typeInfo.IDColumn.AsNullOrWhitespaceMaybe();
                var guidColumnMaybe = typeInfo.GUIDColumn.AsNullOrWhitespaceMaybe();
                var codeNameColumnMaybe = typeInfo.CodeNameColumn.AsNullOrWhitespaceMaybe();
                var queryParams = new QueryDataParameters();
                if (identity.Id.TryGetValue(out var id) && idColumnMaybe.TryGetValue(out var idColumn))
                {
                    query += $"[{idColumn}] = @ID";
                    queryParams.Add(new DataParameter("@ID", id));
                }
                if (identity.Guid.TryGetValue(out var guid) && guidColumnMaybe.TryGetValue(out var guidColumn))
                {
                    query += $"[{guidColumn}] = @Guid";
                    queryParams.Add(new DataParameter("@Guid", guid));
                }
                if (identity.CodeName.TryGetValue(out var codeName) && codeNameColumnMaybe.TryGetValue(out var codeNameColumn))
                {
                    query += $"[{codeNameColumn}] = @CodeName";
                    queryParams.Add(new DataParameter("@CodeName", codeName));
                }
                var item = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, queryParams, QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                if (item.FirstOrMaybe().TryGetValue(out var objRow))
                {
                    Maybe<int> idMaybe = Maybe.None;
                    Maybe<Guid> guidMaybe = Maybe.None;
                    Maybe<string> codeNameMaybe = Maybe.None;
                    if (idColumnMaybe.TryGetValue(out var idColumnVal))
                    {
                        idMaybe = ValidationHelper.GetInteger(objRow[idColumnVal], 0).WithMatchAsNone(0);
                    }
                    if (guidColumnMaybe.TryGetValue(out var guidColumnVal))
                    {
                        guidMaybe = ValidationHelper.GetGuid(objRow[guidColumnVal], Guid.Empty).WithMatchAsNone(Guid.Empty);
                    }
                    if (codeNameColumnMaybe.TryGetValue(out var codeNameColumnVal))
                    {
                        codeNameMaybe = ValidationHelper.GetString(objRow[codeNameColumnVal], string.Empty).WithMatchAsNone(string.Empty);
                    }

                    var newIdentity = new ObjectIdentity()
                    {
                        Id = idMaybe,
                        Guid = guidMaybe,
                        CodeName = codeNameMaybe
                    };
                    return newIdentity;
                }
                else
                {
                    return Result.Failure<ObjectIdentity>($"Could not find an object of type {className} with the given identity [{identity.Id.GetValueOrDefault(0)}-{identity.CodeName.GetValueOrDefault(string.Empty)}-{identity.Guid.GetValueOrDefault(Guid.Empty)}]");
                }
            }
        }

        #endregion

        #region "Object Dictionary Builders / Helpers"

        private static string GetObjectIdentitySelectStatement(DataClassInfo classInfo, BaseInfo baseClassObj)
        {
            var typeInfo = baseClassObj.TypeInfo;
            return $"select {typeInfo.IDColumn} {(typeInfo.GUIDColumn.AsNullOrWhitespaceMaybe().TryGetValue(out var guidColumn) ? $", [{guidColumn}]" : "")} {(typeInfo.CodeNameColumn.AsNullOrWhitespaceMaybe().TryGetValue(out var codenameColumn) ? $", [{codenameColumn}]" : "")} from {classInfo.ClassTableName}";
        }

        private async Task<Result<ObjectIdentityDictionaries>> GetObjectIdentities(DataClassInfo classInfo, BaseInfo baseClassObj)
        {
            var builder = _cacheDependencyBuilderFactory.Create(false).ObjectType(baseClassObj.TypeInfo.ObjectClassName);
            if (!builder.DependenciesNotTouchedSince(TimeSpan.FromSeconds(30)))
            {
                return Result.Failure<ObjectIdentityDictionaries>("Dependency recently touched, waiting 30 seconds before using cached version.");
            }

            return await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var allItems = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(GetObjectIdentitySelectStatement(classInfo, baseClassObj), [], QueryTypeEnum.SQLQuery))
                .Tables[0].Rows.Cast<DataRow>();

                var byId = new Dictionary<int, ObjectIdentity>();
                var byGuid = new Dictionary<Guid, ObjectIdentity>();
                var byName = new Dictionary<string, ObjectIdentity>();
                foreach (DataRow item in allItems)
                {
                    // Create
                    Maybe<Guid> guidMaybe = Maybe.None;
                    Maybe<string> codeNameMaybe = Maybe.None;
                    if (baseClassObj.TypeInfo.GUIDColumn.AsNullOrWhitespaceMaybe().TryGetValue(out var guidColumn))
                    {
                        guidMaybe = ValidationHelper.GetGuid(item[guidColumn], Guid.Empty).WithMatchAsNone(Guid.Empty);
                    }
                    if (baseClassObj.TypeInfo.CodeNameColumn.AsNullOrWhitespaceMaybe().TryGetValue(out var codeNameColumn))
                    {
                        codeNameMaybe = ValidationHelper.GetString(item[codeNameColumn], string.Empty).WithMatchAsNone(string.Empty);
                    }

                    var objectIdentity = new ObjectIdentity()
                    {
                        Id = (int)item[baseClassObj.TypeInfo.IDColumn],
                        Guid = guidMaybe,
                        CodeName = codeNameMaybe
                    };

                    // Add
                    if (objectIdentity.Id.TryGetValue(out var idVal))
                    {
                        byId.TryAdd(idVal, objectIdentity);
                    }
                    if (objectIdentity.Guid.TryGetValue(out var guidVal))
                    {
                        byGuid.TryAdd(guidVal, objectIdentity);
                    }
                    if (objectIdentity.CodeName.TryGetValue(out var codeNameVal))
                    {
                        byName.TryAdd(codeNameVal.ToLowerInvariant(), objectIdentity);
                    }
                }
                return new ObjectIdentityDictionaries(
                    byId: byId,
                    byCodeName: byName,
                    byGuid: byGuid
                    );

            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetAllObjectIdentity", baseClassObj.TypeInfo.ObjectClassName));

        }

        #endregion

        #region "Content Dictionary Builders / Helpers"

        /// <summary>
        /// Highly optimized Identity retrieval query
        /// </summary>
        /// <returns></returns>
        private async Task<Result<ContentIdentityBaseDataDictionary>> GetContentIdentityBaseDataDictionary()
        {
            var builder = _cacheDependencyBuilderFactory.Create(false).AddKey("contentitem|all");

            if (!builder.DependenciesNotTouchedSince(TimeSpan.FromSeconds(30)))
            {
                return Result.Failure<ContentIdentityBaseDataDictionary>("Dependency recently touched, waiting 30 seconds before using cached version.");
            }

            return await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"contentitem|all");
                }

                // To optimize, performing 3 queries in a single call, these are collecting only the essential information, no duplicates of data except the ContentItemID references
                var query = @$"select {nameof(ContentItemInfo.ContentItemID)}, {nameof(ContentItemInfo.ContentItemGUID)}, {nameof(ContentItemInfo.ContentItemName)} from CMS_ContentItem order by {nameof(ContentItemInfo.ContentItemID)} 
select {nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID)}, {nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataID)}, {nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataGUID)}, {nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID)}  from CMS_ContentItemLanguageMetadata order by {nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID)} 
select WebPageItemContentItemID, WebPageItemID, WebPageItemGUID, WebPageItemName, WebPageItemTreePath, WebPageItemWebsiteChannelID from CMS_WebPageItem order by WebPageItemContentItemID";
                var dataSet = await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery);

                var contentBaseData = dataSet.Tables[0].Rows.Cast<DataRow>()
                    .ToDictionary(
                        key => (int)key[nameof(ContentItemInfo.ContentItemID)],
                        value => new ContentBaseData(
                            Id: (int)value[nameof(ContentItemInfo.ContentItemID)],
                            Guid: (Guid)value[nameof(ContentItemInfo.ContentItemGUID)],
                            Name: (string)value[nameof(ContentItemInfo.ContentItemName)]
                            )
                        );

                var contentLanguageBase = dataSet.Tables[1].Rows.Cast<DataRow>()
                   .GroupBy(
                       key => (int)key[nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentItemID)])
                   .ToDictionary(key => key.Key,
                       value => value.Select(x => new ContentLanguageBaseData(
                           Id: (int)x[nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataID)],
                           Guid: (Guid)x[nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataGUID)],
                           LanguageId: (int)x[nameof(ContentItemLanguageMetadataInfo.ContentItemLanguageMetadataContentLanguageID)]
                           )
                       )
                   );

                var webPageBase = dataSet.Tables[2].Rows.Cast<DataRow>()
                   .ToDictionary(
                       key => (int)key["WebPageItemContentItemID"],
                       value => new WebPageBaseData(
                           Id: (int)value["WebPageItemID"],
                           Guid: (Guid)value["WebPageItemGUID"],
                           Name: (string)value["WebPageItemName"],
                           Path: (string)value["WebPageItemTreePath"],
                           ChannelId: (int)value["WebPageItemWebsiteChannelID"]
                           )
                       );

                return new ContentIdentityBaseDataDictionary(
                    contentItemsById: contentBaseData,
                    contentItemLanguageMetadataByContentItemID: contentLanguageBase,
                    webPageItemByContentItemID: webPageBase
                );

            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "GetContentIdentityBaseDataDictionary"));
        }

        private async Task<Result<ContentIdentityHolder>> GetContentIdentityHolder()
        {
            // Only build if the data is not changing frequently.
            var allContentResult = await GetContentIdentityBaseDataDictionary();
            if (!allContentResult.TryGetValue(out var allContent, out var error))
            {
                return Result.Failure<ContentIdentityHolder>(error);
            }

            // Get Lang and channel lookups
            var channelIdToCulture = await ChannelToDefaultCulture();
            var languageIdToCulture = await GetLanguageNameById();

            return _progressiveCache.Load(cs =>
            {
                var builder = _cacheDependencyBuilderFactory.Create(false)
                    .AddKey("contentitem|all");

                if (cs.Cached)
                {
                    cs.CacheDependency = builder
                        .GetCMSCacheDependency();
                }

                var treeIdentityById = new Dictionary<int, TreeIdentity>();
                var treeIdentityByGuid = new Dictionary<Guid, TreeIdentity>();
                var treeIdentityByName = new Dictionary<string, TreeIdentity>();
                var treeIdentityByPathChannelIDKey = new Dictionary<string, TreeIdentity>();

                // Won't have a TreeCultureIdentity dictionary as the TreeCultureIdentity is simply the normal culture agnostic TreeIdentity with the culture passed on, which is required.
                // Not doing logic here to determine if an item EXISTS by those parameters, filling in the ID, Guid, or Path if missing, which again are language agnostic

                var contentItemById = new Dictionary<int, ContentIdentity>();
                var contentItemByName = new Dictionary<string, ContentIdentity>();
                var contentItemByGuid = new Dictionary<Guid, ContentIdentity>();

                var contentItemCultureById = new Dictionary<int, ContentCultureIdentity>();
                var contentItemCultureByGuid = new Dictionary<Guid, ContentCultureIdentity>();
                var contentItemCultureByContentIDAndCulture = new Dictionary<int, Dictionary<string, ContentCultureIdentity>>();

                var contentItemsById = allContent.ContentItemsById;
                var contentLanguageMetadataByContentItemId = allContent.ContentItemLanguageMetadataByContentItemID;
                var webPageItemByContentItemID = allContent.WebPageItemByContentItemID;

                foreach (var contentItemID in contentItemsById.Keys)
                {
                    var contentItem = contentItemsById[contentItemID];
                    var contentIdentity = new ContentIdentity()
                    {
                        ContentID = contentItem.Id,
                        ContentGuid = contentItem.Guid,
                        ContentName = contentItem.Name
                    };
                    try
                    {
                        contentItemById.Add(contentItem.Id, contentIdentity);
                        contentItemByGuid.Add(contentItem.Guid, contentIdentity);
                        contentItemByName.Add(contentItem.Name.ToLowerInvariant(), contentIdentity);
                    }
                    catch (Exception)
                    {
                        // should never hit this but just in case
                    }


                    // now handle language variants

                    if (contentLanguageMetadataByContentItemId.TryGetValue(contentItemID, out var contentLanguageMetadataByContentItem))
                    {
                        var languageToCultureIdentity = new Dictionary<string, ContentCultureIdentity>();
                        foreach (var contentLanguageMetadata in contentLanguageMetadataByContentItem)
                        {
                            var cultureLookup = new ContentCulture(contentItem.Id, languageIdToCulture[contentLanguageMetadata.LanguageId]);
                            var contentCultureIdentity = new ContentCultureIdentity()
                            {
                                ContentCultureID = contentLanguageMetadata.Id,
                                ContentCultureGuid = contentLanguageMetadata.Guid,
                                ContentCultureLookup = cultureLookup
                            };

                            try
                            {
                                contentItemCultureById.Add(contentLanguageMetadata.Id, contentCultureIdentity);
                                contentItemCultureByGuid.Add(contentLanguageMetadata.Guid, contentCultureIdentity);
                                languageToCultureIdentity.Add(languageIdToCulture[contentLanguageMetadata.LanguageId].ToLowerInvariant(), contentCultureIdentity);
                            }
                            catch (Exception)
                            {
                                // should never hit this, but just in case
                            }
                        }
                        contentItemCultureByContentIDAndCulture.Add(contentItem.Id, languageToCultureIdentity);
                    }

                    // Now web page items
                    if (webPageItemByContentItemID.TryGetValue(contentItemID, out var webPageBaseData))
                    {
                        var pathChannelLookup = new PathChannel(webPageBaseData.Path, webPageBaseData.ChannelId);
                        var treeIdentity = new TreeIdentity()
                        {
                            PageID = webPageBaseData.Id,
                            PageGuid = webPageBaseData.Guid,
                            PageName = webPageBaseData.Name,
                            PathChannelLookup = pathChannelLookup
                        };
                        try
                        {
                            treeIdentityById.Add(webPageBaseData.Id, treeIdentity);
                            treeIdentityByGuid.Add(webPageBaseData.Guid, treeIdentity);
                            treeIdentityByName.Add(webPageBaseData.Name, treeIdentity);
                            treeIdentityByPathChannelIDKey.Add(pathChannelLookup.GetCacheKey().ToLowerInvariant(), treeIdentity);
                        }
                        catch (Exception)
                        {
                            // Should never hit this, but just in case.
                        }
                    }
                }


                return new ContentIdentityHolder(
                    content: new ContentIdentityDictionaries(contentItemById, contentItemByName, contentItemByGuid),
                    contentCulture: new ContentCultureIdentityDictionaries(contentItemCultureById, contentItemCultureByGuid, contentItemCultureByContentIDAndCulture),
                    tree: new TreeIdentityDictionaries(treeIdentityById, treeIdentityByGuid, treeIdentityByName, treeIdentityByPathChannelIDKey)
                    );
            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "GetContentIdentityHolder"));

        }

        #endregion

        

        #region "Helper Dictionary Builders"

        private async Task<Dictionary<int, string>> ChannelToDefaultCulture()
        {
            var builder = _cacheDependencyBuilderFactory.Create()
                .ObjectType(ChannelInfo.OBJECT_TYPE);

            return await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var query = $@"SELECT WebsiteChannelChannelID as ChannelID, ContentLanguageName FROM CMS_WebsiteChannel
  inner join CMS_ContentLanguage on WebsiteChannelPrimaryContentLanguageID = ContentLanguageID
union all select ChannelID, ContentLanguageName from CMS_Channel
inner join CMS_ContentLanguage on ContentLanguageIsDefault = 1
where ChannelID not in (Select WebsiteChannelChannelID from CMS_WebsiteChannel)";
                return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>().ToDictionary(key => (int)key["ChannelID"], value => (string)value[nameof(ContentLanguageInfo.ContentLanguageName)]);

            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "ChannelToDefaultCulture"));
        }

        private async Task<Dictionary<int, string>> GetLanguageNameById()
        {
            return await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{ContentLanguageInfo.OBJECT_TYPE}|all");
                }
                return (await _contentLanguageInfoProvider.Get()
                .Columns(nameof(ContentLanguageInfo.ContentLanguageID), nameof(ContentLanguageInfo.ContentLanguageName))
                .GetEnumerableTypedResultAsync()).ToDictionary(key => key.ContentLanguageID, value => value.ContentLanguageName.ToLower());
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetLanguageNameById"));
        }

        private async Task<Dictionary<string, Maybe<string>>> GetCultureToFallbackDictionary()
        {
            return await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{ContentLanguageInfo.OBJECT_TYPE}|all");
                }
                var query = @$"select main.ContentLanguageName as ContentLanguageName, fallback.ContentLanguageName as Fallback from CMS_ContentLanguage main 
                left join CMS_ContentLanguage fallback on main.ContentLanguageFallbackContentLanguageID = fallback.ContentLanguageID";
                return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>()
                .ToDictionary(key => ((string)key["ContentLanguageName"]).ToLower(), value => DataHelper.IsEmpty(value["Fallback"]) ? Maybe<string>.None : ((string)value["Fallback"]).ToLower());

            }, new CacheSettings(1440, "GetCultureToFallbackDictionary"));
        }

        #endregion

        #region "Dictionaries"


        /// <summary>
        /// Using Classes vs. records as we want these to be cached as an individual item vs. a value
        /// </summary>

        private class ContentIdentityBaseDataDictionary(
  Dictionary<int, ContentBaseData> contentItemsById,
  Dictionary<int, IEnumerable<ContentLanguageBaseData>> contentItemLanguageMetadataByContentItemID,
  Dictionary<int, WebPageBaseData> webPageItemByContentItemID)
        {
            public Dictionary<int, ContentBaseData> ContentItemsById { get; } = contentItemsById;
            public Dictionary<int, IEnumerable<ContentLanguageBaseData>> ContentItemLanguageMetadataByContentItemID { get; } = contentItemLanguageMetadataByContentItemID;
            public Dictionary<int, WebPageBaseData> WebPageItemByContentItemID { get; } = webPageItemByContentItemID;
        }

        private class TreeIdentityDictionaries(Dictionary<int, TreeIdentity> byId, Dictionary<Guid, TreeIdentity> byGuid, Dictionary<string, TreeIdentity> byName, Dictionary<string, TreeIdentity> byPathChannelIDKey)
        {
            public Dictionary<int, TreeIdentity> ById { get; internal set; } = byId;
            public Dictionary<Guid, TreeIdentity> ByGuid { get; internal set; } = byGuid;
            public Dictionary<string, TreeIdentity> ByName { get; internal set; } = byName;
            public Dictionary<string, TreeIdentity> ByPathChannelIDKey { get; internal set; } = byPathChannelIDKey;
        }

        private class ContentIdentityDictionaries(Dictionary<int, ContentIdentity> byId, Dictionary<string, ContentIdentity> byName, Dictionary<Guid, ContentIdentity> byGuid)
        {
            public Dictionary<int, ContentIdentity> ById { get; internal set; } = byId;
            public Dictionary<string, ContentIdentity> ByName { get; internal set; } = byName;
            public Dictionary<Guid, ContentIdentity> ByGuid { get; internal set; } = byGuid;
        }

        private class ContentCultureIdentityDictionaries(Dictionary<int, ContentCultureIdentity> byId, Dictionary<Guid, ContentCultureIdentity> byGuid, Dictionary<int, Dictionary<string, ContentCultureIdentity>> byContentIDAndCulture)
        {
            public Dictionary<int, ContentCultureIdentity> ById { get; internal set; } = byId;
            public Dictionary<Guid, ContentCultureIdentity> ByGuid { get; internal set; } = byGuid;
            public Dictionary<int, Dictionary<string, ContentCultureIdentity>> ByContentIDAndCulture { get; internal set; } = byContentIDAndCulture;

        }

        private class ObjectIdentityDictionaries(Dictionary<int, ObjectIdentity> byId, Dictionary<string, ObjectIdentity> byCodeName, Dictionary<Guid, ObjectIdentity> byGuid)
        {
            public Dictionary<int, ObjectIdentity> ById { get; internal set; } = byId;
            public Dictionary<string, ObjectIdentity> ByCodeName { get; internal set; } = byCodeName;
            public Dictionary<Guid, ObjectIdentity> ByGuid { get; internal set; } = byGuid;
        }

        private class ContentIdentityHolder(ContentIdentityDictionaries content, ContentCultureIdentityDictionaries contentCulture, TreeIdentityDictionaries tree)
        {
            public ContentIdentityDictionaries Content { get; } = content;
            public ContentCultureIdentityDictionaries ContentCulture { get; } = contentCulture;
            public TreeIdentityDictionaries Tree { get; } = tree;
        }

        #endregion

        #region "Internal records"

        private record ContentBaseData(int Id, Guid Guid, string Name);
        private record ContentLanguageBaseData(int Id, Guid Guid, int LanguageId);
        private record WebPageBaseData(int Id, Guid Guid, string Name, string Path, int ChannelId);

        #endregion

        #region "Obsolete Methods"

#pragma warning disable CS0618 // Type or member is obsolete
        public Task<Result<DocumentIdentity>> HydrateDocumentIdentity(DocumentIdentity identity)
        {
            throw new NotImplementedException("This is not implemented in XbyK, only there for KX13 cross compatability");
        }

        public Task<Result<NodeIdentity>> HydrateNodeIdentity(NodeIdentity identity)
        {
            throw new NotImplementedException("This is not implemented in XbyK, only there for KX13 cross compatability");
        }
#pragma warning restore CS0618 // Type or member is obsolete

        #endregion
    }

}
