using CMS.DataEngine;
using CMS.DocumentEngine.Routing;
using CMS.SiteProvider;

namespace Core.Extensions
{
    public static class TreeNodeExtensions
    {
        /// <summary>
        /// Converts a TreeNode to the PageIdentity, with a conversion function to cast it to another type (ex a kentico agnostic record)
        /// </summary>
        /// <param name="node">The tree node</param>
        /// <param name="conversion">The conversion function</param>
        /// <returns></returns>
        public static PageIdentity<T> ToPageIdentity<TNode, T>(this TNode node, Func<TNode, T> conversion) where TNode : TreeNode
        {
            var pageIdentity = node.ToPageIdentity();
            var data = conversion.Invoke(node);
            return new PageIdentity<T>(data, pageIdentity);
        }

        /// <summary>
        /// Converts a TreeNode to the PageIdentity, with a conversion function to cast it to another type (ex a kentico agnostic record)
        /// </summary>
        /// <param name="node">The tree node</param>
        /// <param name="conversion">The asynchronous conversion function</param>
        /// <returns></returns>
        public static async Task<PageIdentity<T>> ToPageIdentity<TNode, T>(this TNode node, Func<TNode, Task<T>> conversion) where TNode : TreeNode
        {
            var pageIdentity = node.ToPageIdentity();
            var data = await conversion.Invoke(node);
            return new PageIdentity<T>(data, pageIdentity);
        }

        public static TreeIdentity ToTreeIdentity(this TreeNode node)
        {
            return new TreeIdentity()
            {
                PageGuid = node.NodeGUID.Equals(Guid.Empty) ? Maybe.None : node.NodeGUID,
                PageID = node.NodeID < 1 ? Maybe.None : node.NodeID,
                PathChannelLookup = !string.IsNullOrWhiteSpace(node.NodeAliasPath) ? new PathChannel(Path: node.NodeAliasPath, ChannelId: node.NodeSiteID < 1 ? Maybe.None : node.NodeSiteID) : Maybe.None
            };
        }

        public static ContentIdentity ToContentIdentity(this TreeNode node)
        {
#pragma warning disable CS0618 // Type or member is obsolete - Fine for KX13
            return new ContentIdentity()
            {
                ContentGuid = node.NodeGUID.Equals(Guid.Empty) ? Maybe.None : node.NodeGUID,
                ContentID = node.NodeID < 1 ? Maybe.None : node.NodeID,
                ContentName = !string.IsNullOrWhiteSpace(node.NodeAlias) ? node.NodeAlias : Maybe.None,
                PathChannelLookup = !string.IsNullOrWhiteSpace(node.NodeAliasPath) ? new PathChannel(Path: node.NodeAliasPath, ChannelId: node.NodeSiteID < 1 ? Maybe.None : node.NodeSiteID) : Maybe.None
            };
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static ContentCultureIdentity ToContentCultureIdentity(this TreeNode node)
        {
#pragma warning disable CS0618 // Type or member is obsolete - Fine for KX13
            return new ContentCultureIdentity()
            {
                ContentCultureGuid = node.DocumentGUID.Equals(Guid.Empty) ? Maybe.None : node.DocumentGUID,
                ContentCultureID = node.DocumentID < 1 ? Maybe.None : node.DocumentID,
                PathCultureChannelLookup = !string.IsNullOrWhiteSpace(node.NodeAliasPath) ? new PathCultureChannel(Path: node.NodeAliasPath,
                                                                                                                               Culture: string.IsNullOrWhiteSpace(node.DocumentCulture) ? Maybe.None : node.DocumentCulture,
                                                                                                                               ChannelId: node.NodeSiteID < 1 ? Maybe.None : node.NodeSiteID) : Maybe.None,
                ContentCultureLookup = node.NodeID < 1 ? Maybe.None : new ContentCulture(ContentId: node.NodeID, Culture: string.IsNullOrWhiteSpace(node.DocumentCulture) ? Maybe.None : node.DocumentCulture)
            };
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Converts a TreeNode to the PageIdentity, useful for pages retrieved througH PageBuilder
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static PageIdentity<TPage> ToPageIdentity<TPage>(this TPage node) where TPage : TreeNode
        {
            // Get Urls
            var relativeAndAbsoluteUrl = CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"documentid|{node.DocumentID}");
                }
                try
                {
                    if (node.NodeSiteID <= 0)
                    {
                        throw new Exception("Need NodeSiteD");
                    }
                    string url = DocumentURLProvider.GetUrl(node);
                    return new Tuple<string, string>(DocumentURLProvider.GetUrl(node), GetAbsoluteUrlOptimized(url, node.NodeSiteID, node.DocumentCulture, true));
                }
                catch (Exception)
                {
                    // Will need to re-query the page, must be missing columns
                }
                if (node.DocumentID > 0)
                {
                    // get full page
                    var fullNode = CacheHelper.Cache(cs =>
                    {
                        if (cs.Cached)
                        {
                            cs.CacheDependency = CacheHelper.GetCacheDependency(
                            [
                                $"documentid{ node.DocumentID }"
                            ]);
                        }
                        return new DocumentQuery()
                            .WhereEquals(nameof(TreeNode.DocumentID), node.DocumentID)
                            .WithPageUrlPaths()
                            .GetEnumerableTypedResult()
                            .FirstOrMaybe();
                    }, new CacheSettings(10, "GetDocumentForUrlRetrieval", node.DocumentID));

                    if (fullNode.TryGetValue(out var fullNodeVal))
                    {
                        string url = DocumentURLProvider.GetUrl(fullNodeVal);
                        return new Tuple<string, string>(url.RemoveTildeFromFirstSpot(), GetAbsoluteUrlOptimized(url, fullNodeVal.NodeSiteID, fullNodeVal.DocumentCulture, true));
                    }
                }

                return new Tuple<string, string>(string.Empty, string.Empty);

            }, new CacheSettings(10, "GetNodeUrlsForPageIdentity", node.DocumentID));

            var pageIdentity = new PageIdentity<TPage>(
                node.DocumentName,
                node.NodeAlias,
                node.NodeID,
                node.NodeGUID,
                node.NodeID,
                node.NodeName,
                node.NodeGUID,
                node.DocumentID,
                node.DocumentGUID,
                node.NodeAliasPath,
                node.DocumentCulture,
                relativeAndAbsoluteUrl.Item1,
                relativeAndAbsoluteUrl.Item2,
                node.NodeLevel,
                node.NodeSiteID,
                node.ClassName,
                node);

            return pageIdentity;
        }

        /// <summary>
        /// DocumentURLProvider.GetPresentationUrl() does various uncached database calls, this caches that to minimize calls for absolute url
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <param name="siteIdentifier"></param>
        /// <param name="cultureCode"></param>
        /// <param name="ensureUrlFormat"></param>
        /// <returns></returns>
        private static string GetAbsoluteUrlOptimized(string virtualPath, SiteInfoIdentifier siteIdentifier, string cultureCode, bool ensureUrlFormat)
        {
            if (siteIdentifier == null)
            {
                throw new InvalidOperationException("siteIdentifier is null");
            }
            if (URLHelper.IsAbsoluteUrl(virtualPath, out Uri uri))
            {
                return virtualPath;
            }
            string presentationUrl = CacheHelper.Cache(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(
                    [
                    $"{SiteInfo.OBJECT_TYPE}|all",
                    $"{SiteDomainAliasInfo.OBJECT_TYPE}|all",
                    ]);
                }
                return DocumentURLProvider.GetPresentationUrl(siteIdentifier, cultureCode);
            }, new CacheSettings(1440, "GetPresentationUrl", siteIdentifier, cultureCode));

            string str = URLHelper.CombinePath(virtualPath, '/', presentationUrl, null);
            if (!URLHelper.IsAbsoluteUrl(str, out uri))
            {
                throw new InvalidOperationException("Unable to get the page absolute URL since the site presentation URL is not in correct format.");
            }
            if (ensureUrlFormat)
            {
                PageRoutingHelper.EnsureAbsoluteUrlFormat(str, siteIdentifier, out str);
            }
            return str;
        }
    }
}
