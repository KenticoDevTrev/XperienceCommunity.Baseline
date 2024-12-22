namespace Core.Repositories.Implementation
{
    public class CategoryCachedRepository(
            ICacheRepositoryContext cacheRepositoryContext,
            IInfoProvider<TagInfo> tagInfoProvider,
            IProgressiveCache progressiveCache,
            ICacheDependencyBuilderFactory cacheDependencyBuilderFactory
         ) : ICategoryCachedRepository
    {
        public ICacheRepositoryContext CacheRepositoryContext { get; } = cacheRepositoryContext;
        public IInfoProvider<TagInfo> TagInfoProvider { get; } = tagInfoProvider;
        public IProgressiveCache ProgressiveCache { get; } = progressiveCache;
        public ICacheDependencyBuilderFactory CacheDependencyBuilderFactory { get; } = cacheDependencyBuilderFactory;

        public IEnumerable<ObjectIdentity> CategoryNamesToCategoryIdentity(IEnumerable<string> categoryNames)
        {
            var dictionary = GetCategoriesCached().CategoryByCodeName;
            var lowerCatNames = categoryNames.Select(x => x.ToLowerInvariant().Trim());
            return dictionary.Where(x => lowerCatNames.Contains(x.Key)).Select(x => x.Value.ToObjectIdentity());
        }

        public Dictionary<string, CategoryItem> GetCategoryCachedByCodeName() => GetCategoriesCached().CategoryByCodeName;

        public Dictionary<Guid, CategoryItem> GetCategoryCachedByGuid() => GetCategoriesCached().CategoryByGuid;

        public Dictionary<int, CategoryItem> GetCategoryCachedById() => GetCategoriesCached().CategoryById;

        public IEnumerable<CategoryItem> GetCategoryIdentifiertoCategoryCached(IEnumerable<ObjectIdentity> categoryIdentity)
        {
            return categoryIdentity.Select(GetCategoryIdentifiertoCategoryCached).Where(x => x.IsSuccess).Select(x => x.Value);
        }

        public Result<CategoryItem> GetCategoryIdentifiertoCategoryCached(ObjectIdentity categoryIdentity)
        {
            var dictionaries = GetCategoriesCached();
            if (categoryIdentity.Id.TryGetValue(out var id) && dictionaries.CategoryById.TryGetValue(id, out var itemById)) {
                return itemById;
            } else if (categoryIdentity.CodeName.TryGetValue(out var codeName) && dictionaries.CategoryByCodeName.TryGetValue(codeName.ToLowerInvariant().Trim(), out var itemByCodename)) {
                return itemByCodename;
            } else if (categoryIdentity.Guid.TryGetValue(out var guid) && dictionaries.CategoryByGuid.TryGetValue(guid, out var itemByGuid)) {
                return itemByGuid;
            }
            return Result.Failure<CategoryItem>("Could not find category by that identity");
        }

        private CategoryDictionaryHolder GetCategoriesCached()
        {
            var builder = CacheDependencyBuilderFactory.Create()
                .ObjectType(TagInfo.OBJECT_TYPE);

            return ProgressiveCache.Load(cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                var tags = TagInfoProvider.Get()
                    .Columns(
                        nameof(TagInfo.TagID),
                        nameof(TagInfo.TagName),
                        nameof(TagInfo.TagGUID),
                        nameof(TagInfo.TagTitle),
                        nameof(TagInfo.TagDescription),
                        nameof(TagInfo.TagParentID),
                        nameof(TagInfo.TagTaxonomyID)
                    )
                    .GetEnumerableTypedResult();
                var categoryById = new Dictionary<int, CategoryItem>();
                var categoryByCodeName = new Dictionary<string, CategoryItem>();
                var categoryByGuid = new Dictionary<Guid, CategoryItem>();

                foreach (var tag in tags) {
                    var categoryItem = tag.ToCategoryItem();
                    categoryById.Add(tag.TagID, categoryItem);
                    categoryByCodeName.Add(tag.TagName.ToLower(), categoryItem);
                    categoryByGuid.Add(tag.TagGUID, categoryItem);
                }
                return new CategoryDictionaryHolder(categoryById, categoryByCodeName, categoryByGuid);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "CategoriesCachedToDictionary"));

        }

        private record CategoryDictionaryHolder(Dictionary<int, CategoryItem> CategoryById, Dictionary<string, CategoryItem> CategoryByCodeName, Dictionary<Guid, CategoryItem> CategoryByGuid);
    }

    public static class TagInfoExtensions
    {
        public static CategoryItem ToCategoryItem(this TagInfo tagInfo)
        {
            return new CategoryItem(categoryID: tagInfo.TagID,
                                    categoryGuid: tagInfo.TagGUID,
                                    categoryName: tagInfo.TagName,
                                    categoryTypeID: tagInfo.TagTaxonomyID,
                                    categoryDisplayName: tagInfo.TagTitle,
                                    categoryParentID: tagInfo.TagParentID.AsMaybeIfTrue(x => x > 0).AsNullableIntValue()) {
                CategoryDescription = tagInfo.TagDescription.AsNullOrWhitespaceMaybe()
            };
        }
    }
    
}
