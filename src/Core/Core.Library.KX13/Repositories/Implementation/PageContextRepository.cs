using Microsoft.AspNetCore.Http;
using CMS.Base.Internal;
using Kentico.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

namespace Core.Repositories.Implementation
{
    [AutoDependencyInjection]
    public class PageContextRepository(
        IPageDataContextRetriever _pageDataContextRetriever,
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory,
        IHttpContextAccessor _httpContextAccessor,
        IProgressiveCache _progressiveCache,
        ICacheRepositoryContext _cacheRepositoryContext
            ) : IPageContextRepository
    {
        public Task<Result<PageIdentity>> GetCurrentPageAsync()
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            if (_pageDataContextRetriever.TryRetrieve<TreeNode>(out var currentPage))
            {
                builder.Node(currentPage.Page.NodeID);
                return Task.FromResult((Result<PageIdentity>)currentPage.Page.ToPageIdentity());
            }
            else
            {
                return Task.FromResult(Result.Failure<PageIdentity>("Could not resolve page"));
            }
        }

        public async Task<Result<PageIdentity<T>>> GetCurrentPageAsync<T>()
        {
            var pageNode = await GetCurrentTreeNodeInternal();
            if(pageNode.TryGetValue(out var node) && node is T tNode) {
                return new PageIdentity<T>(tNode, node.ToPageIdentity());
            }
            return Result.Failure<PageIdentity<T>>("No current page, or the page is not the type you requested");
        }

        [Obsolete("Use GetPageAsync(TreeIdentity)")]
        public Task<Result<PageIdentity>> GetPageAsync(NodeIdentity identity) => GetPageAsync(identity.ToTreeIdentity());

        [Obsolete("Use GetPageAsync(TreeCultureIdentity) if possible (requires using Node and cultures only).  If you MUST use DocumentID/DocumentGuid, use this and ignore the warning, but on transition to Xperience by Kentico will need to refactor based on how your data model gets migrated.")]
        public async Task<Result<PageIdentity>> GetPageAsync(DocumentIdentity identity)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            var currentPage = await GetCurrentPageAsync();

            if (identity.DocumentId.TryGetValue(out var documentId))
            {
                if (currentPage.TryGetValue(out var currentPageVal) && currentPageVal.DocumentID == documentId)
                {
                    return currentPage;
                }
                builder.Page(documentId);
                return await _progressiveCache.LoadAsync(async cs =>
                {
                    if (cs.Cached)
                    {
                        cs.CacheDependency = builder.GetCMSCacheDependency();
                    }

                    var result = await DocumentHelper.GetDocuments()
                    .WhereEquals(nameof(TreeNode.DocumentID), documentId)
                    .WithCulturePreviewModeContext(_cacheRepositoryContext)
                    .IncludePageIdentityColumns()
                    .TopN(1)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .GetEnumerableTypedResultAsync();

                    return result.Any() ? result.First().ToPageIdentity() : Result.Failure<PageIdentity>($"Could not find a page with DocumentID {documentId}");

                }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetPageAsyncByDocumentID", documentId));
            }
            if (identity.DocumentGuid.TryGetValue(out var documentGuid))
            {
                if (currentPage.TryGetValue(out var currentPageVal) && currentPageVal.DocumentGUID == documentGuid)
                {
                    return currentPage;
                }
                builder.Page(documentGuid);
                return await _progressiveCache.LoadAsync(async cs =>
                {
                    if (cs.Cached)
                    {
                        cs.CacheDependency = builder.GetCMSCacheDependency();
                    }

                    var result = await DocumentHelper.GetDocuments()
                    .WhereEquals(nameof(TreeNode.DocumentGUID), documentGuid)
                    .WithCulturePreviewModeContext(_cacheRepositoryContext)
                    .IncludePageIdentityColumns()
                    .TopN(1)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .GetEnumerableTypedResultAsync();

                    return result.Any() ? result.First().ToPageIdentity() : Result.Failure<PageIdentity>($"Could not find a page with DocumentGuid {documentGuid}");

                }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetPageAsyncByDocumentGuid", documentGuid));
            }
            if (identity.NodeAliasPathAndMaybeCultureAndSiteId.TryGetValue(out var nodeAliasPathAndMaybeCultureAndSiteId))
            {
                if (currentPage.TryGetValue(out var currentPageVal) &&
                    currentPageVal.Path.Equals(nodeAliasPathAndMaybeCultureAndSiteId.Item1, StringComparison.OrdinalIgnoreCase)
                    && (!nodeAliasPathAndMaybeCultureAndSiteId.Item3.TryGetValue(out var siteID) || currentPageVal.NodeSiteID == siteID)
                    && (!nodeAliasPathAndMaybeCultureAndSiteId.Item2.TryGetValue(out var culture) || currentPageVal.Culture.Equals(culture, StringComparison.OrdinalIgnoreCase))
                    )
                {
                    return currentPage;
                }
                builder.PagePath(nodeAliasPathAndMaybeCultureAndSiteId.Item1);
                return await _progressiveCache.LoadAsync(async cs =>
                {
                    if (cs.Cached)
                    {
                        cs.CacheDependency = builder.GetCMSCacheDependency();
                    }

                    var result = await DocumentHelper.GetDocuments()
                    .Path(nodeAliasPathAndMaybeCultureAndSiteId.Item1, PathTypeEnum.Single)
                    .If(nodeAliasPathAndMaybeCultureAndSiteId.Item2.TryGetValue(out var culture), query => query.Culture(culture))
                    .If(nodeAliasPathAndMaybeCultureAndSiteId.Item3.TryGetValue(out var nodeSiteID), query => query.OnSite(nodeSiteID, false))
                    .WithCulturePreviewModeContext(_cacheRepositoryContext)
                    .IncludePageIdentityColumns()
                    .TopN(1)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .GetEnumerableTypedResultAsync();

                    return result.Any() ? result.First().ToPageIdentity() : Result.Failure<PageIdentity>($"Could not find a page with NodeAliaPath {nodeAliasPathAndMaybeCultureAndSiteId.Item1}");

                }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetPageAsyncByDocumentNodeAliasPathAndCultureSite", nodeAliasPathAndMaybeCultureAndSiteId.Item1, nodeAliasPathAndMaybeCultureAndSiteId.Item2.GetValueOrDefault("SiteCulture"), nodeAliasPathAndMaybeCultureAndSiteId.Item3.GetValueOrDefault(Maybe.None)));
            }
            return Result.Failure<PageIdentity>("No identifier was passed");
        }

        public async Task<Result<PageIdentity>> GetPageAsync(TreeIdentity identity)
        {
            var pageNode = await GetPageTreeNodeInternal(identity);
            if(pageNode.TryGetValue(out var node)) {
                return node.ToPageIdentity();
            }
            return Result.Failure<PageIdentity>("No identifier was passed");
        }

        private Task<Result<TreeNode>> GetCurrentTreeNodeInternal()
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            if (_pageDataContextRetriever.TryRetrieve<TreeNode>(out var currentPage)) {
                builder.Node(currentPage.Page.NodeID);
                return Task.FromResult((Result<TreeNode>)currentPage.Page);
            } else {
                return Task.FromResult(Result.Failure<TreeNode>("Could not resolve page"));
            }
        }

        private async Task<Result<TreeNode>> GetPageTreeNodeInternal(TreeIdentity identity)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            var currentPage = await GetCurrentTreeNodeInternal();

            if (identity.PageID.TryGetValue(out var pageId)) {
                if (currentPage.TryGetValue(out var currentPageVal) && currentPageVal.NodeID == pageId) {
                    return currentPage;
                }
                builder.Node(pageId);
                return await _progressiveCache.LoadAsync(async cs => {
                    var result = await DocumentHelper.GetDocuments()
                    .WhereEquals(nameof(TreeNode.NodeID), pageId)
                    .WithCulturePreviewModeContext(_cacheRepositoryContext)
                    .IncludePageIdentityColumns()
                    .TopN(1)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .GetEnumerableTypedResultAsync();

                    return result.Any() ? result.First() : Result.Failure<TreeNode>($"Could not find a page with NodeID {pageId}");

                }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetPageAsyncByNodeID", pageId));
            }
            if (identity.PageGuid.TryGetValue(out var nodeGuid)) {
                if (currentPage.TryGetValue(out var currentPageVal) && currentPageVal.NodeGUID == nodeGuid) {
                    return currentPage;
                }
                builder.Node(nodeGuid);
                return await _progressiveCache.LoadAsync(async cs => {
                    var result = await DocumentHelper.GetDocuments()
                    .WhereEquals(nameof(TreeNode.NodeGUID), nodeGuid)
                    .WithCulturePreviewModeContext(_cacheRepositoryContext)
                    .IncludePageIdentityColumns()
                    .TopN(1)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .GetEnumerableTypedResultAsync();

                    return result.Any() ? result.First() : Result.Failure<TreeNode>($"Could not find a page with NodeGuid {nodeGuid}");

                }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetPageAsyncByNodeGuid", nodeGuid));
            }
            if (identity.PathChannelLookup.TryGetValue(out var pathChannelLookup)) {
                if (currentPage.TryGetValue(out var currentPageVal) && currentPageVal.NodeAliasPath.Equals(pathChannelLookup.Path, StringComparison.OrdinalIgnoreCase)) {
                    return currentPage;
                }
                builder.PagePath(pathChannelLookup.Path);
                return await _progressiveCache.LoadAsync(async cs => {
                    var result = await DocumentHelper.GetDocuments()
                    .Path(pathChannelLookup.Path, PathTypeEnum.Single)
                    .If(pathChannelLookup.ChannelId.TryGetValue(out var nodeSiteID), query => query.OnSite(nodeSiteID, false))
                    .WithCulturePreviewModeContext(_cacheRepositoryContext)
                    .IncludePageIdentityColumns()
                    .TopN(1)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .GetEnumerableTypedResultAsync();

                    return result.Any() ? result.First() : Result.Failure<TreeNode>($"Could not find a page with NodeAliasPath {pathChannelLookup.Path}");

                }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetPageAsyncByNodePath", pathChannelLookup.Path, pathChannelLookup.ChannelId.GetValueOrDefault(0)));
            }
            return Result.Failure<TreeNode>("No identifier was passed");
        }

        private async Task<Result<TreeNode>> GetPageTreeNodeInternal(TreeCultureIdentity identity)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            var currentPage = await GetCurrentTreeNodeInternal();

            if (identity.PageID.TryGetValue(out var pageId)) {
                if (currentPage.TryGetValue(out var currentPageVal) && currentPageVal.NodeID == pageId
                    && currentPageVal.DocumentCulture.Equals(identity.Culture, StringComparison.OrdinalIgnoreCase)) {
                    return currentPage;
                }
                builder.Page(pageId);
                return await _progressiveCache.LoadAsync(async cs => {
                    if (cs.Cached) {
                        cs.CacheDependency = builder.GetCMSCacheDependency();
                    }

                    var result = await DocumentHelper.GetDocuments()
                    .WhereEquals(nameof(TreeNode.NodeID), pageId)
                    .WithCulturePreviewModeContext(_cacheRepositoryContext)
                    .IncludePageIdentityColumns()
                    .TopN(1)
                    .Culture(identity.Culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .GetEnumerableTypedResultAsync();

                    return result.Any() ? result.First() : Result.Failure<TreeNode>($"Could not find a page with NodeID {pageId} and culture {identity.Culture}");

                }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetPageAsyncByPageIDAndCulture", pageId, identity.Culture));
            }
            if (identity.PageGuid.TryGetValue(out var pageGuid)) {
                if (currentPage.TryGetValue(out var currentPageVal) && currentPageVal.NodeGUID == pageGuid
                    && currentPageVal.DocumentCulture.Equals(identity.Culture, StringComparison.OrdinalIgnoreCase)) {
                    return currentPage;
                }
                builder.Page(pageGuid);
                return await _progressiveCache.LoadAsync(async cs => {
                    if (cs.Cached) {
                        cs.CacheDependency = builder.GetCMSCacheDependency();
                    }

                    var result = await DocumentHelper.GetDocuments()
                    .WhereEquals(nameof(TreeNode.NodeGUID), pageGuid)
                    .WithCulturePreviewModeContext(_cacheRepositoryContext)
                    .IncludePageIdentityColumns()
                    .TopN(1)
                    .Culture(identity.Culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .GetEnumerableTypedResultAsync();

                    return result.Any() ? result.First() : Result.Failure<TreeNode>($"Could not find a page with NodeGuid {pageGuid} and culture {identity.Culture}");

                }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetPageAsyncByPageGuidAndCulture", pageGuid, identity.Culture));
            }
            if (identity.PathChannelLookup.TryGetValue(out var pathChannelLookup)) {
                if (currentPage.TryGetValue(out var currentPageVal) &&
                    currentPageVal.NodeAliasPath.Equals(pathChannelLookup.Path, StringComparison.OrdinalIgnoreCase)
                    && (!pathChannelLookup.ChannelId.TryGetValue(out var siteID) || currentPageVal.NodeSiteID == siteID)
                    && currentPageVal.DocumentCulture.Equals(identity.Culture, StringComparison.OrdinalIgnoreCase)
                    ) {
                    return currentPage;
                }
                builder.PagePath(pathChannelLookup.Path);
                return await _progressiveCache.LoadAsync(async cs => {
                    if (cs.Cached) {
                        cs.CacheDependency = builder.GetCMSCacheDependency();
                    }

                    var result = await DocumentHelper.GetDocuments()
                    .Path(pathChannelLookup.Path, PathTypeEnum.Single)
                    .If(pathChannelLookup.ChannelId.TryGetValue(out var channelId), query => query.OnSite(channelId, false))
                    .WithCulturePreviewModeContext(_cacheRepositoryContext)
                    .IncludePageIdentityColumns()
                    .TopN(1)
                    .Culture(identity.Culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .GetEnumerableTypedResultAsync();

                    return result.Any() ? result.First() : Result.Failure<TreeNode>($"Could not find a page with NodeAliaPath {pathChannelLookup.Path} and culture {identity.Culture}");

                }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetPageAsyncByPathAndCultureSite", pathChannelLookup.Path, identity.Culture, pathChannelLookup.ChannelId.GetValueOrDefault(Maybe.None)));
            }
            return Result.Failure<TreeNode>("No identifier was passed");
        }

        public async Task<Result<PageIdentity>> GetPageAsync(TreeCultureIdentity identity)
        {
            var pageNode = await GetPageTreeNodeInternal(identity);
            if (pageNode.TryGetValue(out var node)) {
                return node.ToPageIdentity();
            }
            return Result.Failure<PageIdentity>("No page found");
        }

        public async Task<Result<PageIdentity<T>>> GetPageAsync<T>(TreeIdentity identity)
        {
            var pageNode = await GetPageTreeNodeInternal(identity);
            if (pageNode.TryGetValue(out var node) && node is T tNode) {
                return new PageIdentity<T>(tNode, node.ToPageIdentity());
            }
            return Result.Failure<PageIdentity<T>>("No current page, or the page is not the type you requested");
        }

        public async Task<Result<PageIdentity<T>>> GetPageAsync<T>(TreeCultureIdentity identity)
        {
            var pageNode = await GetPageTreeNodeInternal(identity);
            if (pageNode.TryGetValue(out var node) && node is T tNode) {
                return new PageIdentity<T>(tNode, node.ToPageIdentity());
            }
            return Result.Failure<PageIdentity<T>>("No current page, or the page is not the type you requested");
        }

        public Task<bool> IsEditModeAsync()
        {
            return Task.FromResult(_httpContextAccessor.HttpContext.Kentico().PageBuilder().EditMode);
        }

        public Task<bool> IsPreviewModeAsync()
        {
            return Task.FromResult(_httpContextAccessor.HttpContext.Kentico().Preview().Enabled);
        }
    }
}
