using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Generic;
using Kentico.Xperience.Lucene.Core.Indexing;
using Kentico.Xperience.Lucene.Core.Search;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Util;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using Navigation.Models;
using Navigation.Repositories;
using Search.Models;
using System.Data;
using Testing;
using XperienceCommunity.Authorization;
using XperienceCommunity.MemberRoles.Models;
using XperienceCommunity.MemberRoles.Repositories;
using XperienceCommunity.MemberRoles.Services;

namespace MVC.Features.Testing
{
    public class TestController(
        IContentQueryExecutor contentQueryExecutor,
        IMemberAuthorizationFilter memberAuthorizationFilter,
        IContentQueryResultMapper contentQueryResultMapper,
        IMemberAuthenticationContext memberAuthenticationContext,
        IRoleStore<TagApplicationUserRole> roleStore,
        IUserRoleStore<ApplicationUserBaseline> userRoleStore,
        IUserStore<ApplicationUserBaseline> userStore,
        UserManager<ApplicationUserBaseline> userManager,
        INavigationRepository navigationRepository,
        IBreadcrumbRepository breadcrumbRepository,
        IInfoProvider<TagInfo> tagInfoProvider,
        ISiteMapRepository siteMapRepository,
        IStringLocalizer<SharedResources> stringLocalizer,
        ILuceneSearchService luceneSearchService,
        ILuceneIndexManager luceneIndexManager
        ) : Controller
    {
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly IMemberAuthorizationFilter _memberAuthorizationFilter = memberAuthorizationFilter;
        private readonly IContentQueryResultMapper _contentQueryResultMapper = contentQueryResultMapper;
        private readonly IMemberAuthenticationContext _memberAuthenticationContext = memberAuthenticationContext;
        private readonly IRoleStore<TagApplicationUserRole> _roleStore = roleStore;
        private readonly IUserRoleStore<ApplicationUserBaseline> _userRoleStore = userRoleStore;
        private readonly IUserStore<ApplicationUserBaseline> _userStore = userStore;

        private readonly UserManager<ApplicationUserBaseline> _userManager = userManager;
        private readonly INavigationRepository _navigationRepository = navigationRepository;
        private readonly IBreadcrumbRepository _breadcrumbRepository = breadcrumbRepository;
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;
        private readonly ISiteMapRepository _siteMapRepository = siteMapRepository;
        private readonly IStringLocalizer<SharedResources> _stringLocalizer = stringLocalizer;
        private readonly ILuceneSearchService _luceneSearchService = luceneSearchService;
        private readonly ILuceneIndexManager _luceneIndexManager = luceneIndexManager;

        private const int PHRASE_SLOP = 3;


        public async Task<string> Index()
        {
            string indexName = "TestIndex";
            string? searchText = "Home";
            int pageSize = 20;
            int page = 1;
            int MAX_RESULTS = 1000;
            var index = _luceneIndexManager.GetIndex(indexName);
            if (index == null) {
                return string.Empty;
            }
            var query = GetTermQuery(searchText ?? string.Empty, index);
            var results = _luceneSearchService.UseSearcher(index, (searcher) => {
                var topDocs = searcher.Search(query, MAX_RESULTS);

                pageSize = Math.Max(1, pageSize);
                page = Math.Max(1, page);

                int offset = pageSize * (page - 1);
                int limit = pageSize;

                // get additional info from topDocs
                var docsWithPosition = new List<ScoreDocWithPosition>();
                for (var i = 0; i < topDocs.ScoreDocs.Length; i++) {
                    docsWithPosition.Add(new ScoreDocWithPosition(topDocs.ScoreDocs[i], i));
                }

                return new LuceneSearchResultModel<PageMetaDataSearchResult> {
                    Query = searchText ?? string.Empty,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = topDocs.TotalHits <= 0 ? 0 : ((topDocs.TotalHits - 1) / pageSize) + 1,
                    TotalHits = topDocs.TotalHits,
                    Hits = docsWithPosition
                        .Skip(offset)
                        .Take(limit)
                        .Select(d => MapToResultItem(d, searcher.Doc(d.ScoreDoc.Doc)))
                        .ToList(),
                };
            });

            // test secondary navigation
            var test = await _navigationRepository.GetSecondaryNavItemsAsync("/MPTest-FullAccess/MPTest-BreakInheritance", Navigation.Enums.PathSelectionEnum.ParentAndChildren);

            var test2 = await _breadcrumbRepository.GetBreadcrumbsAsync(13.ToTreeIdentity(), true);
            var jsonLd = await _breadcrumbRepository.BreadcrumbsToJsonLDAsync(test2);

            var sitemap = await _siteMapRepository.GetSiteMapUrlSetAsync();

            var sitemapText = SitemapNode.GetSitemap(sitemap);
            return sitemapText;
        }
        public record PageMetaDataSearchResult(PageMetaData MetaData, string ContentType, int Position, float Score);
        public record ScoreDocWithPosition(ScoreDoc ScoreDoc, int Position);

        private SearchItem MapToSearchItem(ScoreDocWithPosition scoreDoc, Document doc, string index) => new(
            documentExtensions: string.Empty,
            image: doc.Get(nameof(PageMetaData.Thumbnail)),
            content: "TBD",
            created: DateTimeHelper.ZERO_TIME,
            title: doc.Get(nameof(PageMetaData.Title)),
            index: index,
            maxScore: scoreDoc.ScoreDoc.Score,
            position: scoreDoc.Position,
            score: scoreDoc.ScoreDoc.Score,
            type: doc.Get("ContentType"),
            id: doc.Get("Id"),
            absScore: scoreDoc.ScoreDoc.Score
            ) {
            IsPage = true,
            PageUrl = doc.Get(nameof(PageMetaData.CanonicalUrl)).AsNullOrWhitespaceMaybe()
        };

        private PageMetaDataSearchResult MapToResultItem(ScoreDocWithPosition scoreDoc, Document doc) => new(
            MetaData: new() {
                Title = doc.Get(nameof(PageMetaData.Title)).AsNullOrWhitespaceMaybe(),
                Keywords = doc.Get(nameof(PageMetaData.Keywords)).AsNullOrWhitespaceMaybe(),
                Description = doc.Get(nameof(PageMetaData.Description)).AsNullOrWhitespaceMaybe(),
                CanonicalUrl = doc.Get(nameof(PageMetaData.CanonicalUrl)).AsNullOrWhitespaceMaybe(),
                Thumbnail = doc.Get(nameof(PageMetaData.Thumbnail)).AsNullOrWhitespaceMaybe(),
            },
            ContentType: doc.Get("ContentType"),
            Position: scoreDoc.Position,
            Score: scoreDoc.ScoreDoc.Score
         );

        private Query GetTermQuery(string? searchText, LuceneIndex index)
        {
            var analyzer = index.LuceneAnalyzer;
            var queryBuilder = new QueryBuilder(analyzer);

            var booleanQuery = new BooleanQuery();

            if (!string.IsNullOrWhiteSpace(searchText)) {
                booleanQuery = AddToTermQuery(booleanQuery, queryBuilder.CreatePhraseQuery(nameof(PageMetaData.Title), searchText, PHRASE_SLOP), 5);
                booleanQuery = AddToTermQuery(booleanQuery, queryBuilder.CreateBooleanQuery(nameof(PageMetaData.Title), searchText, Occur.SHOULD), 0.5f);

                if (booleanQuery.GetClauses().Count() > 0) {
                    return booleanQuery;
                }
            }

            return new MatchAllDocsQuery();
        }
        private static BooleanQuery AddToTermQuery(BooleanQuery query, Query textQueryPart, float boost)
        {
            if (textQueryPart != null) {
                textQueryPart.Boost = boost;
                query.Add(textQueryPart, Occur.SHOULD);
            }
            return query;
        }


        [ControllerActionAuthorization(AuthorizationType.ByAuthenticated)]
        public async Task<string> MemberRoleTest()
        {

            var testMember = await _userStore.FindByNameAsync("TestMember", CancellationToken.None);
            var roles = await _userRoleStore.GetRolesAsync(testMember, CancellationToken.None);
            var studentRoleStatus = await _roleStore.CreateAsync(new TagApplicationUserRole() {
                Name = "TestNewRole",
                NormalizedName = "testnewrole"
            }, CancellationToken.None);

            if (studentRoleStatus.Succeeded) {
                var newRole = await _roleStore.FindByNameAsync("TestNewRole", CancellationToken.None);
                if (newRole != null) {
                    await _userRoleStore.AddToRoleAsync(testMember, "students", CancellationToken.None);
                }
            } else {
                var newRole = await _roleStore.FindByNameAsync("TestNewRole", CancellationToken.None);
                if (newRole != null) {
                    await _userRoleStore.AddToRoleAsync(testMember, newRole.Name, CancellationToken.None);
                }
            }

            var currentUser = await _memberAuthenticationContext.GetAuthenticationContext();

            // For single type query, use the .IncludeMemberAuthorization() to ensure the columns are returned that are needed for parsing.
            var singleTypeQuery = new ContentItemQueryBuilder().ForContentType(BasicPage.CONTENT_TYPE_NAME, query => query.Columns(nameof(BasicPage.MetaData_PageName))
            .IncludeMemberAuthorization()
            );
            var itemsSingle = await _contentQueryExecutor.GetMappedWebPageResult<BasicPage>(singleTypeQuery);


            // For Multi Type Querys, the Reusable Field Schema is usually returned in the data anyway
            var multiTypeQuery = new ContentItemQueryBuilder().ForContentTypes(parameters =>
                parameters
                    .OfContentType(BasicPage.CONTENT_TYPE_NAME, WebPage.CONTENT_TYPE_NAME, Generic.Navigation.CONTENT_TYPE_NAME)
                    .WithContentTypeFields()
                );
            var itemsMultiType = await _contentQueryExecutor.GetResult<object>(multiTypeQuery, (selector) => {
                // Important to use the _ContentQueryResultMapper to map your items so the IMemberAuthorizationFilter.RemoveUnauthorizedItems can do type checking
                if (selector.ContentTypeName.Equals(BasicPage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
                    return _contentQueryResultMapper.Map<BasicPage>(selector);
                } else if (selector.ContentTypeName.Equals(WebPage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
                    return _contentQueryResultMapper.Map<WebPage>(selector);
                }
                return selector as IContentItemFieldsSource;
            });

            var result = $"Before Filter: \n\r\n\r   -{string.Join("\n\r   -", itemsSingle.Select(x => x.MetaData_PageName))}\n\r";
            var filteredItemsSingle = await _memberAuthorizationFilter.RemoveUnauthorizedItems(itemsSingle, true, ["teachers"]);
            result += $"\n\r\n\rAfter Filter: \n\r\n\r   -{string.Join("\n\r   -", filteredItemsSingle.Select(x => x.MetaData_PageName))}\n\r";

            result += $"\n\r\n\rBefore Filter Multi: \n\r\n\r   -{string.Join("\n\r   -", itemsMultiType.Select(x => ((IContentItemFieldsSource)x).SystemFields.ContentItemName))}\n\r";
            var filteredItemsMulti = await _memberAuthorizationFilter.RemoveUnauthorizedItems(itemsMultiType, true, ["teachers"]);
            result += $"\n\r\n\rAfter Filter Multi: \n\r\n\r   -{string.Join("\n\r   -", filteredItemsMulti.Select(x => ((IContentItemFieldsSource)x).SystemFields.ContentItemName))}\n\r";


            return result;

        }

    }
}
