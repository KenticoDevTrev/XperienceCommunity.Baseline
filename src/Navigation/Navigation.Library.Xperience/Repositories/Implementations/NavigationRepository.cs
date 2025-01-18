using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CSharpFunctionalExtensions;
using MVCCaching;
using Navigation.Enums;
using Navigation.Models;
using CMS.Websites.Routing;
using NavigationPageType = Generic.Navigation;
using RelationshipsExtended;
using XperienceCommunity.QueryExtensions.ContentItems;
using Kentico.Content.Web.Mvc;
using Core.Enums;
using XperienceCommunity.RelationshipsExtended.Extensions;
using XperienceCommunity.RelationshipsExtended.Services;
using Navigation.XbyK.Models;
using CMS.DataEngine;
using Core.Models;
using Core.Services;
using Navigation.Services;
using Generic;
using Core.Repositories;
using XperienceCommunity.MemberRoles.Services;
using XperienceCommunity.ChannelSettings.Repositories;
using System.Linq;

namespace Navigation.Repositories.Implementations
{
    public class NavigationRepository(IContentQueryExecutor contentQueryExecutor,
                                      ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
                                      IProgressiveCache progressiveCache,
                                      ICacheRepositoryContext cacheRepositoryContext,
                                      IWebsiteChannelContext websiteChannelContext,
                                      IRelationshipExtendedHelper relationshipExtendedHelper,
                                      IMemberAuthorizationFilter memberAuthorizationFilter,
                                      IDynamicNavigationRepository dynamicNavigationRepository,
                                      IIdentityService identityService,
                                      IContentItemLanguageMetadataRepository contentItemLanguageMetadataRepository,
                                      ICategoryCachedRepository categoryCachedRepository,
                                      IChannelCustomSettingsRepository channelCustomSettingsRepository) : INavigationRepository, ISecondaryNavigationService
    {
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;
        private readonly IRelationshipExtendedHelper _relationshipExtendedHelper = relationshipExtendedHelper;
        private readonly IMemberAuthorizationFilter _memberAuthorizationFilter = memberAuthorizationFilter;
        private readonly IDynamicNavigationRepository _dynamicNavigationRepository = dynamicNavigationRepository;
        private readonly IIdentityService _identityService = identityService;
        private readonly IContentItemLanguageMetadataRepository _contentItemLanguageMetadataRepository = contentItemLanguageMetadataRepository;
        private readonly ICategoryCachedRepository _categoryCachedRepository = categoryCachedRepository;
        private readonly IChannelCustomSettingsRepository _channelCustomSettingsRepository = channelCustomSettingsRepository;

        public Task<string> GetAncestorPathAsync(string path, int levels, bool levelIsRelative = true, int minAbsoluteLevel = 2)
        {
            string[] pathParts = EnsureSinglePath(path).Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (levelIsRelative) {
                // Handle minimum absolute level
                if ((pathParts.Length - levels + 1) < minAbsoluteLevel) {
                    levels -= minAbsoluteLevel - (pathParts.Length - levels + 1);
                }
                return Task.FromResult(GetParentPath(EnsureSinglePath(path), levels));
            }

            // Since first 'item' in path is actually level 2, reduce level by 1 to match counts
            levels--;
            if (pathParts.Length > levels) {
                return Task.FromResult("/" + string.Join("/", pathParts.Take(levels)));
            } else {
                return Task.FromResult(EnsureSinglePath(path));
            }
        }

        public Task<string> GetAncestorPathAsync(Guid nodeGuid, int levels, bool levelIsRelative = true, int minAbsoluteLevel = 2) => GetAncestorPathAsync(nodeGuid.ToTreeIdentity(), levels, levelIsRelative, minAbsoluteLevel);

        public Task<string> GetAncestorPathAsync(int nodeID, int levels, bool levelIsRelative = true, int minAbsoluteLevel = 2) => GetAncestorPathAsync(nodeID.ToTreeIdentity(), levels, levelIsRelative, minAbsoluteLevel);

        public async Task<string> GetAncestorPathAsync(TreeIdentity treeIdentity, int levels, bool levelIsRelative = true, int minAbsoluteLevel = 2)
        {
            if ((await treeIdentity.GetOrRetrievePathChannelLookup(_identityService)).TryGetValue(out var pathChannel)) {
                return await GetAncestorPathAsync(pathChannel.Path, levels, levelIsRelative, minAbsoluteLevel);
            }
            return "/";
        }

        public async Task<IEnumerable<NavigationItem>> FilterAndConvertIWebPageContentQueryDataContainerItems(IEnumerable<IWebPageContentQueryDataContainer> webPageContentQueryDataContainers)
        {
            var filteredPageItems = await _memberAuthorizationFilter.RemoveUnauthorizedItems(webPageContentQueryDataContainers);

            var hierarchy = WebPageListToHierarchyWebPagesAsync(filteredPageItems);

            var navigation = await HierarchyToNavigationItems(hierarchy, [], []);

            return navigation;
        }

        public async Task<IEnumerable<NavigationItem>> GetNavItemsAsync(Maybe<string> navPath, IEnumerable<string>? navTypes = null)
        {
            var navPageItems = await GetNavigationItemsAsync(navPath, navTypes ?? []);

            var filteredPageItems = await _memberAuthorizationFilter.RemoveUnauthorizedItems(navPageItems.NavItems);

            // Filter automatic items
            var automaticItemContentWebPageGuidsAllowed = (await _memberAuthorizationFilter.RemoveUnauthorizedItems(navPageItems.LinkedPagesByWebpageGuid.Select(x => x.Value))).Select(x => x.WebPageItemGUID);
            var filteredAutomaticItems = navPageItems.LinkedPagesByWebpageGuid.Where(x => automaticItemContentWebPageGuidsAllowed.Contains(x.Value.WebPageItemGUID)).ToDictionary();

            var hierarchy = NavigationPageListToHierarchyWebPagesAsync(filteredPageItems);

            // Convert to NavigationItems and handle Dynamic calls
            var navigation = await HierarchyToNavigationItems(hierarchy, filteredAutomaticItems, navPageItems.WebPageItemIDToDynamicChildren);

            return navigation;
        }

        public async Task<IEnumerable<NavigationItem>> GetSecondaryNavItemsAsync(string startingPath, PathSelectionEnum pathType = PathSelectionEnum.ChildrenOnly, IEnumerable<string>? pageTypes = null, string? orderBy = null, string? whereCondition = null, int? maxLevel = -1, int? topNumber = -1)
        {
            if (!string.IsNullOrWhiteSpace(whereCondition) || !string.IsNullOrWhiteSpace(orderBy)) {
                throw new NotImplementedException("No longer can use the OrderBy or WhereCondition, please retrieve your own list of IWebPageContentQueryDataContainers and then leverage the ISecondaryNavigationRepository to parse to NavItems. Default order is WebPageItemOrder");
            }

            var navItems = await GetSecondaryNavigationItemsAsync(startingPath, pathType, pageTypes, maxLevel, topNumber);

            return await FilterAndConvertIWebPageContentQueryDataContainerItems(navItems);
        }

        private async Task<List<NavigationItem>> HierarchyToNavigationItems(IEnumerable<HierarchyWebPage> navigationItems, Dictionary<Guid, IWebPageContentQueryDataContainer> filteredAutomaticItems, Dictionary<int, IEnumerable<NavigationItem>> webPageItemIDToDynamicChildren)
        {
            var items = new List<NavigationItem>();
            foreach (var navigationItem in navigationItems) {

                // Get children so can add
                var children = await HierarchyToNavigationItems(navigationItem.Children, filteredAutomaticItems, webPageItemIDToDynamicChildren);

                // check if automatic or not
                var cultureGuid = Maybe<Guid>.None;
                var cultureId = Maybe<int>.None;
                var linkText = Maybe<string>.None;

                if (navigationItem.NavPage.TryGetValue(out var navPage)) {
                    var dynamicItems = navPage.IsDynamic && webPageItemIDToDynamicChildren.TryGetValue(navPage.SystemFields.WebPageItemID, out var dynamicChildren) ? dynamicChildren : [];
                    if (navPage.NavigationType.Equals("automatic", StringComparison.OrdinalIgnoreCase) && navPage.NavigationWebPageItemGuid.TryGetFirst(out var automaticPageWebPageGuid)) {
                        if (filteredAutomaticItems.TryGetValue(automaticPageWebPageGuid.WebPageGuid, out var automaticItem)) {
                            try {
                                linkText = automaticItem.GetValue<string>(nameof(IBaseMetadata.MetaData_PageName));
                            } catch (Exception) {

                            }
                            if ((await _contentItemLanguageMetadataRepository.GetOptimizedContentItemLanguageMetadata(automaticItem, true, true)).TryGetValue(out var langMetadata)) {
                                cultureGuid = langMetadata.ContentItemLanguageMetadataGUID;
                                cultureId = langMetadata.ContentItemLanguageMetadataID;
                                if (linkText.HasNoValue) {
                                    linkText = langMetadata.ContentItemLanguageMetadataDisplayName;
                                }
                            }

                            items.Add(new NavigationItem(linkText.GetValueOrDefault(automaticItem.ContentItemName)) {
                                LinkHref = $"/{automaticItem.WebPageUrlPath}",
                                LinkContentCultureGuid = cultureGuid,
                                LinkContentCultureID = cultureId,
                                LinkPageID = automaticItem.WebPageItemID,
                                LinkPageGuid = automaticItem.WebPageItemGUID,
                                LinkPagePath = automaticItem.WebPageItemTreePath,
                                Children = children.Union(dynamicItems)
                            });
                        } else {
                            // Automatic item not found or filtered out, skip
                            continue;
                        }
                    } else {
                        items.Add(new NavigationItem(navPage.NavigationLinkText) {
                            LinkHref = navPage.NavigationLinkUrl,
                            LinkContentCultureGuid = cultureGuid,
                            LinkContentCultureID = cultureId,
                            LinkPageID = navPage.SystemFields.WebPageItemID,
                            LinkPageGuid = navPage.SystemFields.WebPageItemGUID,
                            LinkPagePath = navPage.SystemFields.WebPageItemTreePath,
                            LinkAlt = navPage.NavigationLinkAlt,
                            LinkTarget = navPage.NavigationLinkTarget,
                            LinkCSSClass = navPage.NavigationLinkCSS,
                            LinkOnClick = navPage.NavigationLinkOnClick,
                            Children = children.Union(dynamicItems),
                            IsMegaMenu = navPage.NavigationIsMegaMenu
                        });
                    }

                } else if (navigationItem.OtherPage.TryGetValue(out var otherPage)) {

                    try {
                        linkText = otherPage.GetValue<string>(nameof(IBaseMetadata.MetaData_PageName));
                    } catch (Exception) {

                    }
                    if ((await _contentItemLanguageMetadataRepository.GetOptimizedContentItemLanguageMetadata(otherPage, true, true)).TryGetValue(out var langMetadata)) {
                        cultureGuid = langMetadata.ContentItemLanguageMetadataGUID;
                        cultureId = langMetadata.ContentItemLanguageMetadataID;
                        if (linkText.HasNoValue) {
                            linkText = langMetadata.ContentItemLanguageMetadataDisplayName;
                        }
                    }

                    items.Add(new NavigationItem(linkText.GetValueOrDefault(otherPage.ContentItemName)) {
                        LinkHref = $"/{otherPage.WebPageUrlPath}",
                        LinkContentCultureGuid = cultureGuid,
                        LinkContentCultureID = cultureId,
                        LinkPageID = otherPage.WebPageItemID,
                        LinkPageGuid = otherPage.WebPageItemGUID,
                        LinkPagePath = otherPage.WebPageItemTreePath,
                        Children = children
                    });
                }
            }
            return items;
        }

        #region "Helpers"

        private async Task<IEnumerable<IWebPageContentQueryDataContainer>> GetSecondaryNavigationItemsAsync(string startingPath, PathSelectionEnum pathType = PathSelectionEnum.ChildrenOnly, IEnumerable<string>? pageTypes = null, int? maxLevel = -1, int? topNumber = -1)
        {
            var topN = topNumber ?? -1;
            var nestingLevel = maxLevel ?? -1;
            startingPath = $"/{startingPath.Trim('%').Trim('/')}";
            var pageTypeEnum = pathType switch {
                PathSelectionEnum.ParentOnly => PathTypeEnum.Explicit,
                PathSelectionEnum.ParentAndChildren => PathTypeEnum.Section,
                PathSelectionEnum.ChildrenOnly => PathTypeEnum.Children,
                _ => PathTypeEnum.Children
            };

            var pathMatch = pathType switch {
                PathSelectionEnum.ParentOnly => PathMatch.Single(startingPath),
                PathSelectionEnum.ParentAndChildren => PathMatch.Section(startingPath, nestingLevel),
                PathSelectionEnum.ChildrenOnly => PathMatch.Children(startingPath, nestingLevel),
                _ => PathMatch.Children(startingPath, nestingLevel)
            };
            var builder = _cacheDependencyBuilderFactory.Create()
                .Navigation(true)
                .WebPagePath(startingPath, pageTypeEnum);

            // Default to site settings if available
            var pageTypesToSelect = pageTypes != null && pageTypes.Any() ? pageTypes.ToArray() : (await _channelCustomSettingsRepository.GetSettingsModel<NavigationChannelSettings>()).NavigationPageTypes.SplitAndRemoveEntries();

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                var contentTypeIds = new List<int>();
                if (pageTypesToSelect != null && pageTypesToSelect.Any()) {
                    contentTypeIds = (await DataClassInfoProvider.GetClasses()
                    .WhereIn(nameof(DataClassInfo.ClassName), pageTypesToSelect.ToArray())
                    .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), "Website")
                    .Columns(nameof(DataClassInfo.ClassID))
                    .GetEnumerableTypedResultAsync())
                    .Select(x => x.ClassID).ToList();
                } 

                // Include content type fields if there is an order or where condition as it may be part of it.
                var navQueryBuilder = new ContentItemQueryBuilder().ForContentTypes(query => query
                            .ForWebsite(_websiteChannelContext.WebsiteChannelName, pathMatch, true)
                        //.WithWebPageData(includeUrlPath: true)
                        )
                        .Parameters(query => {
                            query.Where(where =>
                                where.If(contentTypeIds.Count != 0, where => where.WhereIn(nameof(ContentItemFields.ContentItemContentTypeID), contentTypeIds))
                            );
                            if (topN > 0) {
                                query.TopN(topN);
                            }
                            query.OrderBy(nameof(WebPageFields.WebPageItemOrder));
                        })
                    .WithCultureContext(_cacheRepositoryContext);
                return await _contentQueryExecutor.GetWebPageResult(navQueryBuilder, x => x, new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext));
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetSecondaryNavigationItemsAsync", _cacheRepositoryContext.GetCacheKey(), startingPath, pathType.ToString(), _websiteChannelContext.WebsiteChannelName, string.Join(",", pageTypesToSelect.GetValueOrDefault([])), nestingLevel, topN));
        }

        private async Task<NavItemsAndJoinedDocs> GetNavigationItemsAsync(Maybe<string> navPath, IEnumerable<string> navTypes)
        {
            var builder = _cacheDependencyBuilderFactory.Create()
                .Navigation(true);

            if (navPath.TryGetValue(out var pathForBuilder)) {
                builder.WebPagePath(pathForBuilder.Trim('%'), PathTypeEnum.Section);
            }

            // Convert Navigation Tag Names to Guids
            var navGroupings = _categoryCachedRepository.GetCategoryIdentifiertoCategoryCached(navTypes.Select(x => x.ToObjectIdentity())).Select(x => x.CategoryGuid);

            var results = await _progressiveCache.LoadAsync(async cs => {
                var additionalDependencies = _cacheDependencyBuilderFactory.Create(false);

                // Get NavigationPageType Items
                var queryBuilder = new ContentItemQueryBuilder().ForContentType(NavigationPageType.CONTENT_TYPE_NAME, query => query
                       .If(navPath.TryGetValueNonEmpty(out var navPathVal),
                            queryForPath => queryForPath.ForWebsite(_websiteChannelContext.WebsiteChannelName, PathMatch.Section(navPathVal), includeUrlPath: true),
                            queryWithoutPath => queryWithoutPath.ForWebsite(_websiteChannelContext.WebsiteChannelName, includeUrlPath: true)
                       )
                       .If(navGroupings.Any(), query => query.Where(where => where.WhereContainsTags(nameof(NavigationPageType.NavigationGroups), navGroupings)))
                       .OrderByWebpageItemOrder()
                       )
                       .WithCultureContext(_cacheRepositoryContext);

                var navItems = await _contentQueryExecutor.GetMappedWebPageResult<NavigationPageType>(queryBuilder, new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext));

                // Retrieve related items
                var automaticNavWebPageGuids = navItems.Where(x => x.NavigationType.Equals("automatic", StringComparison.OrdinalIgnoreCase)).SelectMany(x => x.NavigationWebPageItemGuid?.Select(x => x.WebPageGuid) ?? []);
                var additionalItemDictionary = new Dictionary<Guid, IWebPageContentQueryDataContainer>();
                additionalDependencies.WebPages(automaticNavWebPageGuids);
                if (automaticNavWebPageGuids.Any()) {
                    var automaticItemsQueryBuilder = new ContentItemQueryBuilder().ForContentTypes(query => query
                            .ForWebsite(_websiteChannelContext.WebsiteChannelName, includeUrlPath: true)
                        )
                        .Parameters(query => query
                            .Where(where => where.WhereIn(nameof(WebPageFields.WebPageItemGUID), automaticNavWebPageGuids))
                        )
                        .WithCultureContext(_cacheRepositoryContext);
                    var relatedNavItems = await _contentQueryExecutor.GetWebPageResult(automaticItemsQueryBuilder, x => x, new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext));
                    additionalItemDictionary = relatedNavItems.ToDictionary(key => key.WebPageItemGUID, value => value);
                }

                if (cs.Cached) {
                    cs.CacheDependency = builder.AddKeys(additionalDependencies.GetKeys()).GetCMSCacheDependency();
                }

                return new DTOWithDependencies<NavItemsAndJoinedDocs>(new NavItemsAndJoinedDocs(navItems, additionalItemDictionary, []), additionalDependencies.GetKeys().ToList());

            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetNavigationItemsAsync", _cacheRepositoryContext.GetCacheKey(), navPath.GetValueOrDefault(string.Empty), string.Join(",", navGroupings.GetValueOrDefault([]))));

            // Add any dependencies found during retrieval.
            builder.AddKeys(results.AdditionalDependencies);

            // Next handle Dynamic Nav Items
            var webPageItemIDToDynamicNavItems = new Dictionary<int, IEnumerable<NavigationItem>>();
            var dynamicNavs = results.Result.NavItems.Where(x => x.IsDynamic && !string.IsNullOrWhiteSpace(x.DynamicCodeName)).Select(x => x);
            foreach (var dynamicNav in dynamicNavs) {
                var dynamicNavItems = await _dynamicNavigationRepository.GetDynamicNavigation(dynamicNav.DynamicCodeName);
                webPageItemIDToDynamicNavItems.Add(dynamicNav.SystemFields.WebPageItemID, dynamicNavItems);
            }

            return results.Result with { WebPageItemIDToDynamicChildren = webPageItemIDToDynamicNavItems };
        }


        private static List<HierarchyWebPage> NavigationPageListToHierarchyWebPagesAsync(IEnumerable<NavigationPageType> filteredPageItems)
        {
            var webPageItemIDToHierarchyWebPage = new Dictionary<int, HierarchyWebPage>();

            // populate ParentNodeIDToTreeNode
            foreach (var webPageItem in filteredPageItems) {
                webPageItemIDToHierarchyWebPage.Add(webPageItem.SystemFields.WebPageItemID, new HierarchyWebPage(webPageItem));
            }
            return SetHierarchy(webPageItemIDToHierarchyWebPage);
        }

        private static List<HierarchyWebPage> WebPageListToHierarchyWebPagesAsync(IEnumerable<IWebPageContentQueryDataContainer> filteredPageItems)
        {
            var webPageItemIDToHierarchyWebPage = new Dictionary<int, HierarchyWebPage>();

            // populate ParentNodeIDToTreeNode
            foreach (var webPageItem in filteredPageItems) {
                webPageItemIDToHierarchyWebPage.Add(webPageItem.WebPageItemID, new HierarchyWebPage(webPageItem));
            }
            return SetHierarchy(webPageItemIDToHierarchyWebPage);
        }

        private static List<HierarchyWebPage> SetHierarchy(Dictionary<int, HierarchyWebPage> webPageItemIDToHierarchyWebPage)
        {
            var hierarchyWebPages = new List<HierarchyWebPage>();

            // Populate the Children of the TypedResults
            foreach (var webPageItemKey in webPageItemIDToHierarchyWebPage.Keys) {
                var webPageItem = webPageItemIDToHierarchyWebPage[webPageItemKey];
                var parentWebPageItemID = webPageItem.NavPage.AsNullableValue()?.SystemFields.WebPageItemParentID ?? webPageItem.OtherPage.AsNullableValue()?.WebPageItemParentID ?? 0;
                // If parent exists, add as child, otherwise add to top level
                if (webPageItemIDToHierarchyWebPage.TryGetValue(parentWebPageItemID, out HierarchyWebPage? value)) {
                    value.Children.Add(webPageItem);
                } else {
                    hierarchyWebPages.Add(webPageItem);
                }
            }
            return hierarchyWebPages;
        }


        public static string EnsureSinglePath(string path)
        {
            if (string.IsNullOrEmpty(path)) {
                return path;
            }

            path = path.TrimEnd(['%']);
            if (path != "/") {
                path = "/" + path.Trim(['/']);
            }

            return path;
        }

        public static string GetParentPath(string path, int parentLevel)
        {
            while (parentLevel > 0 && path != "/") {
                path = DataHelper.GetParentPath(path);
                parentLevel--;
            }

            return path;
        }
        private record NavItemsAndJoinedDocs(IEnumerable<NavigationPageType> NavItems, Dictionary<Guid, IWebPageContentQueryDataContainer> LinkedPagesByWebpageGuid, Dictionary<int, IEnumerable<NavigationItem>> WebPageItemIDToDynamicChildren);



        #endregion
    }
}
