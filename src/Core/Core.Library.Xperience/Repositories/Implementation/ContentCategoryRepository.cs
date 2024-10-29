using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using RelationshipsExtended;
using System.Data;
using System.Text.Json;
using System.Xml;
using XperienceCommunity.RelationshipsExtended;
using static Core.Models.ContentItemTaxonomyOptions;

namespace Core.Repositories.Implementation
{
    public class ContentCategoryRepository(
        ContentItemTaxonomyOptions contentItemTaxonomyOptions,
        IProgressiveCache progressiveCache,
        IIdentityService identityService,
        ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
        ICacheDependenciesScope cacheDependenciesScope,
        IContentQueryExecutor contentQueryExecutor,
        ICacheReferenceService cacheReferenceService,
        ICacheRepositoryContext cacheRepositoryContext,
        IInfoProvider<TagInfo> tagInfoProvider,
        IInfoProvider<TaxonomyInfo> taxonomyInfoProvider,
        RelationshipsExtendedOptions relationshipsExtendedOptions,
        ICategoryCachedRepository categoryCachedRepository,
        IInfoProvider<ContentItemCategoryInfo> contentItemCategoryInfoProvider,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        ILanguageFallbackRepository languageFallbackRepository,
        ICustomTaxonomyFieldParser customTaxonomyFieldParser
        ) : IContentCategoryRepository
    {
        private readonly ContentItemTaxonomyOptions _contentItemTaxonomyOptions = contentItemTaxonomyOptions;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IIdentityService _identityService = identityService;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly ICacheDependenciesScope _cacheDependenciesScope = cacheDependenciesScope;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly ICacheReferenceService _cacheReferenceService = cacheReferenceService;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;
        private readonly RelationshipsExtendedOptions _relationshipsExtendedOptions = relationshipsExtendedOptions;
        private readonly ICategoryCachedRepository _categoryCachedRepository = categoryCachedRepository;
        private readonly IInfoProvider<ContentItemCategoryInfo> _contentItemCategoryInfoProvider = contentItemCategoryInfoProvider;
        private readonly IPreferredLanguageRetriever _preferredLanguageRetriever = preferredLanguageRetriever;
        private readonly ILanguageFallbackRepository _languageFallbackRepository = languageFallbackRepository;
        private readonly ICustomTaxonomyFieldParser _customTaxonomyFieldParser = customTaxonomyFieldParser;

        public async Task<IEnumerable<CategoryItem>> GetCategoriesByContentCultureIdentities(IEnumerable<ContentCultureIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null)
        {
            // sort identities by language
            var byLang = await SortByLanguageAndConvertToContentIdentities(identities);
            var allCategoryIds = new List<int>();
            foreach(var lang in byLang.Keys) {
                allCategoryIds.AddRange(await GetCategoriesTagIdsInternal(byLang[lang], false, lang));
            }

            return await FilterByTaxonomyTypes(_categoryCachedRepository.GetCategoryIdentifiertoCategoryCached(allCategoryIds.Distinct().Select(x => x.ToObjectIdentity())), taxonomyTypes ?? []);
        }

        public Task<IEnumerable<CategoryItem>> GetCategoriesByContentCultureIdentity(ContentCultureIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null) => GetCategoriesByContentCultureIdentities([identity], taxonomyTypes);

        public async Task<IEnumerable<CategoryItem>> GetCategoriesByContentIdentities(IEnumerable<ContentIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null)
        {
            return await FilterByTaxonomyTypes(
                _categoryCachedRepository.GetCategoryIdentifiertoCategoryCached(
                    (await GetCategoriesTagIdsInternal(identities, true, null)).Distinct().Select(x => x.ToObjectIdentity())
                ), taxonomyTypes ?? []);
        }

        public Task<IEnumerable<CategoryItem>> GetCategoriesByContentIdentity(ContentIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null) => GetCategoriesByContentIdentities([identity], taxonomyTypes);

        public async Task<IEnumerable<CategoryItem>> GetCategoriesByTreeCultureIdentities(IEnumerable<TreeCultureIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null)
        {
            // sort identities by language
            var byLang = await SortByLanguageAndConvertToContentIdentities(identities);
            var allCategoryIds = new List<int>();
            foreach (var lang in byLang.Keys) {
                allCategoryIds.AddRange(await GetCategoriesTagIdsInternal(byLang[lang], false, lang));
            }

            return await FilterByTaxonomyTypes(_categoryCachedRepository.GetCategoryIdentifiertoCategoryCached(allCategoryIds.Distinct().Select(x => x.ToObjectIdentity())), taxonomyTypes ?? []);
        }

        public Task<IEnumerable<CategoryItem>> GetCategoriesByTreeCultureIdentity(TreeCultureIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null) => GetCategoriesByTreeCultureIdentities([identity], taxonomyTypes);

        public async Task<IEnumerable<CategoryItem>> GetCategoriesByTreeIdentities(IEnumerable<TreeIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null)
        {
            var allCategoryIds = await GetCategoriesTagIdsInternal(await TreeIdentitiesToContentIdentities(identities), true);
            return await FilterByTaxonomyTypes(_categoryCachedRepository.GetCategoryIdentifiertoCategoryCached(allCategoryIds.Distinct().Select(x => x.ToObjectIdentity())), taxonomyTypes ?? []);            
        }

        public Task<IEnumerable<CategoryItem>> GetCategoriesByTreeIdentity(TreeIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null) => GetCategoriesByTreeIdentities([identity], taxonomyTypes);

        /// <summary>
        /// Actual logic that runs most of the other functions.
        /// </summary>
        /// <param name="contentItems"></param>
        /// <param name="allLanguages"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private async Task<IEnumerable<int>> GetCategoriesTagIdsInternal(IEnumerable<ContentIdentity> contentItems, bool allLanguages, string? language = null)
        {
            var contentIdsByType = await SortByContentTypeAndGetContentItemID(contentItems);
            var configurationByType = await GetClassNameToTaxonomyFieldConfigurationDictionary();
            var languageToUse = (language ?? _preferredLanguageRetriever.Get()).ToLowerInvariant();
            var categoryTagIds = new List<int>();

            // Add Content Item Categories if they exist.
            categoryTagIds.AddRange(await GetRelExtendedContentItemCategoryTagIds(contentItems));

            // now add class field specific ones
            foreach (var type in contentIdsByType.Keys) {

                // If no configuration by type, then it doesn't contain any taxonomy fields, can continue
                if (!configurationByType.TryGetValue(type, out var configuration)) {
                    continue;
                }

                var contentItemIDToLangToTagIds = new Dictionary<int, Dictionary<string, List<int>>>();

                // If precaching, then use the cached methods, otherwise get the items not cached
                if (configuration.PreCache) {
                    contentItemIDToLangToTagIds = await GetCachedContentItemIdToLanguageToTagId(type);
                } else {
                    contentItemIDToLangToTagIds = await GetContentItemIdToLanguageToTagId(type, contentIdsByType[type], configuration, allLanguages, languageToUse);
                }

                // Now grab appropriate tags
                foreach (var contentItemId in contentIdsByType[type]) {
                    if (!contentItemIDToLangToTagIds.TryGetValue(contentItemId, out var langToTags)) {
                        continue;
                    }

                    // if all languages, add them all
                    if (allLanguages) {
                        categoryTagIds.AddRange(langToTags.Values.SelectMany(x => x));
                        continue;
                    }

                    // Get the proper language tags
                    if ((await _languageFallbackRepository.GetLanguagueToSelect(langToTags.Keys, languageToUse, true)).TryGetValue(out var lang)) {
                        categoryTagIds.AddRange(langToTags[lang]);
                    }
                }
                continue;

            }

            // Return the categories now found
            return categoryTagIds;
        }

        #region "Content Item To Category Methods"

        private async Task<Dictionary<int, Dictionary<string, List<int>>>> GetCachedContentItemIdToLanguageToTagId(string contentType)
        {
            var lookupDictionaries = await GetTagLookups();
            var results = await _progressiveCache.LoadAsync(async cs => {

                var builder = _cacheDependencyBuilderFactory.Create(false)
                .ContentType(contentType);

                // Will add dependency keys if user sets in their customizations
                _cacheDependenciesScope.Begin();

                var configuration = await GetClassNameToTaxonomyFieldConfigurationDictionary();
                if (!configuration.TryGetValue(contentType.ToLowerInvariant(), out var classTaxonomyConfiguration)) {
                    return new DTOWithDependencies<Dictionary<int, Dictionary<string, List<int>>>>([], builder.GetKeys().ToList());
                }

                var queryBuilder = new ContentItemQueryBuilder()
                    .ForContentType(contentType, query => query
                        .Columns(
                            classTaxonomyConfiguration.FieldIdentifiers.SelectMany(x => new string[] { x.TaxonomyFieldName })
                            .Union([nameof(ContentItemFields.ContentItemID), nameof(ContentItemFields.ContentItemCommonDataContentLanguageID)])
                            .Distinct().ToArray())
                    );

                // Execute
                var allContentItemDataContainers = await _contentQueryExecutor.GetResult(builder: queryBuilder, options: new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext), resultSelector: (contentItemDataContainer) => {
                    return contentItemDataContainer;
                });

                var compiledDictionary = new Dictionary<int, Dictionary<string, List<int>>>();
                foreach (var contentItemDataContainer in allContentItemDataContainers) {
                    if (!compiledDictionary.TryGetValue(contentItemDataContainer.ContentItemID, out Dictionary<string, List<int>>? contentItemDictionary)) {
                        contentItemDictionary = [];
                        compiledDictionary.Add(contentItemDataContainer.ContentItemID, contentItemDictionary);
                    }

                    var language = _cacheReferenceService.GetLanguageNameById(contentItemDataContainer.ContentItemCommonDataContentLanguageID).ToLowerInvariant();

                    // Make language area of it
                    if (!contentItemDictionary.TryGetValue(language, out List<int>? tagIds)) {
                        tagIds = [];
                        contentItemDictionary.Add(language, tagIds);
                    }

                    // This may add additional dependencies to the scope for Custom Field Types
                    tagIds.AddRange(await GetTagsFromCategoryFields(contentItemDataContainer, classTaxonomyConfiguration, lookupDictionaries));
                }

                // Add the keys
                builder.AddKeys(_cacheDependenciesScope.End());

                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                return new DTOWithDependencies<Dictionary<int, Dictionary<string, List<int>>>>(compiledDictionary, builder.GetKeys().ToList());
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetCachedContentGuidToFieldGuidToLanguageMediaItems", contentType, _cacheRepositoryContext.GetCacheKey()));

            // Add to the cache scope
            _cacheDependencyBuilderFactory.Create().AddKeys(results.AdditionalDependencies);

            return results.Result;
        }

        private async Task<Dictionary<int, Dictionary<string, List<int>>>> GetContentItemIdToLanguageToTagId(string type, IEnumerable<int> contentItemIds, ContentTypeTaxonomyConfigurations configuration, bool allLanguages, string language)
        {
            var lookupDictionaries = await GetTagLookups();

            // if not using pre cache, then will need to make the call individually
            var queryBuilder = new ContentItemQueryBuilder()
                .ForContentType(type, query =>
                    query.Columns(configuration.FieldIdentifiers.Select(x => x.TaxonomyFieldName).Union([nameof(ContentItemFields.ContentItemID), nameof(ContentItemFields.ContentItemCommonDataContentLanguageID)]).Distinct().ToArray())
                    .Where(where => where.WhereIn(nameof(ContentItemFields.ContentItemID), contentItemIds))
                );
            if (!allLanguages) {
                queryBuilder.InLanguage(language, useLanguageFallbacks: true);
            }

            var results = await _contentQueryExecutor.GetResult(queryBuilder, resultSelector: result => result, options: new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext));
            var contentItemIdToField = results.GroupBy(x => x.ContentItemID).ToDictionary(key => key.Key, value => value);
            var resultDictionary = new Dictionary<int, Dictionary<string, List<int>>>();
            foreach (var contentId in contentItemIdToField.Keys) {

                if (!resultDictionary.TryGetValue(contentId, out var langToTagIds)) {
                    langToTagIds = [];
                    resultDictionary.Add(contentId, langToTagIds);
                }
                foreach (var dataItem in contentItemIdToField[contentId]) {
                    var lang = _cacheReferenceService.GetLanguageNameById(dataItem.ContentItemCommonDataContentLanguageID).ToLowerInvariant();
                    if (!langToTagIds.ContainsKey(lang)) {
                        langToTagIds.Add(lang, (await GetTagsFromCategoryFields(dataItem, configuration, lookupDictionaries)).ToList());
                    }
                }
            }
            return resultDictionary;
        }


        #endregion

        #region "Configuration and Filtering Methods"

        private async Task<Dictionary<string, ContentTypeTaxonomyConfigurations>> GetClassNameToTaxonomyFieldConfigurationDictionary()
        {
            return await _progressiveCache.LoadAsync(async cs => {

                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([$"{DataClassInfo.OBJECT_TYPE}|all"]);
                }

                // Add to Cache
                var classes = await DataClassInfoProvider.GetClasses()
                   .Columns(nameof(DataClassInfo.ClassName), nameof(DataClassInfo.ClassFormDefinition))
                   .WhereLike(nameof(DataClassInfo.ClassFormDefinition), "%columntype=\"taxonomy\"%")
                   .GetEnumerableTypedResultAsync();

                var typeToFieldToConfiguration = classes
                    .ToDictionary(key => key.ClassName.ToLowerInvariant(), value => {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(value.ClassFormDefinition);
                        var nodes = xmlDoc.SelectNodes("//field[@columntype='taxonomy']")?.Cast<XmlNode>() ?? [];
                        var nodeList = nodes.Where(x => x != null && x.Attributes != null && (x.Attributes["column"] != null));
#pragma warning disable CS8602 // Dereference of a possibly null reference. - Ensured above that this will not be the case
                        return nodeList.ToDictionary(key => key.Attributes["column"].Value.ToLowerInvariant(), value => new TaxonomyFieldIdentifier(value.Attributes["column"].Value.ToLowerInvariant(), TaxonomyFieldIdentifierType.Taxonomy));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    });

                // combine with any configurations the user has provided
                var configuration = new Dictionary<string, ContentTypeTaxonomyConfigurations>();

                foreach (var itemConfiguration in _contentItemTaxonomyOptions.ContentItemConfigurations) {

                    if (!typeToFieldToConfiguration.TryGetValue(itemConfiguration.ClassName.ToLowerInvariant(), out var classDictionary)) {
                        classDictionary = [];
                        typeToFieldToConfiguration.Add(itemConfiguration.ClassName.ToLowerInvariant(), classDictionary);
                    }
                    foreach (var config in itemConfiguration.TaxonomyFieldIdentifier) {
                        if (!classDictionary.TryGetValue(config.TaxonomyFieldName.ToLowerInvariant(), out var fieldToConfiguration)) {
                            fieldToConfiguration = new TaxonomyFieldIdentifier(
                                taxonomyFieldName: config.TaxonomyFieldName,
                                taxonomyFieldType: config.TaxonomyFieldType);

                            classDictionary.Add(config.TaxonomyFieldName.ToLowerInvariant(), fieldToConfiguration);
                        }
                    }

                    configuration.TryAdd(itemConfiguration.ClassName.ToLowerInvariant(), new ContentTypeTaxonomyConfigurations([.. classDictionary.Values], itemConfiguration.PreCache));
                }
                return configuration;
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetClassNameToTaxonomyFieldConfigurationDictionary"));
        }

        private async Task<IEnumerable<int>> GetTagsFromCategoryFields(IContentQueryDataContainer dataContainer, ContentTypeTaxonomyConfigurations configuration, TagLookup tagLookup)
        {
            var tagIds = new List<int>();
            var codeNames = new List<string>();
            var guids = new List<Guid>();

            foreach (var taxonomyField in configuration.FieldIdentifiers) {
                // Custom handling
                if(taxonomyField.TaxonomyFieldType == TaxonomyFieldIdentifierType.Custom) {
                    var identities = await _customTaxonomyFieldParser.GetTagIdentities(dataContainer, taxonomyField);
                    foreach(var identity in identities) {
                        if(identity.Id.TryGetValue(out var id)) {
                            tagIds.Add(id);
                        } else if(identity.CodeName.TryGetValue(out var name)) {
                            codeNames.Add(name);
                        } else if(identity.Guid.TryGetValue(out var guid)) {
                            guids.Add(guid);
                        }
                    }
                    continue;
                }

                // now get value from field and parse it
                var fieldValue = (dataContainer.GetValue<string>(taxonomyField.TaxonomyFieldName)).AsNullOrWhitespaceMaybe();
                var serializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                if (fieldValue.TryGetValue(out var fieldValStr)) {
                    switch (taxonomyField.TaxonomyFieldType) {
                        case TaxonomyFieldIdentifierType.Taxonomy:
                            try {
                                if (JsonSerializer.Deserialize<TagReference[]>(fieldValStr, serializerOptions).AsMaybe().TryGetValue(out var values)) {
                                    guids.AddRange(values.Select(x => x.Identifier));
                                }
                            } catch (Exception) {
                                // TODO: Test what possibly could happen with weird values
                            }
                            break;
                        case TaxonomyFieldIdentifierType.ObjectCodeNames:
                            try {
                                if (JsonSerializer.Deserialize<string[]>(fieldValStr, serializerOptions).AsMaybe().TryGetValue(out var values)) {
                                    codeNames.AddRange(values.Select(x => x.ToLowerInvariant()));
                                }
                            } catch (Exception) {
                                // TODO: Test what possibly could happen with weird values
                            }
                            break;
                        case TaxonomyFieldIdentifierType.ObjectGlobalIdentities:
                            try {
                                if (JsonSerializer.Deserialize<string[]>(fieldValStr, serializerOptions).AsMaybe().TryGetValue(out var values)) {
                                    foreach (var value in values) {
                                        if (Guid.TryParse(value, out var guid)) {
                                            guids.Add(guid);
                                        }
                                    }
                                }
                            } catch (Exception) {
                                // TODO: Test what possibly could happen with weird values
                            }
                            break;
                        default:
                            break;
                    }
                }

                // Convert code name and guids to ids
                tagIds.AddRange(tagLookup.TagGuidToTagID.Keys.Intersect(guids).Select(x => tagLookup.TagGuidToTagID[x]));
                tagIds.AddRange(tagLookup.TagNameToTagId.Keys.Intersect(codeNames).Select(x => tagLookup.TagNameToTagId[x]));
            }
            return tagIds.Distinct();
        }

        private async Task<IEnumerable<CategoryItem>> FilterByTaxonomyTypes(IEnumerable<CategoryItem> categories, IEnumerable<ObjectIdentity> taxonomyTypes)
        {
            // No given taxonomy types, show all
            if (!taxonomyTypes.Any()) {
                return categories;
            }

            var builder = _cacheDependencyBuilderFactory.Create().ObjectType(TaxonomyInfo.OBJECT_TYPE);
            // lookups for object identity
            var lookupDictionaries = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var taxonomy = await taxonomyInfoProvider.Get()
                .Columns(nameof(TaxonomyInfo.TaxonomyID), nameof(TaxonomyInfo.TaxonomyGUID), nameof(TaxonomyInfo.TaxonomyName))
                .GetEnumerableTypedResultAsync();

                return new TaxonomyLookup(taxonomy.ToDictionary(key => key.TaxonomyGUID, value => value.TaxonomyID), taxonomy.ToDictionary(key => key.TaxonomyName.ToLowerInvariant(), value => value.TaxonomyID));
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetTaxonomyLookup"));

            var taxonomyIds = new List<int>();
            foreach (var taxonomyType in taxonomyTypes) {
                if (taxonomyType.Id.TryGetValue(out var id)) {
                    taxonomyIds.Add(id);
                } else if (taxonomyType.CodeName.TryGetValue(out var codeName) && lookupDictionaries.TaxonomyNameToTaxonomyId.TryGetValue(codeName.ToLowerInvariant(), out var idByName)) {
                    taxonomyIds.Add(idByName);
                } else if (taxonomyType.Guid.TryGetValue(out var guid) && lookupDictionaries.TaxonomyGuidToTaxonomyID.TryGetValue(guid, out var idByGuid)) {
                    taxonomyIds.Add(idByGuid);
                }
            }

            return categories.Where(x => taxonomyIds.Contains(x.CategoryTypeID));
        }

        #endregion

        #region "Sorting and converting methods"

        private async Task<Dictionary<string, List<int>>> SortByContentTypeAndGetContentItemID(IEnumerable<ContentIdentity> contentItems)
        {
            var contentIdsByType = new Dictionary<string, List<int>>();
            foreach (var contentItem in contentItems) {
                if ((await _identityService.GetContentType(contentItem)).TryGetValue(out var type)) {
                    if (!contentIdsByType.TryGetValue(type.ToLowerInvariant(), out var identityList)) {
                        identityList = [];
                        contentIdsByType.Add(type.ToLowerInvariant(), identityList);
                    }
                    if ((await contentItem.GetOrRetrieveContentID(_identityService)).TryGetValue(out var id)) {
                        identityList.Add(id);
                    }
                }
            }
            return contentIdsByType;
        }

        /// <summary>
        /// Annoyingly, it's possible that a user may send different language content culture identities, and our logic is more optimized for content item to lang.
        /// </summary>
        /// <param name="cultureIdentities"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, List<ContentIdentity>>> SortByLanguageAndConvertToContentIdentities(IEnumerable<ContentCultureIdentity> cultureIdentities)
        {
            var results = new Dictionary<string, List<ContentIdentity>>();
            var builder = _cacheDependencyBuilderFactory.Create(false);

            foreach (var cultureIdentity in cultureIdentities.Where(x => x.ContentCultureLookup.HasValue)) {

                if (cultureIdentity.ContentCultureLookup.TryGetValue(out var lookup)) {
                    var lang = lookup.Culture.GetValueOrDefault(_preferredLanguageRetriever.Get()).ToLowerInvariant();
                    if (!results.TryGetValue(lang, out var identityList)) {
                        identityList = [];
                        results.Add(lang, identityList);
                    }
                    identityList.Add(lookup.ContentId.ToContentIdentity());
                    builder.ContentItemOnLanguage(lookup.ContentId, lang);
                }
            }

            // handle rest
            var byId = cultureIdentities.Where(x => x.ContentCultureID.HasValue).Select(x => x.ContentCultureID.Value);
            var byGuid = cultureIdentities.Where(x => x.ContentCultureGuid.HasValue).Select(x => x.ContentCultureGuid.Value);

            var additionalItems = await _progressiveCache.LoadAsync(async cs => {

                var additionalBuilder = _cacheDependencyBuilderFactory.Create(false);
                var query = $"select ContentItemCommonDataContentLanguageID , {nameof(ContentItemFields.ContentItemCommonDataContentLanguageID)} from CMS_ContentItemCommonData where 1=1";
                if (byId.Any() || byId.Any()) {
                    if (byId.Any()) {
                        query += $" and ContentItemCommonDataID in ({string.Join(',', byId)})";
                    }
                    if (byGuid.Any()) {
                        query += $" and ContentItemCommonDataGUID in ('{string.Join("','", byGuid)}')";
                    }
                    var items = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>()
                            .GroupBy(x => _cacheReferenceService.GetLanguageNameById((int)x["ContentItemCommonDataContentLanguageID"]).ToLowerInvariant())
                            .ToDictionary(key => key.Key, value => value.Select(x => (int)x[nameof(ContentItemFields.ContentItemID)]));

                    foreach (var key in items.Keys) {
                        if (!results.TryGetValue(key, out var identityList)) {
                            identityList = [];
                            results.Add(key, identityList);
                        }
                        identityList.AddRange(items[key].Select(x => x.ToContentIdentity()));
                        foreach (var item in items[key]) {
                            additionalBuilder.ContentItemOnLanguage(item, key);
                        }
                    }
                }

                if (cs.Cached) {
                    cs.CacheDependency = additionalBuilder.GetCMSCacheDependency();
                }

                return new DTOWithDependencies<Dictionary<string, List<ContentIdentity>>>(results, additionalBuilder.GetKeys().ToList());

            }, new CacheSettings(CacheMinuteTypes.Short.ToDouble(), "SortByLanguageAndConvertToContentIdentities", string.Join(",", byId), string.Join(",", byGuid)));

            // Add dependencies to the scope
            builder.AddKeys(additionalItems.AdditionalDependencies);

            return additionalItems.Result;
        }

        /// <summary>
        /// Annoyingly, it's possible that a user may send different language content culture identities, and our logic is more optimized for content item to lang.
        /// </summary>
        /// <param name="cultureIdentities"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, List<ContentIdentity>>> SortByLanguageAndConvertToContentIdentities(IEnumerable<TreeCultureIdentity> treeCultures)
        {
            var results = new Dictionary<string, List<ContentIdentity>>();
            var builder = _cacheDependencyBuilderFactory.Create(false);
            var byCulture = treeCultures.GroupBy(x => x.Culture.ToLowerInvariant()).ToDictionary(key => key.Key, value => value.Select(x => x));
            foreach (var culture in byCulture.Keys) {
                results.Add(culture, await TreeIdentitiesToContentIdentities(byCulture[culture]));
            }
            return results;
        }

        private async Task<List<ContentIdentity>> TreeIdentitiesToContentIdentities(IEnumerable<TreeIdentity> identities)
        {
            var lookups = await GetWebPageLookups();
            var list = new List<ContentIdentity>();
            foreach (var identity in identities) {
                if (identity.PageID.TryGetValue(out var pageId) && lookups.PageIdToContentID.TryGetValue(pageId, out var contentIdById)) {
                    list.Add(contentIdById.ToContentIdentity());
                } else if (identity.PageGuid.TryGetValue(out var pageGuid) && lookups.PageGuidToContentID.TryGetValue(pageGuid, out var contentIdByGuid)) {
                    list.Add(contentIdByGuid.ToContentIdentity());
                } else if (identity.PageName.TryGetValue(out var pageName) && lookups.PageNameToContentID.TryGetValue(pageName.ToLowerInvariant(), out var contentIdByName)) {
                    list.Add(contentIdByName.ToContentIdentity());
                } else if (identity.PathChannelLookup.TryGetValue(out var pathAndChannelId) && lookups.PathChannelKeyToContentID.TryGetValue($"{pathAndChannelId.Path.ToLowerInvariant()}|{pathAndChannelId.ChannelId}", out var contentIdByPathChannel)) {
                    list.Add(contentIdByPathChannel.ToContentIdentity());
                }
            }

            return list;
        }


        #endregion

        #region "Optimization Dictionaries"

        private async Task<TagLookup> GetTagLookups()
        {
            var builder = _cacheDependencyBuilderFactory.Create()
                .ObjectType(TagInfo.OBJECT_TYPE);

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var tags = await _tagInfoProvider.Get()
                .Columns(nameof(TagInfo.TagID), nameof(TagInfo.TagName), nameof(TagInfo.TagGUID))
                .GetEnumerableTypedResultAsync();

                return new TagLookup(
                    TagGuidToTagID: tags.ToDictionary(key => key.TagGUID, value => value.TagID),
                    TagNameToTagId: tags.ToDictionary(key => key.TagName.ToLowerInvariant(), value => value.TagID)
                    );
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetTagLookups"));
        }

        private async Task<Result<Dictionary<int, IEnumerable<int>>>> GetContentItemIDToTagIds()
        {
            var builder = _cacheDependencyBuilderFactory.Create().ObjectType(ContentItemCategoryInfo.OBJECT_TYPE);
            if (!builder.DependenciesNotTouchedSince(new TimeSpan(0, 0, 30))) {
                return Result.Failure<Dictionary<int, IEnumerable<int>>>("Categories are being added, do not use cached");
            }
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                // query will be faster
                var query = $"SELECT {nameof(ContentItemCategoryInfo.ContentItemCategoryContentItemID)} ,{nameof(ContentItemCategoryInfo.ContentItemCategoryTagID)} FROM RelationshipsExtended_ContentItemCategory order by {nameof(ContentItemCategoryInfo.ContentItemCategoryContentItemID)}";
                return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>()
                .GroupBy(x => (int)x[nameof(ContentItemCategoryInfo.ContentItemCategoryContentItemID)])
                .ToDictionary(key => key.Key, value => value.Select(x => (int)x[nameof(ContentItemCategoryInfo.ContentItemCategoryTagID)]));
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetContentItemIDToTagIds"));
        }

        private async Task<IEnumerable<int>> GetRelExtendedContentItemCategoryTagIds(IEnumerable<ContentIdentity> identities)
        {
            if (!_relationshipsExtendedOptions.AllowContentItemCategories) {
                return [];
            }
            // get by ID
            var contentIds = new List<int>();
            foreach (var identity in identities) {
                if ((await identity.GetOrRetrieveContentID(_identityService)).TryGetValue(out var id)) {
                    contentIds.Add(id);
                }
            }

            if ((await GetContentItemIDToTagIds()).TryGetValue(out var dictionary)) {
                return dictionary.Keys.Intersect(contentIds).SelectMany(x => dictionary[x]).Select(x => x);
            }

            // Manual Lookup
            var builder = _cacheDependencyBuilderFactory.Create().ObjectType(ContentItemCategoryInfo.OBJECT_TYPE);
            var tagIds = await _progressiveCache.LoadAsync(async cs => {

                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                return (await _contentItemCategoryInfoProvider.Get()
                .Columns(nameof(ContentItemCategoryInfo.ContentItemCategoryTagID))
                .WhereIn(nameof(ContentItemCategoryInfo.ContentItemCategoryContentItemID), contentIds)
                .GetEnumerableTypedResultAsync()).Select(x => x.ContentItemCategoryTagID);
            }, new CacheSettings(CacheMinuteTypes.Short.ToDouble(), "GetTagIdsFromContentIds", string.Join(',', contentIds)));

            return tagIds;
        }

        private async Task<PageIdentityLookups> GetWebPageLookups()
        {
            // not going to add to global scope as this is only for translating existing data.
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(["contentitem|all"]);
                }
                var query = @$"SELECT {nameof(WebPageFields.WebPageItemID)},{nameof(WebPageFields.WebPageItemGUID)},{nameof(WebPageFields.WebPageItemName)},{nameof(WebPageFields.WebPageItemTreePath)},WebPageItemWebsiteChannelID,WebPageItemContentItemID FROM CMS_WebPageItem
            inner join CMS_ContentItem on ContentItemID = WebPageItemContentItemID";
                var items = (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>()
                    .Select(x => new PageIdentityTempValues((int)x[nameof(WebPageFields.WebPageItemID)], (Guid)x[nameof(WebPageFields.WebPageItemGUID)], (string)x[nameof(WebPageFields.WebPageItemName)], (string)x[nameof(WebPageFields.WebPageItemTreePath)], (int)x["WebPageItemWebsiteChannelID"], (int)x["WebPageItemContentItemID"]));
                return new PageIdentityLookups(
                    PageIdToContentID: items.ToDictionary(key => key.PageID, value => value.ContentID),
                    PageNameToContentID: items.ToDictionary(key => key.PageName.ToLowerInvariant(), value => value.ContentID),
                    PathChannelKeyToContentID: items.ToDictionary(key => $"{key.Path}|{key.ChannelID}".ToLowerInvariant(), value => value.ContentID),
                    PageGuidToContentID: items.ToDictionary(key => key.PageGuid, value => value.ContentID)
                    );
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetWebpageTranslationsLookup"));
        }


        #endregion

        #region "Records for holding data"

        private record TagLookup(Dictionary<Guid, int> TagGuidToTagID, Dictionary<string, int> TagNameToTagId);

        private record TaxonomyLookup(Dictionary<Guid, int> TaxonomyGuidToTaxonomyID, Dictionary<string, int> TaxonomyNameToTaxonomyId);

        private record ContentTypeTaxonomyConfigurations(List<TaxonomyFieldIdentifier> FieldIdentifiers, bool PreCache);

        private record PageIdentityLookups(Dictionary<int, int> PageIdToContentID, Dictionary<string, int> PageNameToContentID, Dictionary<string, int> PathChannelKeyToContentID, Dictionary<Guid, int> PageGuidToContentID);

        private record PageIdentityTempValues(int PageID, Guid PageGuid, string PageName, string Path, int ChannelID, int ContentID);

        #endregion
    }
}
