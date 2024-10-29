using CMS;
using CMS.DataEngine;
using CMS.SiteProvider;
using System.Data;

namespace Core.KX13.Repositories.Implementation
{
    [AutoDependencyInjection]
    public class ContentCategoryRepository(ICategoryCachedRepository categoryCachedRepository,
        IIdentityService identityService,
        IProgressiveCache progressiveCache,
        ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
        ISiteInfoProvider siteInfoProvider) : IContentCategoryRepository
    {
        private readonly ICategoryCachedRepository _categoryCachedRepository = categoryCachedRepository;
        private readonly IIdentityService _identityService = identityService;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly ISiteInfoProvider _siteInfoProvider = siteInfoProvider;

        public async Task<IEnumerable<CategoryItem>> GetCategoriesByContentCultureIdentities(IEnumerable<ContentCultureIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null)
        {
            // get DocumentIds and node ids
            var docIds = new List<int>();
            foreach(var identity in identities) {
                if(identity.ContentCultureID.TryGetValue(out var docId)) {
                    docIds.Add(docId);
                } else if ((await identity.GetOrRetrieveContentCultureID(_identityService)).TryGetValue(out var docIdFromService)) {
                    docIds.Add(docIdFromService);
                }
            }
            if (docIds.Count == 0) {
                return [];
            }

            var docToNodeDictionary = await GetDocumentIdToNodeId();
            var nodeIds = docToNodeDictionary.Keys.Intersect(docIds).Select(x => docToNodeDictionary[x]);

            return await GetCategoriesByNodeAndDocuments(nodeIds, docIds);
        }

        public Task<IEnumerable<CategoryItem>> GetCategoriesByContentCultureIdentity(ContentCultureIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null) => GetCategoriesByContentCultureIdentities([identity]);

        public async Task<IEnumerable<CategoryItem>> GetCategoriesByContentIdentities(IEnumerable<ContentIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null)
        {
            // get all documentIds based on node ids
            var nodeIds = new List<int>();
            foreach (var identity in identities) {
                if (identity.ContentID.TryGetValue(out var docId)) {
                    nodeIds.Add(docId);
                } else if ((await identity.GetOrRetrieveContentID(_identityService)).TryGetValue(out var docIdFromService)) {
                    nodeIds.Add(docIdFromService);
                }
            }
            if (nodeIds.Count == 0) {
                return [];
            }

            var nodeToDocDictionary = await GetNodeIdToDocumentWithCultures();
            var docIds = nodeToDocDictionary.Keys.Intersect(nodeIds).SelectMany(x => nodeToDocDictionary[x].Select(x => x.DocumentId));

            return await GetCategoriesByNodeAndDocuments(nodeIds, docIds);
        }

        public Task<IEnumerable<CategoryItem>> GetCategoriesByContentIdentity(ContentIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null) => GetCategoriesByContentIdentities([identity]);    

        public async Task<IEnumerable<CategoryItem>> GetCategoriesByTreeCultureIdentities(IEnumerable<TreeCultureIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null)
        {
            var nodeIds = new List<int>();
            var docIds = new List<int>();
            var nodeToDocument = await GetNodeIdToDocumentWithCultures();
            foreach (var identity in identities) {
                if(identity.PageID.TryGetValue(out var nodeId)) {
                    nodeIds.Add(nodeId);
                    if(nodeToDocument.TryGetValue(nodeId, out var docs) && docs.FirstOrMaybe(x => x.Culture.Equals(identity.Culture, StringComparison.OrdinalIgnoreCase)).TryGetValue(out var docItem)) {
                        docIds.Add(docItem.DocumentId);
                    }
                } else if ((await identity.GetOrRetrievePageID(_identityService)).TryGetValue(out var nodeIdFromService)) {
                    nodeIds.Add(nodeIdFromService);
                    if (nodeToDocument.TryGetValue(nodeIdFromService, out var docs) && docs.FirstOrMaybe(x => x.Culture.Equals(identity.Culture, StringComparison.OrdinalIgnoreCase)).TryGetValue(out var docItem)) {
                        docIds.Add(docItem.DocumentId);
                    }
                }
            }

            return await GetCategoriesByNodeAndDocuments(nodeIds, docIds);
        }

        public Task<IEnumerable<CategoryItem>> GetCategoriesByTreeCultureIdentity(TreeCultureIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null) => GetCategoriesByTreeCultureIdentities([identity]);

        public async Task<IEnumerable<CategoryItem>> GetCategoriesByTreeIdentities(IEnumerable<TreeIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null)
        {
            var nodeIds = new List<int>();
            foreach (var identity in identities) {
                if (identity.PageID.TryGetValue(out var nodeId)) {
                    nodeIds.Add(nodeId);
                } else if ((await identity.GetOrRetrievePageID(_identityService)).TryGetValue(out var nodeIdFromService)) {
                    nodeIds.Add(nodeIdFromService);
                }
            }
            // Just convert to Content Identities
            return await GetCategoriesByContentIdentities(nodeIds.Select(x => x.ToContentIdentity()));
        }

        public Task<IEnumerable<CategoryItem>> GetCategoriesByTreeIdentity(TreeIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null) => GetCategoriesByTreeIdentities([identity]);

        private async Task<Dictionary<int, IEnumerable<int>>> GetNodeIdToCategoryIDs()
        {
            var builder = _cacheDependencyBuilderFactory.Create().ObjectType(TreeCategoryInfo.OBJECT_TYPE);

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                var query = $"select {nameof(TreeCategoryInfo.NodeID)}, {nameof(TreeCategoryInfo.CategoryID)} from CMS_TreeCategory order by {nameof(TreeCategoryInfo.NodeID)}";
                try {
                    return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery))
                        .Tables[0].Rows.Cast<DataRow>()
                        .GroupBy(x => (int)x[nameof(TreeCategoryInfo.NodeID)])
                        .ToDictionary(key => key.Key, value => value.Select(x => (int)x[nameof(TreeCategoryInfo.CategoryID)]));
                } catch (Exception) {
                    return [];
                }
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetNodeIdToCategoryIDs"));
        }

        private async Task<Dictionary<int, IEnumerable<DocWithCulture>>> GetNodeIdToDocumentWithCultures()
        {
            // not important to put cache dependency outside, just inside
            var siteNames = await GetSiteNames();

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(siteNames.Select(x => $"node|{x}|/|childnodes").ToArray());
                }

                var query = $"SELECT {nameof(TreeNode.DocumentID)} ,{nameof(TreeNode.DocumentCulture)} ,{nameof(TreeNode.DocumentNodeID)}  FROM CMS_Document order by {nameof(TreeNode.DocumentNodeID)}";
                try {
                    return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery))
                        .Tables[0].Rows.Cast<DataRow>()
                        .GroupBy(x => (int)x[nameof(TreeNode.DocumentNodeID)])
                        .ToDictionary(key => key.Key, value => value.Select(x => new DocWithCulture((int)x[nameof(TreeNode.DocumentID)], (string)x[nameof(TreeNode.DocumentCulture)])));
                } catch (Exception) {
                    return [];
                }
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetNodeIdToDocumentWithCultures"));
        }

        private async Task<Dictionary<int, IEnumerable<int>>> GetDocumentIDToCategoryIDs() {
            var builder = _cacheDependencyBuilderFactory.Create().ObjectType(DocumentCategoryInfo.OBJECT_TYPE);

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                var query = $"SELECT {nameof(DocumentCategoryInfo.DocumentID)} ,{nameof(DocumentCategoryInfo.CategoryID)} FROM CMS_DocumentCategory order by DocumentID";
                try {
                    return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery))
                        .Tables[0].Rows.Cast<DataRow>()
                        .GroupBy(x => (int)x[nameof(DocumentCategoryInfo.DocumentID)])
                        .ToDictionary(key => key.Key, value => value.Select(x => (int)x[nameof(DocumentCategoryInfo.CategoryID)]));
                } catch (Exception) {
                    return [];
                }
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetDocumentIDToCategoryIDs"));
        }

        private async Task<Dictionary<int, int>> GetDocumentIdToNodeId()
        {
            // not important to put cache dependency outside, just inside
            var siteNames = await GetSiteNames();

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(siteNames.Select(x => $"node|{x}|/|childnodes").ToArray());
                }

                var query = $"SELECT {nameof(TreeNode.DocumentID)} ,{nameof(TreeNode.DocumentNodeID)}  FROM CMS_Document order by {nameof(TreeNode.DocumentID)}";
                try {
                    return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery))
                        .Tables[0].Rows.Cast<DataRow>()
                        .ToDictionary(key =>(int)key[nameof(TreeNode.DocumentID)], value => (int)value[nameof(TreeNode.NodeID)]);
                } catch (Exception) {
                    return [];
                }
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetDocumentIdToNodeId"));
        }

        private async Task<IEnumerable<string>> GetSiteNames()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{SiteInfo.OBJECT_TYPE}|all");
                }
                return (await _siteInfoProvider.Get()
                .Columns(nameof(SiteInfo.SiteName))
                .GetEnumerableTypedResultAsync()).Select(x => x.SiteName);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetSiteNames"));
        }

        private record DocWithCulture(int DocumentId, string Culture);

        private async Task<IEnumerable<CategoryItem>> GetCategoriesByNodeAndDocuments(IEnumerable<int> nodeIds, IEnumerable<int> docIds)
        {
            var docIdToCategoryDictionary = await GetDocumentIDToCategoryIDs();
            var nodeIdToCategoryDictionary = await GetNodeIdToCategoryIDs();

            var categoryIds = docIdToCategoryDictionary.Keys.Intersect(docIds).SelectMany(x => docIdToCategoryDictionary[x]);
            var nodeCategoryIds = nodeIdToCategoryDictionary.Keys.Intersect(nodeIds).SelectMany(x => nodeIdToCategoryDictionary[x]);
            return _categoryCachedRepository.GetCategoryIdentifiertoCategoryCached(categoryIds.Union(nodeCategoryIds).Select(x => x.ToObjectIdentity()));
        }
    }
}
