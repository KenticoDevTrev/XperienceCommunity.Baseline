using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using Core.Enums;
using Core.Models;
using Core.Repositories;
using CSharpFunctionalExtensions;
using Localization.Models;
using Localization.Repositories;
using MVCCaching;
using System.Text.Json;

namespace Localization.Repositories.Implementations
{
    public class LocalizedCategoryCachedRepository(IInfoProvider<TagInfo> tagInfoProvider,
        IProgressiveCache progressiveCache,
        ICacheDependencyScopedBuilderFactory cacheDependencyBuilder,
        ILanguageRepository languageRepository,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider) : ILocalizedCategoryCachedRepository
    {
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ICacheDependencyScopedBuilderFactory _cacheDependencyBuilder = cacheDependencyBuilder;
        private readonly ILanguageRepository _languageRepository = languageRepository;
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider = contentLanguageInfoProvider;

        public LocalizedCategoryItem LocalizeCategoryItem(CategoryItem categoryItem, string cultureCode)
        {
            if (!GetCategoryIdToLocalizedWithVariants().TryGetValue(categoryItem.CategoryID, out var categoryWithVariants)) {
                return new LocalizedCategoryItem(categoryID: categoryItem.CategoryID,
                                                 categoryGuid: categoryItem.CategoryGuid,
                                                 categoryName: categoryItem.CategoryName,
                                                 categoryTypeID: categoryItem.CategoryTypeID,
                                                 categoryDisplayName: categoryItem.CategoryDisplayName,
                                                 categoryParentID: categoryItem.CategoryParentID.AsNullableIntValue()) 
                                                 { 
                                                    CategoryDescription = categoryItem.CategoryDescription 
                                                 };
            };
            var lookup = cultureCode.ToLowerInvariant();
            var lookupAlt = lookup.Split('-')[0];

            // Direct Matchies
            if (categoryWithVariants.LanguageVariants.TryGetValue(lookup, out var localizedCategory)) {
                return localizedCategory;
            }
            if (!lookupAlt.Equals(lookup) && categoryWithVariants.LanguageVariants.TryGetValue(lookupAlt, out var localizedCategorySplit)) {
                return localizedCategorySplit;
            }

            // No matches, just use main one
            return categoryWithVariants.MainCategoryItem;
        }

        public IEnumerable<LocalizedCategoryItem> LocalizeCategoryItems(IEnumerable<CategoryItem> categories, string cultureCode) => categories.Select(x => LocalizeCategoryItem(x, cultureCode));

        public async Task<LocalizedCategoryItem> LocalizeCategoryItemAsync(CategoryItem categoryItem, string? cultureCode = null)
        {
            if (!GetCategoryIdToLocalizedWithVariants().TryGetValue(categoryItem.CategoryID, out var categoryWithVariants)) {
                return new LocalizedCategoryItem(categoryItem.CategoryID, categoryItem.CategoryGuid, categoryItem.CategoryName, categoryItem.CategoryTypeID, categoryItem.CategoryDisplayName, categoryItem.CategoryParentID.AsNullableIntValue()) { CategoryDescription = categoryItem.CategoryDescription };
            };
            
            var lookup = (cultureCode ?? System.Globalization.CultureInfo.CurrentCulture.Name).ToLowerInvariant();
            var lookupAlt = lookup.Split('-')[0];

            // Direct Matches
            if (categoryWithVariants.LanguageVariants.TryGetValue(lookup.ToLowerInvariant(), out var localizedCategory)) {
                return localizedCategory;
            }
            if (categoryWithVariants.LanguageVariants.TryGetValue(lookupAlt.ToLowerInvariant(), out var localizedCategorySplit)) {
                return localizedCategorySplit;
            }

            // Try language fallbacks
            if ((await _languageRepository.GetLanguagueToSelect(categoryWithVariants.LanguageVariants.Keys, lookup, firstIfNoMatch: false, includeDefaultAsMatch: false)).TryGetValue(out var match)) {
                return categoryWithVariants.LanguageVariants[match.ToLowerInvariant()];
            }
            if (!lookupAlt.Equals(lookup) && (await _languageRepository.GetLanguagueToSelect(categoryWithVariants.LanguageVariants.Keys, lookupAlt, firstIfNoMatch: false, includeDefaultAsMatch: false)).TryGetValue(out var matchAlt)) {
                return categoryWithVariants.LanguageVariants[matchAlt.ToLowerInvariant()];
            }

            // No matches, just use main one
            return categoryWithVariants.MainCategoryItem;
        }

        public async Task<IEnumerable<LocalizedCategoryItem>> LocalizeCategoryItemsAsync(IEnumerable<CategoryItem> categories, string? cultureCode = null)
        {
            var items = new List<LocalizedCategoryItem>();
            foreach(var category in categories) {
                items.Add(await LocalizeCategoryItemAsync(category, cultureCode));
            }
            return items;
        }

        private Dictionary<int, CategoryItemWithLangVariants> GetCategoryIdToLocalizedWithVariants()
        {
            var builder = _cacheDependencyBuilder.Create()
                .ObjectType(TagInfo.OBJECT_TYPE)
                .ObjectType(ContentLanguageInfo.OBJECT_TYPE)
                .Object(SettingsKeyInfo.OBJECT_TYPE, "CMSDefaultCultureCode");

            return _progressiveCache.Load(cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                // CMS Default Language
                var cmsDefaultLanguage = _languageRepository.GetInstanceDefaultLanguage();

                var langItems = _contentLanguageInfoProvider.Get()
                  .Columns(nameof(ContentLanguageInfo.ContentLanguageGUID),
                           nameof(ContentLanguageInfo.ContentLanguageIsDefault),
                           nameof(ContentLanguageInfo.ContentLanguageName),
                           nameof(ContentLanguageInfo.ContentLanguageCultureFormat))
                  .GetEnumerableTypedResult();

                var langGuidToValue = langItems.ToDictionary(key => key.ContentLanguageGUID, value => value);

                // Default language by the "ContentLanguageIsDefault", honestly not sure which is primary...
                var defaultCulture = (langItems.Where(x => x.ContentLanguageIsDefault).FirstOrDefault()?.ContentLanguageCultureFormat ?? cmsDefaultLanguage.CodeName.GetValueOrDefault("en-US")).ToLowerInvariant();
                var defaultCultureName = (langItems.Where(x => x.ContentLanguageIsDefault).FirstOrDefault()?.ContentLanguageName ?? cmsDefaultLanguage.CodeName.GetValueOrDefault("en")).ToLowerInvariant();

                var tagInfos = _tagInfoProvider.Get()
                    .GetEnumerableTypedResult();

                var allTagDictionary = new Dictionary<int, CategoryItemWithLangVariants>();
                foreach (var tagInfo in tagInfos) {
                    var mainItem = MainTagInfoToLocalizedCategoryItem(tagInfo);

                    // Language matches may be on the full Culture ("en-US"), the language portion ("en"), or possibly the language code name ("english"), supporting all
                    var childrenByCultureCode = new Dictionary<string, LocalizedCategoryItem> {
                        { defaultCulture, mainItem }
                    };
                    childrenByCultureCode.TryAdd(defaultCulture.Split('-')[0], mainItem);
                    childrenByCultureCode.TryAdd(defaultCultureName, mainItem);

                    // Loop through metadata values for others
                    if (!string.IsNullOrWhiteSpace(tagInfo.TagMetadata)) {
                        var metaData = JsonSerializer.Deserialize<TagMetadata>(tagInfo.TagMetadata);
                        if (metaData != null && metaData.Translations.Count != 0) {
                            foreach (var translation in metaData.Translations) {
                                if (langGuidToValue.TryGetValue(translation.Key, out var language)) {
                                    var localizedItem = mainItem with {
                                        CategoryDisplayName = translation.Value.Title.AsNullOrWhitespaceMaybe().GetValueOrDefault(mainItem.CategoryDisplayName),
                                        CategoryDescription = translation.Value.Description.AsNullOrWhitespaceMaybe().GetValueOrDefault(mainItem.CategoryDescription)
                                    };
                                    childrenByCultureCode.TryAdd(language.ContentLanguageCultureFormat.ToLowerInvariant(), localizedItem);
                                    childrenByCultureCode.TryAdd(language.ContentLanguageCultureFormat.Split('-')[0].ToLowerInvariant(), localizedItem);
                                    childrenByCultureCode.TryAdd(language.ContentLanguageName.ToLowerInvariant(), localizedItem);
                                }
                            }
                        }
                    }

                    allTagDictionary.Add(tagInfo.TagID, new CategoryItemWithLangVariants(mainItem, childrenByCultureCode));
                };

                return allTagDictionary;
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetCategoryIdToLocalizedWithVariants"));
        }

        private record CategoryItemWithLangVariants(LocalizedCategoryItem MainCategoryItem, Dictionary<string, LocalizedCategoryItem> LanguageVariants);

        public static LocalizedCategoryItem MainTagInfoToLocalizedCategoryItem(TagInfo tagInfo) => new(categoryID: tagInfo.TagID,
                                                                                                       categoryGuid: tagInfo.TagGUID,
                                                                                                       categoryName: tagInfo.TagName,
                                                                                                       categoryTypeID: tagInfo.TagTaxonomyID,
                                                                                                       categoryDisplayName: tagInfo.TagTitle,
                                                                                                       categoryParentID: tagInfo.TagParentID.WithDefaultAsNone().AsNullableIntValue()) 
                                                                                                       {
                                                                                                            CategoryDescription = tagInfo.TagDescription.AsNullOrWhitespaceMaybe()
                                                                                                       };
    }
}
