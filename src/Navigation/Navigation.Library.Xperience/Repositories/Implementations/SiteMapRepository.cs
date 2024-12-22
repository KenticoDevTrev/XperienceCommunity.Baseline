using CMS.ContentEngine;
using CMS.Headless.Internal;
using CMS.Websites;
using CMS.Websites.Routing;
using Core.Repositories;
using Core.Services;
using CSharpFunctionalExtensions;
using Generic;
using Navigation.Models;
using Navigation.Services;
using XperienceCommunity.MemberRoles.Services;

namespace Navigation.Repositories.Implementations
{
    public class SiteMapRepository(IUrlResolver urlResolver,
        IContentItemLanguageMetadataRepository contentItemLanguageMetadataRepository,
        ISiteMapCustomizationService siteMapCustomizationService,
        IWebPageQueryResultMapper webPageQueryResultMapper,
        IDynamicNavigationRepository dynamicNavigationRepository,
        ILanguageRepository languageRepository,
        IWebsiteChannelContext websiteChannelContext,
        IContentTranslationInformationRepository webpageUrlRepository,
        IContentQueryExecutor contentQueryExecutor,
        IMemberAuthorizationFilter memberAuthorizationFilter,
        BaselineNavigationOptions baselineNavigationOptions) : ISiteMapRepository, ISiteMapService
    {
        private readonly IUrlResolver _urlResolver = urlResolver;
        private readonly IContentItemLanguageMetadataRepository _contentItemLanguageMetadataRepository = contentItemLanguageMetadataRepository;
        private readonly ISiteMapCustomizationService _siteMapCustomizationService = siteMapCustomizationService;
        private readonly IWebPageQueryResultMapper _webPageQueryResultMapper = webPageQueryResultMapper;
        private readonly IDynamicNavigationRepository _dynamicNavigationRepository = dynamicNavigationRepository;
        private readonly ILanguageRepository _languageRepository = languageRepository;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;
        private readonly IContentTranslationInformationRepository _webpageUrlRepository = webpageUrlRepository;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly IMemberAuthorizationFilter _memberAuthorizationFilter = memberAuthorizationFilter;
        private readonly BaselineNavigationOptions _baselineNavigationOptions = baselineNavigationOptions;

        public Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetAsync(SiteMapOptions options)
        {
            throw new NotImplementedException(@"Will not be used in Xperience by Kentico

Instead use your own logic to retrieve the IEnumerable<IContentQueryDataContainer> of your items and then use the ISiteMapService.ConvertToSitemapNode to convert.

May wish to overwrite and implement a custom ISiteMapCustomizationService to parsing individual page types (using the IWebPageQueryResultMapper or IContentItemQueryResultMapper) to map to your own data");
        }

        public async Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetAsync()
        {
            // Get anything inheriting the IBaseMetadata
            var baseMetadataQuery = new ContentItemQueryBuilder().ForContentTypes(query => query
                    .ForWebsite(_websiteChannelContext.WebsiteChannelName, PathMatch.Section("/"), includeUrlPath: true)
                    .OfReusableSchema([IBaseMetadata.REUSABLE_FIELD_SCHEMA_NAME])
                )
                .InLanguage(_languageRepository.DefaultLanguageForWebsiteChannel(_websiteChannelContext.WebsiteChannelID).CodeName.GetValueOrDefault("en"), useLanguageFallbacks: true)
                .Parameters(query => query
                    .Where(where => where.WhereEmpty(nameof(IBaseMetadata.MetaData_NoIndex)).Or().WhereEquals(nameof(IBaseMetadata.MetaData_NoIndex), false))
                );
            var baseMetadataItems = await _contentQueryExecutor.GetWebPageResult(baseMetadataQuery, x => x, new ContentQueryExecutionOptions() { ForPreview = false, IncludeSecuredItems = true });

            // Also handle any Navigation Items
            var navigationQuery = new ContentItemQueryBuilder().ForContentType(Generic.Navigation.CONTENT_TYPE_NAME, query => query
                .ForWebsiteChannels([_websiteChannelContext.WebsiteChannelID])
                )
                .InLanguage(_languageRepository.DefaultLanguageForWebsiteChannel(_websiteChannelContext.WebsiteChannelID).CodeName.GetValueOrDefault("en"), useLanguageFallbacks: true);
                
            var navItems = await _contentQueryExecutor.GetWebPageResult(navigationQuery, x=> x, new ContentQueryExecutionOptions() { ForPreview=false, IncludeSecuredItems = true });

            // filter by public only
            var publicItemsOnly = await _memberAuthorizationFilter.RemoveUnauthorizedItems(baseMetadataItems.Union(navItems), userIsAuthenticated: false, userRoles: []);

            // Finally convert
            return await ConvertToSitemapNode(publicItemsOnly, _baselineNavigationOptions.ShowPagesNotTranslatedInSitemapUrlSet);
        }

        public async Task<IEnumerable<SitemapNode>> ConvertToSitemapNode(IEnumerable<IContentQueryDataContainer> contentQueryDataContainerItems, bool showPagesNotTranslatedInSitemapUrlSet)
        {
            var nodes = new List<SitemapNode>();

            foreach (var group in contentQueryDataContainerItems.GroupBy(x => x.ContentTypeName)) {
                if ((await _siteMapCustomizationService.CustomizeCasting(group.Key, group)).TryGetValue(out var sitemapNodes)) {
                    nodes.AddRange(sitemapNodes);
                } else if (group.First() is IWebPageContentQueryDataContainer) {
                    foreach (IWebPageContentQueryDataContainer item in group.Cast<IWebPageContentQueryDataContainer>()) {
                        nodes.AddRange(await WebPageContentItemToSitemapNode(item, showPagesNotTranslatedInSitemapUrlSet));
                    }
                }
            }

            return nodes;
        }

        /// <summary>
        /// Default logic, handles the Navigation Item type logic
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task<IEnumerable<SitemapNode>> WebPageContentItemToSitemapNode(IWebPageContentQueryDataContainer data, bool showPagesNotTranslatedInSitemapUrlSet)
        {
            if (data.ContentTypeName.Equals(Generic.Navigation.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
                var navItem = _webPageQueryResultMapper.Map<Generic.Navigation>(data);
                var navNodes = new List<SitemapNode>();

                // Don't do automatic (should be caught by other page types) and don't do external links
                if (!navItem.NavigationType.Equals("automatic", StringComparison.OrdinalIgnoreCase) && !navItem.NavigationLinkUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                    navNodes.Add(new SitemapNode(navItem.NavigationLinkUrl));
                }

                // Add dynamic sub nav items if available
                if (navItem.IsDynamic && !string.IsNullOrWhiteSpace(navItem.DynamicCodeName)) {
                    var dynamicSubItems = await _dynamicNavigationRepository.GetDynamicNavication(navItem.DynamicCodeName);
                    navNodes.AddRange(dynamicSubItems.Where(x => x.LinkHref.HasValue && !x.LinkHref.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase)).Select(x => new SitemapNode(x.LinkHref.Value)));
                }
                return navNodes;
            }

            var urlSummary = await _webpageUrlRepository.GetWebpageTranslationSummaries(data.WebPageItemID, data.WebPageItemWebsiteChannelID);
            if(!showPagesNotTranslatedInSitemapUrlSet) {
                urlSummary = urlSummary.Where(x => x.TranslationExists);
            }

            // Use summaries instead so we can get other languages
            if (urlSummary.Any()) {
                var firstItem = urlSummary.Where(x => x.IsDefaultLanguage).FirstOrMaybe().GetValueOrDefault(urlSummary.First());
                var otherItems = urlSummary.Except([firstItem]);
                return [new SitemapNode(_urlResolver.ResolveUrl(firstItem.Url)) {
                    LastModificationDate = (await _contentItemLanguageMetadataRepository.GetOptimizedContentItemLanguageMetadata(data, true, true)).TryGetValue(out var webPageMetaData) ? webPageMetaData.ContentItemLanguageMetadataModifiedWhen : Maybe.None,
                    OtherLanguageCodeToUrl = otherItems.ToDictionary(key => key.LanguageName.ToLower(), value => value.Url)
                }];
            } else {
                // Need to get default one
                return [new SitemapNode(_urlResolver.ResolveUrl($"/{data.WebPageUrlPath}")) {
                    LastModificationDate = (await _contentItemLanguageMetadataRepository.GetOptimizedContentItemLanguageMetadata(data, true, true)).TryGetValue(out var webPageMetaData) ? webPageMetaData.ContentItemLanguageMetadataModifiedWhen : Maybe.None,
                }];
            }
        }
    }
}
