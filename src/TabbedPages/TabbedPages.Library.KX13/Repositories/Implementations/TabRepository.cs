using CMS.Base;

namespace TabbedPages.Repositories.Implementations
{
    public class TabRepository(
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory,
        ICacheRepositoryContext _cacheRepositoryContext,
        IProgressiveCache _progressiveCache,
        IIdentityService _identityService,
        ISiteService _siteService,
        ISiteRepository _siteRepository) : ITabRepository
    {
        public async Task<IEnumerable<TabItem>> GetTabsAsync(TreeIdentity parentIdentity)
        {
            // This implementation uses the NodeAliasPath, so if it's missing then we need to get it.
            if(parentIdentity.PathChannelLookup.HasNoValue)
            {
                var identityResult = await _identityService.HydrateTreeIdentity(parentIdentity);
                if(identityResult.IsFailure)
                {
                    return [];
                }
            }

            // Should never be Maybe.None but just in case...
            if(!parentIdentity.PathChannelLookup.TryGetValue(out var pathChannelLookup))
            {
                return [];
            }

            string path = pathChannelLookup.Path;
            string siteName = _siteRepository.ChannelNameById(pathChannelLookup.ChannelId.GetValueOrDefault(_siteService.CurrentSite.SiteID));

            var builder = _cacheDependencyBuilderFactory.Create(siteName)
                .PagePath(path, PathTypeEnum.Children);

            var nodes = (await _progressiveCache.LoadAsync(async cs =>
            {
                var tabs = await new DocumentQuery<Tab>()
                    .WithCulturePreviewModeContext(_cacheRepositoryContext)
                    .Path(path, PathTypeEnum.Children)
                    .NestingLevel(1)
                    .OnSite(siteName)
                    .ColumnsSafe([
                    nameof(Tab.DocumentID),
                    nameof(Tab.TabName),
                    nameof(Tab.DocumentCulture)
                    ])
                    .OrderBy(nameof(TreeNode.NodeLevel), nameof(TreeNode.NodeOrder))
                    .GetEnumerableTypedResultAsync();

                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                return tabs;
            }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetTabsAsync", parentIdentity.GetCacheKey()))).ToList();

            return nodes.Select(x => x.ToTabItem());
        }

        [Obsolete("Use GetTabsAsync(TreeIdentity parentIdentity)")]
        public Task<IEnumerable<TabItem>> GetTabsAsync(NodeIdentity parentIdentity) => GetTabsAsync(parentIdentity.ToTreeIdentity());
    }
}
namespace CMS.DocumentEngine.Types.Generic
{
    public static class TabExtensions
    {
        public static TabItem ToTabItem(this Tab value)
        {

            return new TabItem(
                name: value.TabName,
                pageID: value.DocumentID);
        }
    }
}
