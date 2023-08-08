﻿using CMS;
using CMS.DataEngine;
using CMS.Taxonomy;
using Core;
using System.Data;

namespace Generic.Repositories.Implementation
{
    [AutoDependencyInjection]
    public class PageCategoryRepository : IPageCategoryRepository
    {
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory;
        private readonly IProgressiveCache _progressiveCache;
        private readonly ICategoryCachedRepository _categoryCachedRepository;

        public PageCategoryRepository(
            ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
            IProgressiveCache progressiveCache,
            ICategoryCachedRepository categoryCachedRepository)
        {
            _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
            _progressiveCache = progressiveCache;
            _categoryCachedRepository = categoryCachedRepository;
        }

        public async Task<IEnumerable<CategoryItem>> GetCategoriesByNodeAsync(int nodeID)
        {
            var dictionary = (await GetCategoriesByIdentifiersAsync()).Item1;
            if (dictionary.ContainsKey(nodeID))
            {
                return dictionary[nodeID];
            }
            else
            {
                return Array.Empty<CategoryItem>();
            }
        }

        public async Task<IEnumerable<CategoryItem>> GetCategoriesByNodesAsync(IEnumerable<int> nodeIDs)
        {
            var dictionary = (await GetCategoriesByIdentifiersAsync()).Item1;
            var categoryItems = dictionary
                .Where(x => nodeIDs.Contains(x.Key))
                .SelectMany(x => x.Value)
                .Distinct(new CategoryItemEqualityComparer());
            return categoryItems;
        }

        public async Task<IEnumerable<CategoryItem>> GetCategoryItemsByPathAsync(string path)
        {
            var dictionary = (await GetCategoriesByIdentifiersAsync()).Item2;
            if (dictionary.ContainsKey(path.ToLowerInvariant()))
            {
                return dictionary[path.ToLowerInvariant()];
            }
            else
            {
                return Array.Empty<CategoryItem>();
            }
        }

        /// <summary>
        /// Helper function that gets ALL node/path categories in one query, used by others to quickly retrieve data.
        /// </summary>
        /// <returns></returns>
        private async Task<Tuple<IReadOnlyDictionary<int, IEnumerable<CategoryItem>>, IReadOnlyDictionary<string, IEnumerable<CategoryItem>>>> GetCategoriesByIdentifiersAsync()
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.ObjectType(TreeCategoryInfo.OBJECT_TYPE)
                .ObjectType(CategoryInfo.OBJECT_TYPE);

            return await _progressiveCache.Load(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                };

                // no need to cache this call as caching entire thing
                var query = @"
select T.NodeID, T.NodeAliasPath, C.CategoryID
from CMS_Tree T
inner join CMS_TreeCategory TC on TC.NodeID = T.NodeID
inner join CMS_Category C on C.CategoryID = TC.CategoryID
";

                var retriever = await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, new QueryDataParameters(), QueryTypeEnum.SQLQuery);

                // Group into two dictionaries
                var categoriesById = _categoryCachedRepository.GetCategoryCachedById();
                var items = retriever.Tables[0].Rows.Cast<DataRow>().Where(x => categoriesById.ContainsKey((int)x[nameof(CategoryInfo.CategoryID)])).Select(x => new PageCategoryItem(
                    nodeID: (int)x[nameof(TreeNode.NodeID)],
                    path: (string)x[nameof(TreeNode.NodeAliasPath)],
                    categoryItem: categoriesById[(int)x[nameof(CategoryInfo.CategoryID)]]
                ));

                var dictionaryByNodeID = items.GroupBy(x => x.NodeID).ToDictionary(key => key.Key, value => value.Select(x => x.CategoryItem));
                var dictionaryByPath = items.GroupBy(x => x.Path).ToDictionary(key => key.Key, value => value.Select(x => x.CategoryItem));
                var result = new Tuple<IReadOnlyDictionary<int, IEnumerable<CategoryItem>>, IReadOnlyDictionary<string, IEnumerable<CategoryItem>>>(dictionaryByNodeID, dictionaryByPath);
                return result;
            }, new CacheSettings(60, $"GetCategoriesByIdentifiersAsync"));
        }

        private record PageCategoryItem
        {
            public PageCategoryItem(int nodeID, string path, CategoryItem categoryItem)
            {
                NodeID = nodeID;
                Path = path;
                CategoryItem = categoryItem;
            }

            public int NodeID { get; init; }
            public string Path { get; init; }
            public CategoryItem CategoryItem { get; init; }
        }
    }

    
}
