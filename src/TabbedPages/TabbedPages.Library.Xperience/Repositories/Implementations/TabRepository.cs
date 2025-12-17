using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Core.Models;
using Core.Services;
using Generic;
using Kentico.Content.Web.Mvc;
using MVCCaching;
using TabbedPages.Models;
using XperienceCommunity.MemberRoles.Models;
using XperienceCommunity.MemberRoles.Repositories;
using XperienceCommunity.MemberRoles.Services;

namespace TabbedPages.Repositories.Implementations
{
    public class TabRepository(IIdentityService identityService,
        ICacheDependencyScopedBuilderFactory cacheDependencyBuilderFactory,
        IProgressiveCache progressiveCache,
        ICacheRepositoryContext cacheRepositoryContext,
        IContentQueryExecutor contentQueryExecutor,
        IMemberPermissionConfigurationService memberPermissionConfigurationService,
        IMemberAuthorizationFilter memberAuthorizationFilter,
        IWebsiteChannelContext websiteChannelContext) : ITabRepository
    {
        private readonly IIdentityService _identityService = identityService;
        private readonly ICacheDependencyScopedBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly IMemberPermissionConfigurationService _memberPermissionConfigurationService = memberPermissionConfigurationService;
        private readonly IMemberAuthorizationFilter _memberAuthorizationFilter = memberAuthorizationFilter;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;

        public async Task<IEnumerable<TabItem>> GetTabsAsync(TreeIdentity parentIdentity)
        {
            // Need both path and the parent for dependencies and lookup
            if(!(await parentIdentity.GetOrRetrievePageID(_identityService)).TryGetValue(out var parentWebPageID)
                || !(await parentIdentity.GetOrRetrievePathChannelLookup(_identityService)).TryGetValue(out var pathChannelLookup)) {
                return [];
            }

            // Add children to the dependencies
            var builder = _cacheDependencyBuilderFactory.Create()
                .WebPagePath(pathChannelLookup.Path, PathTypeEnum.Children);
            var websiteChannelName = _websiteChannelContext.WebsiteChannelName;
            var childTabs = await _progressiveCache.LoadAsync(async cs => {
                var innerBuilder = _cacheDependencyBuilderFactory.Create(false);

                var webpageQuery = new ContentItemQueryBuilder().ForContentType(Tab.CONTENT_TYPE_NAME, query => query    
                    .ForWebsite(websiteChannelName, includeUrlPath: false)
                    .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemParentID), parentWebPageID))
                ).WithCultureContext(_cacheRepositoryContext);

                var tabPages = await _contentQueryExecutor.GetMappedWebPageResult<Tab>(webpageQuery, new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext));

                innerBuilder.WebPages(tabPages);

                if(cs.Cached) {
                    cs.CacheDependency = innerBuilder.AddKeys(builder.GetKeys()).GetCMSCacheDependency();
                }

                // Cast to DTO with permissions
                var dtoWithPermissions = tabPages.Select(x => _memberPermissionConfigurationService.WrapGenericDTOWithMemberPermissions(new TabItem(x.TabName, x.SystemFields.WebPageItemID), x));
                return new DTOWithDependencies<IEnumerable<DTOWithMemberPermissionConfiguration<TabItem>>>(dtoWithPermissions, additionalDependencies: [.. builder.GetKeys()]);
            }, new CacheSettings(60, "GetTabs", websiteChannelName, parentWebPageID, _cacheRepositoryContext.ToCacheNameIdentifier()));

            // Add keys to 
            builder.AddKeys(childTabs.AdditionalDependencies);

            // Filter out tabs not authorized
            var filteredTabs = await _memberAuthorizationFilter.RemoveUnauthorizedItems(childTabs.Result);

            // return TabItems
            return filteredTabs.Select(x => x.Model);

        }

        [Obsolete("Use GetTabsAsync(TreeIdentity), will not be implemented in Xperience by Kentico")]
        public Task<IEnumerable<TabItem>> GetTabsAsync(NodeIdentity parentIdentity)
        {
            throw new NotImplementedException("Use GetTabsAsync(TreeIdentity), will not be implemented in Xperience by Kentico");
        }
    }

    
}
