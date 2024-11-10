using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CSharpFunctionalExtensions;
using MVCCaching;
using Navigation.Enums;
using Navigation.Models;
using CMS.Websites.Routing;
using XperienceCommunity.MVCCaching.Implementations;
using NavigationPageType = Generic.Navigation;
using RelationshipsExtended;
using XperienceCommunity.QueryExtensions.ContentItems;
using Kentico.Content.Web.Mvc;
using Core.Enums;



namespace Navigation.Repositories.Implementations
{
    public class NavigationRepository(IContentQueryExecutor contentQueryExecutor,
                                      ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
                                      IProgressiveCache progressiveCache,
                                      ICacheRepositoryContext cacheRepositoryContext,
                                      ILinkedItemsDependencyAsyncRetriever linkedItemsDependencyAsyncRetriever,
                                      IWebsiteChannelContext websiteChannelContext) : INavigationRepository
    {
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly ILinkedItemsDependencyAsyncRetriever _linkedItemsDependencyAsyncRetriever = linkedItemsDependencyAsyncRetriever;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;

        public Task<string> GetAncestorPathAsync(string path, int levels, bool levelIsRelative = true, int minAbsoluteLevel = 2)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAncestorPathAsync(Guid nodeGuid, int levels, bool levelIsRelative = true, int minAbsoluteLevel = 2)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAncestorPathAsync(int nodeID, int levels, bool levelIsRelative = true, int minAbsoluteLevel = 2)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<NavigationItem>> GetNavItemsAsync(Maybe<string> navPath, IEnumerable<string>? navTypes = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<NavigationItem>> GetSecondaryNavItemsAsync(string startingPath, PathSelectionEnum pathType = PathSelectionEnum.ChildrenOnly, IEnumerable<string>? pageTypes = null, string? orderBy = null, string? whereCondition = null, int? maxLevel = -1, int? topNumber = -1)
        {
            throw new NotImplementedException();
        }

        #region "Helpers"

        private async Task<IEnumerable<NavigationPageType>> GetNavigationItemsAsync(Maybe<string> navPath, IEnumerable<string> navTypes)
        {
            var builder = _cacheDependencyBuilderFactory.Create();

            if (navPath.TryGetValue(out var pathForBuilder)) {
                builder.WebPagePath(pathForBuilder.Trim('%'), PathTypeEnum.Section);
            }
            if (navTypes.Any()) {
                builder.ObjectType(ContentItemCategoryInfo.OBJECT_TYPE);
            }

            return await _progressiveCache.LoadAsync(async cs => {
                var queryBuilder = new ContentItemQueryBuilder().ForContentType(NavigationPageType.CONTENT_TYPE_NAME, query => query.OrderBy(new string[] { nameof(WebPageFields.WebPageItemOrder) })
                        .Columns(new string[] {
                        "WebPageItemParentID", "WebPageItemID", nameof(NavigationPageType.NavigationWebPageItemGuid), nameof(ContentItemFields.ContentItemID), nameof(ContentItemFields.ContentItemGUID),
                        nameof(NavigationPageType.NavigationType), nameof(NavigationPageType.NavigationWebPageItemGuid), nameof(NavigationPageType.NavigationLinkText), nameof(NavigationPageType.NavigationLinkTarget), nameof(NavigationPageType.NavigationLinkUrl),
                        nameof(NavigationPageType.NavigationLinkCSS), nameof(NavigationPageType.NavigationLinkOnClick), nameof(NavigationPageType.NavigationLinkAlt), nameof(WebPageFields.WebPageItemTreePath),
                        nameof(NavigationPageType.IsDynamic), nameof(NavigationPageType.DynamicCodeName), nameof(ContentItemFields.ContentItemCommonDataContentLanguageID), "ContentItemLanguageMetadataGUID"
                       })
                       .If(navPath.TryGetValueNonEmpty(out var navPathVal), query => query.Path(navPathVal, PathTypeEnum.Section))
                       );
                // TODO: Re-enable once set
                // .If(navTypes.Any(), query => query.TreeCategoryCondition(navTypes)));

                return await _contentQueryExecutor.GetMappedWebPageResult<NavigationPageType>(queryBuilder, new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext));
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetNavigationItemsAsync", _cacheRepositoryContext.GetCacheKey(), navPath.GetValueOrDefault(string.Empty), string.Join(",", navTypes.GetValueOrDefault(Array.Empty<string>()))));
        }

        #endregion

        /*
        public Task<IEnumerable<BlogItem>> GetBlogs(string path)
        {
            
            // Create the Cache Dependency Builder and set the dependencies
            var builder = CacheDependencyBuilderFactory.Create()
                .WebPagePath(path, PathTypeEnum.Children);

            var results = ProgressiveCache.LoadAsync(async cs => {

                var nestedLevel = 2;

                // Build Query with Linked Items
                var queryBuilder = new ContentItemQueryBuilder()
                    .WithCultureContext(CacheRepositoryContext) // Applys Language Context;
                    // Main item we are calling
                    .ForContentType("Sample.Blog", subqueryParameters => 
                        subqueryParameters.ForWebsite(
                            websiteChannelName: WebsiteChannelContext.WebsiteChannelName,
                            pathMatch: PathMatch.Children(path),
                            includeUrlPath: true
                            )
                            .Columns(nameof(Blog.BlogTitle), nameof(Blog.BlogSummary))
                        // Include linked items to 2 nested levels, which will grab our Author and Address
                        .WithLinkedItems(nestedLevel)
                    )
                    // Good practice to apply columns even to sub items
                    .ForContentType("Sample.Author", subqueryParameters => 
                        subqueryParameters.Columns(nameof(Author.AuthorName))
                    )
                    .ForContentType("Sample.AuthorAddress", subqueryParameters => 
                        subqueryParameters.Columns(nameof(AuthorAddress.AuthorAddressState), nameof(AuthorAddress.AuthorAddressCity))
                    );


                // Apply Preview Mode Context
                var queryOptions = new ContentQueryExecutionOptions()
                    .WithPreviewModeContext(CacheRepositoryContext);

                // Get Items
                var items = await ContentQueryExecutor.GetMappedResult<Blog>(queryBuilder, queryOptions);

                // Get Linked Item Dependencies
                var linkedDependencies = await LinkedItemsDependencyAsyncRetriever.Get(items.Select(x => x.ContentItemID), nestedLevel);

                // Apply cache dependencies, including the additional linked dependencies
                if(cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency(additionalDependencies: linkedDependencies);
                }

                // Optional convert items into a lightweight DTO to be returned
                var blogItems = items.Select(x => x.ToBlogItemDTO()); // ToBlogItemDTO is just an extension method that converts the Xperience Webpage Blog to the generic BlogItem DTO

                // Return the results with a wrapper
                return new DTOWithDependencies<IEnumerable<BlogItem>>(blogItems, linkedDependencies);
            }, new CacheSettings(60, "GetBlogs", path, CacheRepositoryContext.ToCacheNameIdentifier()));

            // Append added dependencies
            builder.AppendDTOWithDependencies(results);

            // Return actual results
            return results.Result;
        }
        */

    }
}
