using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.Websites;
using CMS.Websites.Routing;
using Core.Enums;
using Core.Models;
using Core.Repositories;
using Core.Services;
using Generic;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Localization;
using MVCCaching;
using Navigation.Models;
using XperienceCommunity.ChannelSettings.Repositories;

namespace Navigation.Repositories.Implementations
{
    public class BreadcrumbRepository(IUrlResolver urlResolver,
        ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
        IStringLocalizer<SharedResources> stringLocalizer,
        IIdentityService identityService,
        IChannelCustomSettingsRepository channelCustomSettingsRepository,
        IProgressiveCache progressiveCache,
        IWebsiteChannelContext websiteChannelContext,
        ICacheRepositoryContext cacheRepositoryContext,
        IContentQueryExecutor contentQueryExecutor,
        IContentItemLanguageMetadataRepository contentItemLanguageMetadataRepository
        ) : IBreadcrumbRepository
    {
        private readonly IUrlResolver _urlResolver = urlResolver;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly IStringLocalizer<SharedResources> _stringLocalizer = stringLocalizer;
        private readonly IIdentityService _identityService = identityService;
        private readonly IChannelCustomSettingsRepository _channelCustomSettingsRepository = channelCustomSettingsRepository;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly IContentItemLanguageMetadataRepository _contentItemLanguageMetadataRepository = contentItemLanguageMetadataRepository;

        public Task<BreadcrumbJsonLD> BreadcrumbsToJsonLDAsync(IEnumerable<Breadcrumb> breadcrumbs, bool excludeFirst = true)
        {
            var itemListElement = new List<ItemListElementJsonLD>();
            int position = 0;
            foreach (Breadcrumb breadcrumb in (excludeFirst ? breadcrumbs.Skip(1) : breadcrumbs)) {
                position++;
                itemListElement.Add(new ItemListElementJsonLD(
                    position: position,
                    name: breadcrumb.LinkText,
                    item: _urlResolver.GetAbsoluteUrl(breadcrumb.LinkUrl)
                    )
               );
            }

            return Task.FromResult(new BreadcrumbJsonLD(itemListElement));
        }

        public Task<List<Breadcrumb>> GetBreadcrumbsAsync(int nodeID, bool includeDefaultBreadcrumb = true) => GetBreadcrumbsAsync(nodeID.ToTreeIdentity(), includeDefaultBreadcrumb);

        public async Task<List<Breadcrumb>> GetBreadcrumbsAsync(TreeIdentity treeIdentity, bool includeDefaultBreadcrumb = true)
        {
            if (!(await treeIdentity.GetOrRetrievePageID(_identityService)).TryGetValue(out var webPageItemID)) {
                return [];
            }
            var builder = _cacheDependencyBuilderFactory.Create();
            var breadcrumbsDictionary = await GetWebPageItemIDToBreadcrumbAndParent();

            bool isCurrentPage = true;
            List<Breadcrumb> breadcrumbs = [];
            var nextWebPageItemID = webPageItemID;
            while (breadcrumbsDictionary.ContainsKey(nextWebPageItemID)) {
                // Add dependency
                builder.WebPage(nextWebPageItemID);

                // Create or get breadcrumb
                var breadcrumbTuple = breadcrumbsDictionary[nextWebPageItemID];
                var breadcrumb = !isCurrentPage ? breadcrumbTuple.Item1 : new Breadcrumb(linkText: breadcrumbTuple.Item1.LinkText, linkUrl: breadcrumbTuple.Item1.LinkUrl, true);
                isCurrentPage = false;
                if (!string.IsNullOrWhiteSpace(breadcrumb.LinkText)) {
                    breadcrumbs.Add(breadcrumb);
                }
                nextWebPageItemID = breadcrumbTuple.Item2;
            }

            // Add given Top Level Breadcrumb if provided
            if (includeDefaultBreadcrumb) {
                breadcrumbs.Add(await GetDefaultBreadcrumbAsync());
            }

            // Reverse breadcrumb order
            breadcrumbs.Reverse();
            return breadcrumbs;
        }

        public async Task<Breadcrumb> GetDefaultBreadcrumbAsync()
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            var settings = await _channelCustomSettingsRepository.GetSettingsModel<NavigationChannelSettings>();

            // TODO: replace with new localization string service
            builder.AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<NavigationChannelSettings>())
                .Object("TheCustomLocalizationKey", settings.DefaultBreadcrumbText)
                .Object("TheCustomLocalizationKey", settings.DefaultBreadcrumbUrl);

            return new Breadcrumb(linkText: _stringLocalizer.GetStringOrDefault(settings.DefaultBreadcrumbText, settings.DefaultBreadcrumbText),
                linkUrl: _stringLocalizer.GetStringOrDefault(settings.DefaultBreadcrumbUrl, settings.DefaultBreadcrumbUrl));
        }

        private async Task<Dictionary<int, Tuple<Breadcrumb, int>>> GetWebPageItemIDToBreadcrumbAndParent()
        {
            var settings = await _channelCustomSettingsRepository.GetSettingsModel<NavigationChannelSettings>();
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.AddKeys(_channelCustomSettingsRepository.GetSettingModelDependencyKeys<NavigationChannelSettings>())
                .AddKey($"webpageitems|all");

            string[] validClassNames = (settings.NavigationPageTypes ?? "").ToLower().Split(";|,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            var channelName = _websiteChannelContext.WebsiteChannelName;
            // Cache dependency should not extend to the CacheDependenciesStore as only the matching breadcrumbs should apply.
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var itemBuilder = new ContentItemQueryBuilder().ForContentTypes(query => {
                    query.ForWebsite(channelName, PathMatch.Section("/"), includeUrlPath: true);
                    //if (validClassNames.Any()) {
                    //    query.OfContentType(validClassNames);
                    //}
                })
                .WithCultureContext(_cacheRepositoryContext);

                var results = await _contentQueryExecutor.GetWebPageResult(itemBuilder, x => x, new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext));

                var webPageItemIDToBreadcrumbWithParent = new Dictionary<int, Tuple<Breadcrumb, int>>();

                var groupedResults = results.GroupBy(x => x.WebPageItemID);
                foreach (var item in groupedResults) {

                    var x = item.First();
                    // Getting WebPageItemID blew up for items without a parent...
                    // TODO: Undo this insane hackery when they fix the null exception...
                    int parentId = 0;
                    if (x.TryGetValue(nameof(WebPageFields.WebPageItemParentID), out int? parentIdVal) && parentIdVal.HasValue) {
                        parentId = parentIdVal.Value;
                    }

                    var linkText = "";
                    try {
                        linkText = x.TryGetValue(nameof(IBaseMetadata.MetaData_PageName), out string menuName) ? menuName : "";
                    } catch (Exception) {
                        // if try get value throws an exception (page doesn't inherit the BaseMetadata i suppose)
                    }
                    if (linkText.AsNullOrWhitespaceMaybe().HasNoValue
                        && (await _contentItemLanguageMetadataRepository.GetOptimizedContentItemLanguageMetadata(x, true, true)).TryGetValue(out var languageMetadata)) {
                        linkText = languageMetadata.ContentItemLanguageMetadataDisplayName;
                    }

                    webPageItemIDToBreadcrumbWithParent.Add(item.Key, new Tuple<Breadcrumb, int>(new Breadcrumb(linkText: linkText.AsNullOrWhitespaceMaybe().GetValueOrDefault(x.WebPageItemName), linkUrl: $"/{x.WebPageUrlPath}"), parentId));
                }
                return webPageItemIDToBreadcrumbWithParent;

            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetWebPageItemIDToBreadcrumbAndParent", string.Join(",", validClassNames), channelName, _cacheRepositoryContext.ToCacheNameIdentifier()));
        }
    }
}
