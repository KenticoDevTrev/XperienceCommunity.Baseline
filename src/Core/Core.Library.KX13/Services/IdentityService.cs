using CMS.Base;
using CMS.DataEngine;
using CMS.SiteProvider;
using System.Data;

namespace Core.Services.Implementations
{
    public class IdentityService(
        IProgressiveCache _progressiveCache,
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory,
        ISiteService _siteService,
        ICacheRepositoryContext _cacheRepositoryContext,
        ISiteInfoProvider _siteInfoProvider) : IIdentityService
    {
        public async Task<Result<TreeIdentity>> HydrateTreeIdentity(TreeIdentity identity)
        {
            // If current identity is full, then just return it.
            if (identity.PageGuid.HasValue && identity.PageID.HasValue && identity.PathChannelLookup.TryGetValue(out var pathChannel) && pathChannel.ChannelId.HasValue)
            {
                return identity;
            }

            var currentSiteID = _siteService.CurrentSite.SiteID;

            var dictionaryResult = await GetTreeIdentityHolderAsync();
            if (dictionaryResult.TryGetValue(out var dictionary))
            {
                if (identity.PageID.TryGetValue(out var id) && dictionary.Tree.ById.TryGetValue(id, out var newIdentityFromId))
                {
                    return newIdentityFromId;
                }
                if (identity.PageGuid.TryGetValue(out var guid) && dictionary.Tree.ByGuid.TryGetValue(guid, out var newIdentityFromGuid))
                {
                    return newIdentityFromGuid;
                }
                if (identity.PathChannelLookup.TryGetValue(out var pathAndChannelValues))
                {
                    string key = $"{pathAndChannelValues.Path}|{pathAndChannelValues.ChannelId.GetValueOrDefault(currentSiteID)}".ToLower();
                    if (dictionary.Tree.ByPathChannelIDKey.TryGetValue(key, out var newIdentityFromPath))
                    {
                        return newIdentityFromPath;
                    }
                }
                return Result.Failure<TreeIdentity>("Could not find document identity.");
            }
            else
            {
                // Can't use cached version, so just generate manually
                var queryParams = new QueryDataParameters();
                var query = $"select NodeID, NodeGUID, NodeAliasPath, NodeSiteID from View_CMS_Tree_Joined where ";
                if (identity.PageID.TryGetValue(out var id))
                {
                    query += $"NodeID = @NodeID";
                    queryParams.Add(new DataParameter("@NodeID", id));
                }
                if (identity.PageGuid.TryGetValue(out var guid))
                {
                    query += $"NodeGuid = @NodeGuid";
                    queryParams.Add(new DataParameter("@NodeGuid", guid));
                }
                if (identity.PathChannelLookup.TryGetValue(out var pathAndChannel))
                {
                    query += $"NodeAliasPath = @NodeAliasPath and NodeSiteID = @NodeSiteID";
                    queryParams.Add(new DataParameter("@NodeAliasPath", pathAndChannel.Path));
                    queryParams.Add(new DataParameter("@NodeSiteID", pathAndChannel.ChannelId.GetValueOrDefault(currentSiteID)));
                }
                var item = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, queryParams, QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                if (item.FirstOrMaybe().TryGetValue(out var docRow))
                {
                    return new TreeIdentity()
                    {
                        PageID = (int)docRow["NodeID"],
                        PageGuid = (Guid)docRow["NodeGUID"],
                        PathChannelLookup = new PathChannel(Path: (string)docRow["NodeAliasPath"], ChannelId: (int)docRow["NodeSiteID"])
                    };
                }
                else
                {
                    return Result.Failure<TreeIdentity>("Could not find a node with the given tree identity");
                }
            }
        }

        public async Task<Result<TreeCultureIdentity>> HydrateTreeCultureIdentity(TreeCultureIdentity identity)
        {
            // If current identity is full, then just return it.
            if (identity.PageGuid.HasValue && identity.PageID.HasValue && identity.PathChannelLookup.TryGetValue(out var nodepathculture) && nodepathculture.ChannelId.HasValue)
            {
                return identity;
            }
            var currentSiteID = _siteService.CurrentSite.SiteID;

            // Site settings are already cached in the Provider
            var channelIdToCulture = await SiteToDefaultCulture();
            var defaultCulture = channelIdToCulture[currentSiteID];
            var culture = _cacheRepositoryContext.CurrentCulture();

            var dictionaryResult = await GetTreeIdentityHolderAsync();
            if (dictionaryResult.TryGetValue(out var dictionary))
            {
                if (identity.PageID.TryGetValue(out var id) && dictionary.TreeCulture.ById.TryGetValue(id, out var newIdentityFromId))
                {
                    return newIdentityFromId.OrderBy(x => x.Culture.Equals(identity.Culture, StringComparison.OrdinalIgnoreCase) ? 0 : 1).First();
                }
                if (identity.PageGuid.TryGetValue(out var guid) && dictionary.TreeCulture.ByGuid.TryGetValue(guid, out var newIdentityFromGuid))
                {
                    return newIdentityFromGuid.OrderBy(x => x.Culture.Equals(identity.Culture, StringComparison.OrdinalIgnoreCase) ? 0 : 1).First();
                }
                if (identity.PathChannelLookup.TryGetValue(out var pathChannelValue))
                {
                    string key = $"{pathChannelValue.Path}|{pathChannelValue.ChannelId.GetValueOrDefault(currentSiteID)}".ToLower();
                    if (dictionary.TreeCulture.ByPathChannelIDKey.TryGetValue(key, out var newIdentityFromPath))
                    {
                        return newIdentityFromPath.OrderBy(x => x.Culture.Equals(identity.Culture, StringComparison.OrdinalIgnoreCase) ? 0 : 1).First();
                    }
                }
                return Result.Failure<TreeCultureIdentity>("Could not find tree culture identity.");
            }
            else
            {
                // Can't use cached version, so just generate manually
                var queryParams = new QueryDataParameters();
                var query = $"select NodeID, NodeGuid, NodeAliasPath, NodeSiteID, DocumentCulture, DocumentID, DocumentGUID from View_CMS_Tree_Joined where ";
                if (identity.PageID.TryGetValue(out var id))
                {
                    query += $"NodeID = @NodeID";
                    queryParams.Add(new DataParameter("@NodeID", id));
                }
                if (identity.PageGuid.TryGetValue(out var guid))
                {
                    query += $"NodeGuid = @NodeGuid";
                    queryParams.Add(new DataParameter("@NodeGuid", guid));
                }
                if (identity.PathChannelLookup.TryGetValue(out var pathChannelValue))
                {
                    // for documents, culture should be a preferred otherwise use current site default or first found
                    query += $"NodeAliasPath = @NodeAliasPath and NodeSiteID = @NodeSiteID order by case when DocumentCulture = @DocumentCulture then 0 else case when DocumentCulture = @DefaultCulture then 1 else 2 end end";
                    queryParams.Add(new DataParameter("@NodeAliasPath", pathChannelValue.Path));
                    queryParams.Add(new DataParameter("@DocumentCulture", identity.Culture));
                    queryParams.Add(new DataParameter("@DefaultCulture", defaultCulture));
                    queryParams.Add(new DataParameter("@NodeSiteID", pathChannelValue.ChannelId.GetValueOrDefault(currentSiteID)));
                }
                var item = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, queryParams, QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                if (item.FirstOrMaybe().TryGetValue(out var docRow))
                {
                    return new TreeCultureIdentity((string)docRow["DocumentCulture"])
                    {
                        PageID = (int)docRow["NodeID"],
                        PageGuid = (Guid)docRow["NodeGUID"],
                        PathChannelLookup = new PathChannel(Path: (string)docRow["NodeAliasPath"], ChannelId: (int)docRow["NodeSiteID"])
                    };
                }
                else
                {
                    return Result.Failure<TreeCultureIdentity>("Could not find a document with the given tree culture identity");
                }
            }
        }

        public async Task<Result<ContentIdentity>> HydrateContentIdentity(ContentIdentity identity)
        {
            // If current identity is full, then just return it.
            if (identity.ContentID.HasValue && identity.ContentID.HasValue && identity.PathChannelLookup.TryGetValue(out var pathChannel) && pathChannel.ChannelId.HasValue)
            {
                return identity;
            }

            var currentSiteID = _siteService.CurrentSite.SiteID;

            var dictionaryResult = await GetContentIdentityHolderAsync();
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
                if (identity.PathChannelLookup.TryGetValue(out var pathAndChannelValues))
                {
                    string key = $"{pathAndChannelValues.Path}|{pathAndChannelValues.ChannelId.GetValueOrDefault(currentSiteID)}".ToLower();
                    if (dictionary.Content.ByPathChannelIDKey.TryGetValue(key, out var newIdentityFromPath))
                    {
                        return newIdentityFromPath;
                    }
                }
                return Result.Failure<ContentIdentity>("Could not find content identity.");
            }
            else
            {
                // Can't use cached version, so just generate manually
                var queryParams = new QueryDataParameters();
                var query = $"select NodeID, NodeGUID, NodeAliasPath, NodeSiteID from View_CMS_Tree_Joined where ";
                if (identity.ContentID.TryGetValue(out var id))
                {
                    query += $"NodeID = @NodeID";
                    queryParams.Add(new DataParameter("@NodeID", id));
                }
                if (identity.ContentGuid.TryGetValue(out var guid))
                {
                    query += $"NodeGuid = @NodeGuid";
                    queryParams.Add(new DataParameter("@NodeGuid", guid));
                }
                if (identity.PathChannelLookup.TryGetValue(out var pathAndChannel))
                {
                    query += $"NodeAliasPath = @NodeAliasPath and NodeSiteID = @NodeSiteID";
                    queryParams.Add(new DataParameter("@NodeAliasPath", pathAndChannel.Path));
                    queryParams.Add(new DataParameter("@NodeSiteID", pathAndChannel.ChannelId.GetValueOrDefault(currentSiteID)));
                }
                var item = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, queryParams, QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                if (item.FirstOrMaybe().TryGetValue(out var docRow))
                {
                    return new ContentIdentity()
                    {
                        ContentID = (int)docRow["NodeID"],
                        ContentGuid = (Guid)docRow["NodeGUID"],
                        PathChannelLookup = new PathChannel(Path: (string)docRow["NodeAliasPath"], ChannelId: (int)docRow["NodeSiteID"])
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
            if (identity.ContentCultureID.HasValue && identity.ContentCultureID.HasValue && identity.PathCultureChannelLookup.TryGetValue(out var nodepathculture) && nodepathculture.ChannelId.HasValue && nodepathculture.Culture.HasValue)
            {
                return identity;
            }
            var currentSiteID = _siteService.CurrentSite.SiteID;

            // Site settings are already cached in the Provider
            var channelIdToCulture = await SiteToDefaultCulture();
            var defaultCulture = channelIdToCulture[currentSiteID];

            var culture = _cacheRepositoryContext.CurrentCulture();

            var dictionaryResult = await GetContentIdentityHolderAsync();
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
                if (identity.PathCultureChannelLookup.TryGetValue(out var pathCultureChannelValues))
                {
                    string key = $"{pathCultureChannelValues.Path}|{pathCultureChannelValues.Culture.GetValueOrDefault(culture)}|{pathCultureChannelValues.ChannelId.GetValueOrDefault(currentSiteID)}".ToLower();
                    if (dictionary.ContentCulture.ByPathChannelCultureIDKey.TryGetValue(key, out var newIdentityFromPath))
                    {
                        return newIdentityFromPath;
                    }

                    string keyCultureLess = $"{pathCultureChannelValues.Path}|{pathCultureChannelValues.ChannelId.GetValueOrDefault(currentSiteID)}".ToLower();
                    if (dictionary.ContentCulture.ByPathChannelIDKey.TryGetValue(keyCultureLess, out var newIdentityFromPathNoCulture))
                    {
                        return newIdentityFromPathNoCulture;
                    }
                }
                return Result.Failure<ContentCultureIdentity>("Could not find content culture identity.");
            }
            else
            {
                // Can't use cached version, so just generate manually
                var queryParams = new QueryDataParameters();
                var query = $"select NodeAliasPath, NodeSiteID, DocumentCulture, DocumentID, DocumentGUID from View_CMS_Tree_Joined where ";
                if (identity.ContentCultureID.TryGetValue(out var id))
                {
                    query += $"DocumentID = @DocumentID";
                    queryParams.Add(new DataParameter("@DocumentID", id));
                }
                if (identity.ContentCultureGuid.TryGetValue(out var guid))
                {
                    query += $"DocumentGuid = @DocumentGuid";
                    queryParams.Add(new DataParameter("@DocumentGuid", guid));
                }
                if (identity.PathCultureChannelLookup.TryGetValue(out var nodePathValues))
                {
                    // for documents, culture should be a preferred otherwise use current site default or first found
                    query += $"NodeAliasPath = @NodeAliasPath and NodeSiteID = @NodeSiteID order by case when DocumentCulture = @DocumentCulture then 0 else case when DocumentCulture = @DefaultCulture then 1 else 2 end end";
                    queryParams.Add(new DataParameter("@NodeAliasPath", nodePathValues.Path));
                    queryParams.Add(new DataParameter("@DocumentCulture", nodePathValues.Culture.GetValueOrDefault(culture)));
                    queryParams.Add(new DataParameter("@DefaultCulture", defaultCulture));
                    queryParams.Add(new DataParameter("@NodeSiteID", nodePathValues.ChannelId.GetValueOrDefault(currentSiteID)));
                }
                var item = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, queryParams, QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>();
                if (item.FirstOrMaybe().TryGetValue(out var docRow))
                {
                    return new ContentCultureIdentity()
                    {
                        ContentCultureID = (int)docRow["DocumentID"],
                        ContentCultureGuid = (Guid)docRow["DocumentGuid"],
                        PathCultureChannelLookup = new PathCultureChannel(Path: (string)docRow["NodeAliasPath"], Culture: (string)docRow["DocumentCulture"], ChannelId: (int)docRow["NodeSiteID"])
                    };
                }
                else
                {
                    return Result.Failure<ContentCultureIdentity>("Could not find a content culture item with the given content culture identity");
                }
            }
        }

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

            var classObj = (new InfoObjectFactory(className).Singleton);
            if (classObj is not BaseInfo baseClassObj)
            {
                return Result.Failure<ObjectIdentity>($"Class of {className} not a BaseInfo typed class, cannot parse.");
            }

            var dictionaryResult = await GetObjectIdentitiesAsync(classInfo, baseClassObj);
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

        private static string GetObjectIdentitySelectStatement(DataClassInfo classInfo, BaseInfo baseClassObj)
        {
            var typeInfo = baseClassObj.TypeInfo;
            return $"select {typeInfo.IDColumn} {(typeInfo.GUIDColumn.AsNullOrWhitespaceMaybe().TryGetValue(out var guidColumn) ? $", [{guidColumn}]" : "")} {(typeInfo.CodeNameColumn.AsNullOrWhitespaceMaybe().TryGetValue(out var codenameColumn) ? $", [{codenameColumn}]" : "")} from {classInfo.ClassTableName}";
        }

        [Obsolete("Use HydrateContentCultureIdentity")]
        public async Task<Result<DocumentIdentity>> HydrateDocumentIdentity(DocumentIdentity identity)
        {
            var result = await HydrateContentCultureIdentity(identity.ToContentCultureIdentity());
            if(result.TryGetValue(out var contentCultureIdentity, out var error))
            {
                return new DocumentIdentity()
                {
                    DocumentId = contentCultureIdentity.ContentCultureID,
                    DocumentGuid = contentCultureIdentity.ContentCultureGuid,
                    NodeAliasPathAndMaybeCultureAndSiteId = contentCultureIdentity.PathAndMaybeCultureAndChannelId
                };
            }
            return Result.Failure<DocumentIdentity>(error);
        }

        [Obsolete("Use HydrateTreeIdentity or HydrateContentIdentity")]
        public async Task<Result<NodeIdentity>> HydrateNodeIdentity(NodeIdentity identity)
        {
            var result = await HydrateTreeIdentity(identity.ToTreeIdentity());
            if (result.TryGetValue(out var treeIdentity, out var error))
            {
                return new NodeIdentity()
                {
                    NodeId = treeIdentity.PageID,
                    NodeGuid = treeIdentity.PageGuid,
                    NodeAliasPathAndSiteId = treeIdentity.PathAndChannelId
                };
            }
            return Result.Failure<NodeIdentity>(error);
        }

        #region "Dictionary Holder Populators"

        private async Task<Result<TreeIdentityHolder>> GetTreeIdentityHolderAsync()
        {
            var allPagesResult = await GetBaseAllPagesData();
            if (!allPagesResult.TryGetValue(out var allPages, out var error))
            {
                return Result.Failure<TreeIdentityHolder>(error);
            }

            var allPagesKeys = await GetAllPagesKeys();

            var channelIdToCulture = await SiteToDefaultCulture();

            return _progressiveCache.Load(cs =>
            {
                var builder = _cacheDependencyBuilderFactory.Create(false)
                    .AddKeys(allPagesKeys);

                if (cs.Cached)
                {
                    cs.CacheDependency = builder
                        .GetCMSCacheDependency();
                }

                var treeIdentityDictionaries = new TreeIdentityDictionaries();
                var treeCultureIdentityDictionaries = new TreeCultureIdentityDictionaries();
                foreach (var nodeGrouping in allPages.GroupBy(x => x.NodeId))
                {
                    var firstItem = nodeGrouping.First();
                    var treeIdentity = new TreeIdentity()
                    {
                        PageID = firstItem.NodeId,
                        PageGuid = firstItem.NodeGuid,
                        PathChannelLookup = new PathChannel(Path: firstItem.NodeAliasPath, ChannelId: firstItem.NodeSiteID)
                    };
                    var nodeKey = $"{firstItem.NodeAliasPath}|{firstItem.NodeSiteID}".ToLower();
                    treeIdentityDictionaries.ById.TryAdd(firstItem.NodeId, treeIdentity);
                    treeIdentityDictionaries.ByPathChannelIDKey.TryAdd(nodeKey, treeIdentity);
                    treeIdentityDictionaries.ByGuid.TryAdd(firstItem.NodeGuid, treeIdentity);

                    // Order by the default culture item first
                    foreach (var document in nodeGrouping.OrderBy(x => x.DocumentCulture.Equals(channelIdToCulture[x.NodeSiteID], StringComparison.OrdinalIgnoreCase) ? 0 : 1))
                    {
                        var documentIdentity = new TreeCultureIdentity(document.DocumentCulture)
                        {
                            PageID = document.NodeId,
                            PageGuid = document.NodeGuid,
                            PathChannelLookup = new PathChannel(Path: document.NodeAliasPath, ChannelId: document.NodeSiteID)
                        };
                        var documentKey = $"{document.NodeAliasPath}|{document.NodeSiteID}".ToLower();
                        var documentCulturelessKey = $"{document.NodeAliasPath}|{document.NodeSiteID}".ToLower();

                        treeCultureIdentityDictionaries.ById.TryAdd(document.NodeId, []);
                        treeCultureIdentityDictionaries.ById[document.NodeId].Add(documentIdentity);
                        treeCultureIdentityDictionaries.ByGuid.TryAdd(document.NodeGuid, []);
                        treeCultureIdentityDictionaries.ByGuid[document.NodeGuid].Add(documentIdentity);
                        treeCultureIdentityDictionaries.ByPathChannelIDKey.TryAdd(documentKey, []);
                        treeCultureIdentityDictionaries.ByPathChannelIDKey[documentKey].Add(documentIdentity);
                    }
                }
                return new TreeIdentityHolder(treeIdentityDictionaries, treeCultureIdentityDictionaries);

            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "GetTreeIdentityHolder"));

        }

        private async Task<Result<ContentIdentityHolder>> GetContentIdentityHolderAsync()
        {
            var allPagesResult = await GetBaseAllPagesData();
            if (!allPagesResult.TryGetValue(out var allPages, out var error))
            {
                return Result.Failure<ContentIdentityHolder>(error);
            }

            var allPagesKeys = await GetAllPagesKeys();

            var channelIdToCulture = await SiteToDefaultCulture();

            return _progressiveCache.Load(cs =>
            {
                var builder = _cacheDependencyBuilderFactory.Create(false)
                    .AddKeys(allPagesKeys);

                if (cs.Cached)
                {
                    cs.CacheDependency = builder
                        .GetCMSCacheDependency();
                }

                var contentIdentityDictionaries = new ContentIdentityDictionaries();
                var contentCultureIdentityDictionaries = new ContentCultureIdentityDictionaries();
                foreach (var nodeGrouping in allPages.GroupBy(x => x.NodeId))
                {
                    var firstItem = nodeGrouping.First();
                    var contentIdentity = new ContentIdentity()
                    {
                        ContentID = firstItem.NodeId,
                        ContentGuid = firstItem.NodeGuid,
                        PathChannelLookup = new PathChannel(Path: firstItem.NodeAliasPath, ChannelId: firstItem.NodeSiteID)
                    };
                    var pathChannelKey = $"{firstItem.NodeAliasPath}|{firstItem.NodeSiteID}".ToLower();
                    contentIdentityDictionaries.ById.TryAdd(firstItem.NodeId, contentIdentity);
                    contentIdentityDictionaries.ByPathChannelIDKey.TryAdd(pathChannelKey, contentIdentity);
                    contentIdentityDictionaries.ByGuid.TryAdd(firstItem.NodeGuid, contentIdentity);

                    // Order by the default culture item first
                    foreach (var document in nodeGrouping)
                    {
                        var documentIdentity = new ContentCultureIdentity()
                        {
                            ContentCultureID = document.DocumentID,
                            ContentCultureGuid = document.DocumentGuid,
                            PathCultureChannelLookup = new PathCultureChannel(Path: document.NodeAliasPath, Culture: document.DocumentCulture, ChannelId: document.NodeSiteID)
                        };

                        var contentIDCultureKey = $"{document.NodeId}|{document.DocumentCulture}".ToLower();
                        var pathCultureChannelKey = $"{document.NodeAliasPath}|{document.NodeSiteID}|{document.DocumentCulture}".ToLower();

                        contentCultureIdentityDictionaries.ById.TryAdd(document.DocumentID, documentIdentity);
                        contentCultureIdentityDictionaries.ByGuid.TryAdd(document.DocumentGuid, documentIdentity);
                        contentCultureIdentityDictionaries.ByContentIDAndCultureKey.TryAdd(contentIDCultureKey, documentIdentity);
                        contentCultureIdentityDictionaries.ByPathChannelCultureIDKey.TryAdd(pathCultureChannelKey, documentIdentity);
                        contentCultureIdentityDictionaries.ByPathChannelIDKey.TryAdd(pathChannelKey, documentIdentity);
                    }
                }
                return new ContentIdentityHolder(contentIdentityDictionaries, contentCultureIdentityDictionaries);

            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "GetContentIdentityHolder"));

        }

        private async Task<Result<ObjectIdentityDictionaries>> GetObjectIdentitiesAsync(DataClassInfo classInfo, BaseInfo baseClassObj)
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

                var dictionary = new ObjectIdentityDictionaries();
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
                        dictionary.ById.TryAdd(idVal, objectIdentity);
                    }
                    if (objectIdentity.Guid.TryGetValue(out var guidVal))
                    {
                        dictionary.ByGuid.TryAdd(guidVal, objectIdentity);
                    }
                    if (objectIdentity.CodeName.TryGetValue(out var codeNameVal))
                    {
                        dictionary.ByCodeName.TryAdd(codeNameVal, objectIdentity);
                    }
                }
                return dictionary;

            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetAllObjectIdentity", baseClassObj.TypeInfo.ObjectClassName));

        }

        #endregion

        #region "All Data"

        private async Task<IEnumerable<string>> GetAllPagesKeys()
        {
            return await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = _cacheDependencyBuilderFactory.Create(false).ObjectType(SiteInfo.OBJECT_TYPE).GetCMSCacheDependency();
                }
                return (await _siteInfoProvider.Get()
                 .ColumnsSafe(nameof(SiteInfo.SiteName))
                .GetEnumerableTypedResultAsync()
                )
                .Select(x => $"nodes|{x.SiteName}|/|childnodes");
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetAllPagesDependencyKeysOnAllSites"));
        }

        private async Task<Dictionary<int, string>> SiteToDefaultCulture()
        {
            var builder = _cacheDependencyBuilderFactory.Create()
                .ObjectType(SiteInfo.OBJECT_TYPE)
                .Object(SettingsKeyInfo.OBJECT_TYPE, "CMSDefaultCultureCode");

            return await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var query = $"select SiteID, COALESCE(SiteDefaultVisitorCulture, {nameof(SettingsKeyInfo.KeyValue)}) as DefaultCulture from CMS_Site left join (select top 1 {nameof(SettingsKeyInfo.KeyValue)} from CMS_SettingsKey where KeyName = 'CMSDefaultCultureCode') a on 1=1";
                return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery))
                .Tables[0].Rows.Cast<DataRow>()
                .Select(x => new Tuple<int, string>((int)x[nameof(SiteInfo.SiteID)], (string)x["DefaultCulture"]))
                .GroupBy(x => x.Item1)
                .ToDictionary(key => key.Key, value => value.First().Item2);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "SiteToDefaultCulture"));
        }

        private async Task<Result<IEnumerable<AllPagesData>>> GetBaseAllPagesData()
        {
            var keys = await GetAllPagesKeys();

            var builder = _cacheDependencyBuilderFactory.Create(false).AddKeys(keys);

            if (!builder.DependenciesNotTouchedSince(TimeSpan.FromSeconds(30)))
            {
                return Result.Failure<IEnumerable<AllPagesData>>("Dependency recently touched, waiting 30 seconds before using cached version.");
            }

            // Add universal key that indicates hydration service was used in case user wishes to manually clear this, normally this should not be within any other IProgressiveCache so it will clear itself normally, and not 
            // relavent to <cache> tag dependencies.
            _cacheDependencyBuilderFactory.Create().AddKey("IdentityService|AllPageIdentities");

            return await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var query = $"select NodeID, NodeGUID, NodeAliasPath, NodeSiteID, DocumentCulture, DocumentID, DocumentGUID from View_CMS_Tree_Joined";
                return Result.Success<IEnumerable<AllPagesData>>((await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery))
                    .Tables[0].Rows.Cast<DataRow>()
                    .Select(x => new AllPagesData(NodeId: (int)x["NodeID"], NodeGuid: (Guid)x["NodeGuid"], NodeAliasPath: (string)x["NodeAliasPath"], NodeSiteID: (int)x["NodeSiteID"], DocumentCulture: (string)x["DocumentCulture"], DocumentID: (int)x["DocumentID"], DocumentGuid: (Guid)x["DocumentGUID"]))
                    );
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetBaseNodeAndDocumentData"));
        }

        private record AllPagesData(int NodeId, Guid NodeGuid, string NodeAliasPath, int NodeSiteID, string DocumentCulture, int DocumentID, Guid DocumentGuid);

        #endregion


        #region "Dictionary Holders"

        private record NodeDocumentIdentityHolder(DocumentIdentityDictionaries Document, NodeIdentityDictionaries Node);

        private record TreeIdentityHolder(TreeIdentityDictionaries Tree, TreeCultureIdentityDictionaries TreeCulture);

        private record ContentIdentityHolder(ContentIdentityDictionaries Content, ContentCultureIdentityDictionaries ContentCulture);

        #endregion

        #region "Dictionaries"

        #pragma warning disable CS0618 // Type or member is obsolete, but this is used for internal implementation and KX13 support
        private class DocumentIdentityDictionaries
        {
            public Dictionary<int, DocumentIdentity> ById { get; set; } = [];
            public Dictionary<string, DocumentIdentity> ByNodeAliasPathCultureSiteIDKey { get; set; } = [];
            public Dictionary<string, DocumentIdentity> ByNodeAliasPathSiteIDKey { get; set; } = [];

            public Dictionary<Guid, DocumentIdentity> ByGuid { get; set; } = [];
        }

        private class NodeIdentityDictionaries
        {
            public Dictionary<int, NodeIdentity> ById { get; set; } = [];
            public Dictionary<string, NodeIdentity> ByNodeAliasPathSiteIDKey { get; set; } = [];
            public Dictionary<Guid, NodeIdentity> ByGuid { get; set; } = [];
        }
        #pragma warning restore CS0618 // Type or member is obsolete


        private class TreeIdentityDictionaries
        {
            public Dictionary<int, TreeIdentity> ById { get; set; } = [];
            public Dictionary<string, TreeIdentity> ByPathChannelIDKey { get; set; } = [];
            public Dictionary<Guid, TreeIdentity> ByGuid { get; set; } = [];
        }

        private class TreeCultureIdentityDictionaries
        {
            public Dictionary<int, List<TreeCultureIdentity>> ById { get; set; } = [];
            public Dictionary<string, List<TreeCultureIdentity>> ByPathChannelIDKey { get; set; } = [];
            public Dictionary<Guid, List<TreeCultureIdentity>> ByGuid { get; set; } = [];
        }

        private class ContentIdentityDictionaries
        {
            public Dictionary<int, ContentIdentity> ById { get; set; } = [];
            public Dictionary<string, ContentIdentity> ByPathChannelIDKey { get; set; } = [];
            public Dictionary<Guid, ContentIdentity> ByGuid { get; set; } = [];
        }

        private class ContentCultureIdentityDictionaries
        {
            public Dictionary<int, ContentCultureIdentity> ById { get; set; } = [];
            public Dictionary<string, ContentCultureIdentity> ByContentIDAndCultureKey { get; init; } = [];
            public Dictionary<string, ContentCultureIdentity> ByPathChannelCultureIDKey { get; set; } = [];
            public Dictionary<string, ContentCultureIdentity> ByPathChannelIDKey { get; set; } = [];
            public Dictionary<Guid, ContentCultureIdentity> ByGuid { get; set; } = [];
        }

        private class ObjectIdentityDictionaries
        {
            public Dictionary<int, ObjectIdentity> ById { get; set; } = [];
            public Dictionary<string, ObjectIdentity> ByCodeName { get; set; } = [];
            public Dictionary<Guid, ObjectIdentity> ByGuid { get; set; } = [];
        }

        #endregion
    }


}
