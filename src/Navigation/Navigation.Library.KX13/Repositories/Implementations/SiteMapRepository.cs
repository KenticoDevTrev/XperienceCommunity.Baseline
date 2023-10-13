using CMS.DataEngine;
using CMS.DocumentEngine.Routing;
using Kentico.Content.Web.Mvc;

namespace Navigation.Repositories.Implementations
{
    [AutoDependencyInjection]
    public class SiteMapRepository : ISiteMapRepository
    {
        private readonly IPageUrlRetriever _pageUrlRetriever;
        private readonly IPageRetriever _pageRetriever;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory;
        private readonly ICacheRepositoryContext _repoContext;

        public SiteMapRepository(IPageUrlRetriever pageUrlRetriever,
            IPageRetriever pageRetriever,
            ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
            ICacheRepositoryContext repoContext)
        {
            _pageUrlRetriever = pageUrlRetriever;
            _pageRetriever = pageRetriever;
            _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
            _repoContext = repoContext;
        }

        public async Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetAsync()
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.PagePath("/", PathTypeEnum.Children);

            // You can implement your own custom data here. 
            // In this example, we will pull from any pages with the "Navigation Item" feature.
            var nodes = await _pageRetriever.RetrieveMultipleAsync(
               query => query
                    .WhereEquals(nameof(TreeNode.DocumentShowInMenu), true)
                    .OrderBy(nameof(TreeNode.NodeLevel), nameof(TreeNode.NodeOrder))
                    .WithPageUrlPaths(),
                cacheSettings => cacheSettings
                    .Dependencies((items, csbuilder) => builder.ApplyDependenciesTo(key => csbuilder.Custom(key)))
                    .Key($"GetSiteMapUrlSetAsync|Custom")
                    .Expiration(TimeSpan.FromMinutes(60))
                    );

            return nodes.Select(x => ConvertToSiteMapUrl(_pageUrlRetriever.Retrieve(x).RelativePath, x.DocumentModifiedWhen));
        }

        public async Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetAsync(SiteMapOptions options)
        {
            // Switch on the type
            if (options is SiteMapOptionsPageBuilderOnly pageBuilderOnlySitemapOptions)
            {
                return await GetSiteMapUrlSetPageBuilderAsync(pageBuilderOnlySitemapOptions);
            }

            // Clean up
            options.Path = DataHelper.GetNotEmpty(options.Path, "/").Replace("%", "");

            var nodes = new List<SitemapNode>();

            if (options.ClassNames.Any())
            {
                foreach (string ClassName in options.ClassNames)
                {
                    if (options.UrlColumnName.TryGetValue(out var urlColumnName))
                    {
                        // Since it's not the specific node, but the page found at that url that we need, we will first get the urls, then cache on getting those items.
                        nodes.AddRange(await GetSiteMapUrlSetForClassWithUrlColumnAsync(options.Path, ClassName, options, urlColumnName));
                    }
                    else
                    {
                        nodes.AddRange(await GetSiteMapUrlSetForClassAsync(options.Path, ClassName, options));
                    }
                }
            }
            else
            {
                nodes.AddRange(await GetSiteMapUrlSetForAllClassAsync(options.Path, options));
            }

            // Clean up, remove any that are not a URL
            nodes.RemoveAll(x => !Uri.IsWellFormedUriString(x.Url, UriKind.Absolute));
            return nodes;
        }


        /// <summary>
        /// Converts the realtive Url and possible Datetime into an SitemapNode with an absolute Url
        /// </summary>
        /// <param name="RelativeURL">The Relative Url</param>
        /// <param name="ModifiedLast">The last modified date</param>
        /// <returns>The SitemapNode</returns>
        private SitemapNode ConvertToSiteMapUrl(string relativeURL, DateTime? modifiedLast)
        {
            string url = URLHelper.GetAbsoluteUrl(relativeURL, RequestContext.CurrentDomain);
            var siteMapItem = new SitemapNode(url);
            if (modifiedLast.HasValue)
            {
                siteMapItem.LastModificationDate = modifiedLast.Value;
            }
            return siteMapItem;
        }

        private async Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetForClassAsync(string path, string className, SiteMapOptions options)
        {
            return (await GetSiteMapUrlSetForClassBase(path, className, options)).Select(x => TreeNodeToSitemapNode(x));
        }

        private async Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetForClassWithUrlColumnAsync(string path, string className, SiteMapOptions options, string urlColumnName)
        {
            var documents = await GetSiteMapUrlSetForClassBase(path, className, options);
            var siteMapItems = new List<SitemapNode>();

            foreach (var page in documents)
            {
                var relativeUrl = page.GetStringValue(urlColumnName, _pageUrlRetriever.Retrieve(page).RelativePath);

                // TODO: do a massive lookup by URL and alt url instead and look for a match vs. individual, this is highly unoptimized
                // Try to find page by NodeAliasPath

                var actualPage = await _pageRetriever.RetrieveAsync<TreeNode>(
                    query => query
                        .Path(relativeUrl, PathTypeEnum.Single)
                        .Columns(nameof(TreeNode.DocumentModifiedWhen))
                        ,
                    cacheSettings => cacheSettings
                        .Dependencies((items, csbuilder) => csbuilder.Pages(items))
                        .Key($"GetDocumentModified|{relativeUrl}")
                        .Expiration(TimeSpan.FromMinutes(15))
                        );

                if (actualPage.Any())
                {
                    siteMapItems.Add(ConvertToSiteMapUrl(relativeUrl, actualPage.First().DocumentModifiedWhen));
                }
                else
                {
                    siteMapItems.Add(ConvertToSiteMapUrl(relativeUrl, null));
                }
            }
            return siteMapItems;
        }

        private async Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetForAllClassAsync(string path, SiteMapOptions options)
        {
            return (await GetSiteMapUrlSetBaseAsync(path, options)).Select(x => TreeNodeToSitemapNode(x));
        }

        private SitemapNode TreeNodeToSitemapNode(TreeNode node)
        {
            return new SitemapNode(node.ToPageIdentity().AbsoluteUrl)
            {
                LastModificationDate = node.DocumentModifiedWhen
            };
        }

        private async Task<IEnumerable<TreeNode>> GetSiteMapUrlSetForClassBase(string path, string className, SiteMapOptions options)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.PagePath(path, PathTypeEnum.Section);

            string culture = DataHelper.GetNotEmpty(options.CultureCode, _repoContext.CurrentCulture());

            int nodeLevel = 0;
            if (options.MaxRelativeLevel > -1)
            {
                nodeLevel = await GetNodeLevelAsync(path);
            }

            // Get the actual items
            var results = await _pageRetriever.RetrieveAsync(className, query =>
            {
                query.Path(path, PathTypeEnum.Section);
                if (options.CheckDocumentPermissions.HasValue)
                {
                    query.CheckPermissions(options.CheckDocumentPermissions.Value);
                }
                if (options.CombineWithDefaultCulture.HasValue)
                {
                    query.CombineWithDefaultCulture(options.CombineWithDefaultCulture.Value);
                }
                if (options.MaxRelativeLevel > -1)
                {
                    // Get the nesting level of the give path
                    query.NestingLevel(options.MaxRelativeLevel + nodeLevel);
                }
                if (options.WhereCondition.TryGetValue(out var whereCondition))
                {
                    query.Where(whereCondition);
                }
                query.Culture(culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .WithPageUrlPaths();
            }, cacheSettings =>
                cacheSettings
                    .Dependencies((items, csbuilder) => builder.ApplyDependenciesTo(key => csbuilder.Custom(key)))
                    .Key($"GetSiteMapUrlSetForClassBase|{path}{className}|{options.GetCacheKey()}")
                    .Expiration(TimeSpan.FromMinutes(1440))
                );
            return results;
        }

        private async Task<IEnumerable<TreeNode>> GetSiteMapUrlSetBaseAsync(string path, SiteMapOptions options)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.PagePath(path, PathTypeEnum.Section);

            string culture = DataHelper.GetNotEmpty(options.CultureCode, _repoContext.CurrentCulture());

            int nodeLevel = 0;
            if (options.MaxRelativeLevel > -1)
            {
                nodeLevel = await GetNodeLevelAsync(path);
            }

            // Get the actual items
            var results = await _pageRetriever.RetrieveAsync<TreeNode>(query =>
            {
                query.Path(path, PathTypeEnum.Section);
                if (options.CheckDocumentPermissions.HasValue)
                {
                    query.CheckPermissions(options.CheckDocumentPermissions.Value);
                }
                if (options.CombineWithDefaultCulture.HasValue)
                {
                    query.CombineWithDefaultCulture(options.CombineWithDefaultCulture.Value);
                }
                if (options.MaxRelativeLevel > -1)
                {
                    // Get the nesting level of the give path
                    query.NestingLevel(options.MaxRelativeLevel + nodeLevel);
                }
                if (options.WhereCondition.TryGetValue(out var whereCondition))
                {
                    query.Where(whereCondition);
                }
                query.Culture(culture)
                    .CombineWithDefaultCulture()
                    .CombineWithAnyCulture()
                    .WithPageUrlPaths();
            }, cacheSettings =>
                cacheSettings
                    .Dependencies((items, csbuilder) => builder.ApplyDependenciesTo(key => csbuilder.Custom(key)))
                    .Key($"GetSiteMapUrlSetBaseAsync|{path}|{options.GetCacheKey()}")
                    .Expiration(TimeSpan.FromMinutes(1440))
                );
            return results;
        }

        private async Task<int> GetNodeLevelAsync(string path)
        {
            var levelResult = await _pageRetriever.RetrieveAsync<TreeNode>(query =>
               query
                   .Path(path, PathTypeEnum.Single)
                   .Columns(nameof(TreeNode.NodeLevel)),
               cacheSettings =>
               cacheSettings
                   .Dependencies((items, csbuilder) => csbuilder.PagePath(path, PathTypeEnum.Single))
                   .Key($"GetNodeLevelByPath|{path}")
                   .Expiration(TimeSpan.FromMinutes(60))
               );
            return levelResult.FirstOrDefault()?.NodeLevel ?? 0;
        }

        public Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetPageBuilderAsync(SiteMapOptionsPageBuilderOnly options)
        {
            options.Path = DataHelper.GetNotEmpty(options.Path, "/").Replace("%", "");

            // Build out the query
            /*declare @siteID int = 1
            declare @contentHolderPageType nvarchar(100) = 'mmweb.sectionfolder' -- If null, won't check sub areas
            declare @cultureCode nvarchar(5) = 'en-US'*/
            var queryParams = new QueryDataParameters();
            if (options.SiteID.TryGetValue(out var siteID))
            {
                queryParams.Add("@SiteID", siteID);
            }
            if (options.CultureCode.TryGetValue(out var culture))
            {
                queryParams.Add("@DocumentCulture", culture);
            }

            Maybe<int> maxLevel = options.MaxRelativeLevel < 0 ? Maybe.None : options.Path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length + options.MaxRelativeLevel;

            string viewCultureJoin = (options.LanguageGroupingMode == PageBuilderLanguageGroupingMode.AllCultures || options.LanguageGroupingMode == PageBuilderLanguageGroupingMode.OnlySpecifiedCulture)
           ? " inner join View_CMS_Tree_Joined on NodeID = PageUrlPathNodeID and (case when HasDocMatch = 1 then DocumentCulture else PageUrlPathCulture end = PageUrlPathCulture)"
           : " inner join View_CMS_Tree_Joined on NodeID = PageUrlPathNodeID and PageUrlPathCulture = DocumentCulture";

            var query = $@"
select 
    *,
        case when DaysSinceModified > 365 then 'yearly' else
            case when DaysSinceModified > 30 then 'monthly' else
                case when DaysSinceModified > 7 then 'weekly' else 'daily' end
            end
        end as ChangeFrequency
from 
(
    Select 
        '/'+PageUrlPathUrlPath as [Url],
        LatestModified, 
        NodeID, 
        NodeLevel, 
        NodeOrder, 
        NodeParentID,
        PageUrlPathCulture, 
        DATEDIFF(day, LatestModified, GETDATE()) as DaysSinceModified 
    from 
    (
        select 
            CMS_PageUrlPath.*, 
            case when DocumentID is null then 0 else 1 end as HasDocMatch 
        from CMS_PageUrlPath
        left join View_CMS_Tree_Joined on NodeID = PageUrlPathNodeID and DocumentCulture = PageUrlPathCulture
    ) PageUrlPathWithHasCultureMatch
    {viewCultureJoin}
    inner join 
    (
        select 
            ParentDocumentID, 
            MAX(LargestDate) as LatestModified
        from (
            select 
                L0.DocumentID as ParentDocumentID, 
                (select MAX(v) from (VALUES (l0.DocumentModifiedWhen), (L1.DocumentModifiedWhen), (L2.DocumentModifiedWhen), (L3.DocumentModifiedWhen), (L4.DocumentModifiedWhen)) as value(v)) as LargestDate 
            from View_CMS_Tree_Joined L0
            left outer join View_CMS_Tree_Joined L1 on L0.NodeID = L1.NodeParentID and L1.ClassName = @contentHolderPageType
            left outer join View_CMS_Tree_Joined L2 on L1.NodeID = L2.NodeParentID
            left outer join View_CMS_Tree_Joined L3 on L2.NodeID = L3.NodeParentID
            left outer join View_CMS_Tree_Joined L4 on L3.NodeID = L4.NodeParentID
        where 
            {(options.ClassNames.Any() ? $"LO.ClassName in ('{string.Join("','", options.ClassNames.Select(x => SqlHelper.EscapeQuotes(x)))}')" : "")}
    ) DocumentsWithLargestDate
    group by 
        ParentDocumentID
) DocumentIDToLastModified on ParentDocumentID = DocumentID
where 
    NodeAliasPath like '{options.Path}%'
    {(options.SiteID.HasValue ? "and NodeSiteID = @SiteID" : "")}
    {(options.LanguageGroupingMode == PageBuilderLanguageGroupingMode.OnlySpecifiedCulture && options.CultureCode.HasValue ? "and DocumentCulture = @CultureCode" : "")}
    {(options.WhereCondition.TryGetValue(out var whereCondition) ? $"and ({whereCondition})" : "")}
) DataWithDays
    where 
        1=1
        {(maxLevel.TryGetValue(out var maxLevelVal) ? $"and NodeLevel <= {maxLevelVal}" : "")}
    order by
        NodeID,
        PageUrlPathCulture
";

        }
    }
}