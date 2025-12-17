using CMS.DataEngine;
using CMS.Localization;
using CMS.Taxonomy;
using System.Data;

namespace Localization.KX13.Repositories.Implementations
{
    public class LocalizedCategoryCachedRepository(
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory,
        IProgressiveCache _progressiveCache,
        LocalizationConfiguration _configuration) : ILocalizedCategoryCachedRepository
    {
        public LocalizedCategoryItem LocalizeCategoryItem(CategoryItem categoryItem, string cultureCode)
        {
            var localizationDictionary = GetLocalizedItemsCached();
            return LocalizeCategoryItem(categoryItem, cultureCode, localizationDictionary);
        }

        public IEnumerable<LocalizedCategoryItem> LocalizeCategoryItems(IEnumerable<CategoryItem> categories, string cultureCode)
        {
            var localizationDictionary = GetLocalizedItemsCached();
            return categories.Select(x => LocalizeCategoryItem(x, cultureCode, localizationDictionary));
        }

        private LocalizedCategoryItem LocalizeCategoryItem(CategoryItem categoryItem, string cultureCode, Dictionary<int, LocalizedCategoryValues> localizationDictionary )
        {
            var localizedCategoryItem = categoryItem.ToLocalizedCategoryItem();
            string cultureOrLanguage = cultureCode.ToLower();
            string language = cultureCode.Split('-')[0];
            string defaultCulture = _configuration.DefaultCulture.ToLower();
            string defaultLanguage = defaultCulture.Split("-")[0];
            if (localizationDictionary.TryGetValue(categoryItem.CategoryID, out var localizationValues))
            {
                var properKey = localizationValues.DisplayNames.Keys.OrderBy(cultureKey =>
                {
                    string categoryLanguage = cultureKey.Split('-')[0];
                    if (cultureKey.Equals(cultureOrLanguage))
                    {
                        return 0;
                    }
                    else if (categoryLanguage.Equals(language))
                    {
                        return 1;
                    }
                    if (cultureKey.Equals(defaultCulture))
                    {
                        return 2;
                    }
                    else if (cultureKey.Equals(defaultLanguage))
                    {
                        return 3;
                    }
                    else
                    {
                        return 4;
                    }
                }).FirstOrMaybe();
                var properDescriptionKey = localizationValues.Descriptions.Keys.OrderBy(cultureKey =>
                {
                    string categoryLanguage = cultureKey.Split('-')[0];
                    if (cultureKey.Equals(cultureOrLanguage))
                    {
                        return 0;
                    }
                    else if (categoryLanguage.Equals(language))
                    {
                        return 1;
                    }
                    if (cultureKey.Equals(defaultCulture))
                    {
                        return 2;
                    }
                    else if (cultureKey.Equals(defaultLanguage))
                    {
                        return 3;
                    }
                    else
                    {
                        return 4;
                    }
                }).FirstOrMaybe();

                // Clone record with new values if they are overwritten
                localizedCategoryItem = localizedCategoryItem with {
                    CategoryDisplayName = properKey.TryGetValue(out var displayNameKey) ? localizationValues.DisplayNames[displayNameKey] : localizedCategoryItem.CategoryDisplayName,
                    CategoryDescription = properDescriptionKey.TryGetValue(out var descriptionKey) ? localizationValues.Descriptions[descriptionKey] : localizedCategoryItem.CategoryDescription
                };
            }

            return localizedCategoryItem;
        }

       
        private Dictionary<int, LocalizedCategoryValues> GetLocalizedItemsCached()
        {
            var builder = _cacheDependencyBuilderFactory.Create()
                .ObjectType(CategoryInfo.OBJECT_TYPE)
                .ObjectType(ResourceStringInfo.OBJECT_TYPE)
                .ObjectType(ResourceTranslationInfo.OBJECT_TYPE);

            return _progressiveCache.Load(cs =>
            {
                var values = new Dictionary<int, InternalLocalizedCategoryValues>();
                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                var queryName = @"-- Gets the categories and translations, used to build the server-cached object  
select C.CategoryID, RT.TranslationText as CategoryDisplayName, Cu.CultureCode
from CMS_Category C  
left outer join CMS_ResourceString RS on RS.StringKey = TRIM(SUBSTRING(C.CategoryDisplayName, 3, len(C.CategoryDisplayname)-4))
left outer join CMS_ResourceTranslation RT on RT.TranslationStringID = RS.StringID 
left outer join CMS_Culture Cu on Cu.CultureID = RT.TranslationCultureID
where C.CategoryDisplayname like '{$%$}'
union all  
select C.CategoryID, C.CategoryDisplayName, 'en-US' as CultureCode
from CMS_Category C  
where C.CategoryDisplayname not like '{$%$}'";
                var queryDescriptions = @"-- Gets the categories and translations, used to build the server-cached object  
select C.CategoryID, RT.TranslationText as CategoryDescription
from CMS_Category C  
left outer join CMS_ResourceString RS on RS.StringKey = TRIM(SUBSTRING(C.CategoryDescription, 3, len(C.CategoryDescription)-4))  
left outer join CMS_ResourceTranslation RT on RT.TranslationStringID = RS.StringID 
left outer join CMS_Culture Cu on Cu.CultureID = RT.TranslationCultureID
where C.CategoryDescription like '{$%$}'
union all  
select C.CategoryID, C.CategoryparentID,C.CategoryDescription
from CMS_Category C  
where C.CategoryDescription not like '{$%$}'";
                var resultsName = ConnectionHelper.ExecuteQuery(queryName, [], QueryTypeEnum.SQLQuery);
                var resultsDescription = ConnectionHelper.ExecuteQuery(queryDescriptions, [], QueryTypeEnum.SQLQuery);

                foreach (DataRow dr in resultsName.Tables[0].Rows)
                {
                    int categoryID = (int)dr[nameof(CategoryInfo.CategoryID)];
                    string categoryDisplayName = (string)dr[nameof(CategoryInfo.CategoryDisplayName)];
                    Maybe<string> cultureCode = ValidationHelper.GetString(dr["CultureCode"], "").AsNullOrWhitespaceMaybe();

                    if (cultureCode.TryGetValue(out var cultureCodeVal))
                    {
                        if (!values.TryGetValue(categoryID, out var categoryValue))
                        {
                            categoryValue = new InternalLocalizedCategoryValues();
                            values.Add(categoryID, categoryValue);
                        }
                        categoryValue.DisplayNames.Add(cultureCodeVal.ToLower(), categoryDisplayName);
                    }
                }
                foreach (DataRow dr in resultsDescription.Tables[0].Rows)
                {
                    int categoryID = (int)dr[nameof(CategoryInfo.CategoryID)];
                    Maybe<string> categoryDescription = ValidationHelper.GetString(dr[nameof(CategoryInfo.CategoryDisplayName)], string.Empty).AsNullOrWhitespaceMaybe();
                    Maybe<string> cultureCode = ValidationHelper.GetString(dr["CultureCode"], "").AsNullOrWhitespaceMaybe();

                    if (cultureCode.TryGetValue(out var cultureCodeVal))
                    {
                        if (!values.TryGetValue(categoryID, out var value))
                        {
                            value = new InternalLocalizedCategoryValues();
                            values.Add(categoryID, value);
                        }
                        value.Descriptions.Add(cultureCodeVal.ToLower(), categoryDescription);
                    }
                }
                // return as read only version
                return values.ToDictionary(key => key.Key, value => new LocalizedCategoryValues(
                    displayNames: value.Value.DisplayNames,
                    descriptions: value.Value.Descriptions
                ));
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetCategoryLocalizedDictionary"));
        }

        public Task<LocalizedCategoryItem> LocalizeCategoryItemAsync(CategoryItem categoryItem, string? cultureCode = null) => Task.FromResult(LocalizeCategoryItem(categoryItem, cultureCode ?? System.Globalization.CultureInfo.CurrentCulture.Name));

        public Task<IEnumerable<LocalizedCategoryItem>> LocalizeCategoryItemsAsync(IEnumerable<CategoryItem> categories, string? cultureCode = null) => Task.FromResult(LocalizeCategoryItems(categories, cultureCode ?? System.Globalization.CultureInfo.CurrentCulture.Name));

        /// <summary>
        /// Temporary record used to build the dictionaries before converting to readonly
        /// </summary>
        private record InternalLocalizedCategoryValues
        {
            public Dictionary<string, string> DisplayNames { get; set; } = [];
            public Dictionary<string, Maybe<string>> Descriptions { get; set; } = [];
        }
    }

    public record LocalizedCategoryValues
    {
        public LocalizedCategoryValues(IReadOnlyDictionary<string, string> displayNames, IReadOnlyDictionary<string, Maybe<string>> descriptions)
        {
            DisplayNames = displayNames;
            Descriptions = descriptions;
        }

        public IReadOnlyDictionary<string, string> DisplayNames { get; init; }
        public IReadOnlyDictionary<string, Maybe<string>> Descriptions { get; init; }
    }

    public static class CategoryItemExtensions
    {
        public static LocalizedCategoryItem ToLocalizedCategoryItem(this CategoryItem item)
        {
            return new LocalizedCategoryItem(
                categoryID: item.CategoryID,
                categoryName: item.CategoryName,
                categoryGuid: item.CategoryGuid,
                categoryTypeID: item.CategoryParentID.GetValueOrDefault(0),
                categoryParentID: item.CategoryParentID.AsNullableIntValue(),
                categoryDisplayName: item.CategoryDisplayName)
            {
                CategoryDescription= item.CategoryDescription
            };
        }
    }
}

