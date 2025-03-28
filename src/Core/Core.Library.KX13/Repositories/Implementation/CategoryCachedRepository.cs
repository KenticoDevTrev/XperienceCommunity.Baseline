﻿using CMS.Taxonomy;

namespace Core.Repositories.Implementation
{
    public class CategoryCachedRepository(
        ICategoryInfoProvider _categoryInfoProvider,
        IProgressiveCache _progressiveCache,
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory) : ICategoryCachedRepository
    {
        public IEnumerable<ObjectIdentity> CategoryNamesToCategoryIdentity(IEnumerable<string> categoryNames)
        {
            List<ObjectIdentity> results = [];
            var categoriesByName = GetCachedHolder().ByCodeName;
            foreach (var key in categoriesByName.Keys.Intersect(categoryNames.Select(x => x.ToLower())))
            {
                results.Add(categoriesByName[key].ToObjectIdentity());
            }
            return results;
        }

        public Dictionary<Guid, CategoryItem> GetCategoryCachedByGuid() => GetCachedHolder().ByGuid;

        public Dictionary<int, CategoryItem> GetCategoryCachedById() => GetCachedHolder().ByID;

        public Dictionary<string, CategoryItem> GetCategoryCachedByCodeName() => GetCachedHolder().ByCodeName;

        public IEnumerable<CategoryItem> GetCategoryIdentifiertoCategoryCached(IEnumerable<ObjectIdentity> categoryIdentity) => categoryIdentity.Select(x => GetCategoryIdentifiertoCategoryCached(x)).Where(x => x.IsSuccess).Select(x => x.Value);

        public Result<CategoryItem> GetCategoryIdentifiertoCategoryCached(ObjectIdentity categoryIdentity)
        {
            var cached = GetCachedHolder();
            if (categoryIdentity.Id.TryGetValue(out var id) && id > 0)
            {
                return cached.ByID.GetValueOrMaybe(id).TryGetValue(out var foundItem) ? foundItem : Result.Failure<CategoryItem>($"No category found by id {id}");
            }
            if (categoryIdentity.CodeName.TryGetValue(out var codeName) && !string.IsNullOrWhiteSpace(codeName))
            {
                return cached.ByCodeName.GetValueOrMaybe(codeName.ToLowerInvariant()).TryGetValue(out var foundItem) ? foundItem : Result.Failure<CategoryItem>($"No category found by codename {codeName}");
            }
            if (categoryIdentity.Guid.TryGetValue(out var guid) && guid != Guid.Empty)
            {
                return cached.ByID.GetValueOrMaybe(id).TryGetValue(out var foundItem) ? foundItem : Result.Failure<CategoryItem>($"No category found by id {id}");
            }
            return Result.Failure<CategoryItem>("No identifier value found on passed ObjectIdentity");
        }

        private CategoryItemCachedHolder GetCachedHolder()
        {
            var builder = _cacheDependencyBuilderFactory.Create()
                .ObjectType(CategoryInfo.OBJECT_TYPE);

            return _progressiveCache.Load(cs =>
            {
                var allCategoryItems = _categoryInfoProvider.Get()
                .ColumnsSafe(nameof(CategoryInfo.CategoryID),
                nameof(CategoryInfo.CategoryName),
                nameof(CategoryInfo.CategoryDisplayName),
                nameof(CategoryInfo.CategoryGUID),
                nameof(CategoryInfo.CategoryParentID),
                nameof(CategoryInfo.CategoryDescription))
                .GetEnumerableTypedResult()
                .Select(x => x.CategoryInfoToItem());

                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                return new CategoryItemCachedHolder(
                    byID: allCategoryItems.GroupBy(key => key.CategoryID).ToDictionary(key => key.Key, value => value.First()),
                    byCodeName: allCategoryItems.GroupBy(key => key.CategoryName.ToLowerInvariant()).ToDictionary(key => key.Key, value => value.First()),
                    byGuid: allCategoryItems.GroupBy(key => key.CategoryGuid).ToDictionary(key => key.Key, value => value.First())
                    );
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetCategoryCachedHolder"));
        }

        private record CategoryItemCachedHolder
        {
            public CategoryItemCachedHolder(Dictionary<int, CategoryItem> byID, Dictionary<string, CategoryItem> byCodeName, Dictionary<Guid, CategoryItem> byGuid)
            {
                ByID = byID;
                ByCodeName = byCodeName;
                ByGuid = byGuid;
            }

            public Dictionary<int, CategoryItem> ByID { get; internal set; }
            public Dictionary<string, CategoryItem> ByCodeName { get; internal set; }
            public Dictionary<Guid, CategoryItem> ByGuid { get; internal set; }
        }
    }


}

namespace CMS.Taxonomy
{
    public static class CategoryInfoExtensions
    {
        public static CategoryItem CategoryInfoToItem(this CategoryInfo category)
        {
            return new CategoryItem(
                categoryID: category.CategoryID,
                categoryName: category.CategoryName,
                categoryGuid: category.CategoryGUID,
                categoryParentID: category.CategoryParentID,
                categoryTypeID: category.CategoryParentID,
                categoryDisplayName: category.CategoryDisplayName
                )
            {
                CategoryDescription = category.CategoryDescription.AsNullOrWhitespaceMaybe()
            };
        }
    }
}