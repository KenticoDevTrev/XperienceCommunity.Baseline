using CMS.ContentEngine;
using CMS.Core;
using Core.Models;
using CSharpFunctionalExtensions;
using Kentico.Xperience.Lucene.Core.Indexing;
using Kentico.Xperience.Lucene.Core.Search;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Search.Library.Xperience.Lucene.IndexStrategies;
using Search.Library.Xperience.Lucene.Services;
using Search.Library.Xperience.Lucene.Services.Implementations;
using Search.Models;
using XperienceCommunity.MemberRoles.Interfaces;
using XperienceCommunity.MemberRoles.Models;
using XperienceCommunity.MemberRoles.Services;

namespace Search.Repositories.Implementations
{
    public class LuceneSearchRepository(ILuceneIndexManager luceneIndexManager,
        ILuceneSearchService luceneSearchService,
        IBaselineSearchLuceneCustomizations baselineSearchLuceneCustomizations,
        IEventLogService eventLogService,
        IMemberAuthorizationFilter memberAuthorizationFilter) : ISearchRepository
    {
        private readonly ILuceneIndexManager _luceneIndexManager = luceneIndexManager;
        private readonly ILuceneSearchService _luceneSearchService = luceneSearchService;
        private readonly IBaselineSearchLuceneCustomizations _baselineSearchLuceneCustomizations = baselineSearchLuceneCustomizations;
        private readonly IEventLogService _eventLogService = eventLogService;
        private readonly IMemberAuthorizationFilter _memberAuthorizationFilter = memberAuthorizationFilter;

        public async Task<SearchResponse> Search(string searchValue, IEnumerable<string> indexes, int page, int pageSize)
        {
            return indexes.Count() switch {
                0 => new SearchResponse(),
                _ => await SearchInternalMultipleIndexes(searchValue, indexes, page, pageSize),
            };
        }

        private async Task<SearchResponse> SearchInternalMultipleIndexes(string searchValue, IEnumerable<string> indexes, int page, int pageSize)
        {
            int MAX_RESULTS = 1000;
            var combinedItems = new List<DTOWithMemberPermissionConfiguration<CombinedDocWithScoreAndIndex>>();
            var indexDocIdKeyToDoc = new Dictionary<string, DTOWithMemberPermissionConfiguration<Document>>();
            int totalHits = 0;
            foreach (var indexName in indexes) {
                var index = _luceneIndexManager.GetIndex(indexName);
                if (index == null) {
                    _eventLogService.LogWarning("LuceneSearchRepository", "IndexMissing", eventDescription: $"Index {indexName} requested in code but not found, please create this Lucene index!");
                    continue;
                }
                var query = await _baselineSearchLuceneCustomizations.GetTermQuery(searchValue ?? string.Empty, index);
                combinedItems.AddRange( _luceneSearchService.UseSearcher(index, (searcher) => {
                    var results = searcher.Search(query, MAX_RESULTS);
                    totalHits += results.TotalHits;
                    var items = new List<DTOWithMemberPermissionConfiguration<CombinedDocWithScoreAndIndex>>();
                    foreach(var scoreDoc in results.ScoreDocs) {
                        items.Add(ToCombinedDocWithScoreAndIndexWithPermissions(new CombinedDocWithScoreAndIndex(searcher.Doc(scoreDoc.Doc), scoreDoc, indexName)));
                    }
                    return items;
                }));
            }

            // Filter out items not authorized.
            var authorizedSearchItems = (await _memberAuthorizationFilter.RemoveUnauthorizedItems(combinedItems)).Select(x => x.Model);

            var allScoreDocs = authorizedSearchItems.OrderByDescending(x => x.ScoreDoc.Score).ToArray();

            pageSize = Math.Max(1, pageSize);
            page = Math.Max(1, page);

            int offset = pageSize * (page - 1);
            int limit = pageSize;

            // get additional info from topDocs
            var docsWithPosition = new List<ScoreDocWithIndexAndPosition>();
            for (var i = 0; i < allScoreDocs.Length; i++) {
                docsWithPosition.Add(new ScoreDocWithIndexAndPosition(
                    ScoreDoc: allScoreDocs[i].ScoreDoc,
                    Doc: allScoreDocs[i].Doc,
                    Position: i,
                    IndexName: allScoreDocs[i].IndexName));
            }

            var items = docsWithPosition
                    .Skip(offset)
                    .Take(limit)
                    .Select(d => MapToSearchItem(d, d.Doc, d.IndexName))
                    .ToList();

            return new SearchResponse() {
                Items = items,
                TotalPossible = totalHits,
                HighlightedWords = [], // not supported
                HighlightRegex = Maybe.None // not supported
            };

            /*
            This is the recommended model if you are doing your own.
            return new LuceneSearchResultModel<SearchItem> {
                Query = searchValue ?? string.Empty,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalHits <= 0 ? 0 : ((totalHits - 1) / pageSize) + 1,
                TotalHits = totalHits,
                Hits = docsWithPosition
                    .Skip(offset)
                    .Take(limit)
                    .Select(d => MapToSearchItem(d, indexToSearcher[d.IndexName].Doc(d.ScoreDoc.Doc), d.IndexName))
                    .ToList();,
            };
            */

        }

        private static DTOWithMemberPermissionConfiguration<CombinedDocWithScoreAndIndex> ToCombinedDocWithScoreAndIndexWithPermissions(CombinedDocWithScoreAndIndex CombinedItem)
        {
            var Doc = CombinedItem.Doc;
            return new DTOWithMemberPermissionConfiguration<CombinedDocWithScoreAndIndex>(
                Model: CombinedItem,
                MemberPermissionOverride: bool.TryParse(Doc.Get(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionOverride)), out bool overrideValue) && overrideValue,
                ContentID: int.TryParse(Doc.Get(nameof(ContentItemFields.ContentItemID)), out var contentItemID) ? contentItemID : 0,
                MemberPermissionIsSecure: bool.TryParse(Doc.Get(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionIsSecure)), out bool secureValue) && secureValue,
                MemberPermissionRoleTags: (Doc.Get(nameof(IXperienceCommunityMemberPermissionConfiguration.MemberPermissionRoleTags)) ?? "").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                );
        }

        private record ScoreDocWithIndexAndPosition(ScoreDoc ScoreDoc, Document Doc, int Position, string IndexName);

        private record CombinedDocWithScoreAndIndex(Document Doc, ScoreDoc ScoreDoc, string IndexName);

        private static SearchItem MapToSearchItem(ScoreDocWithIndexAndPosition scoreDoc, Document doc, string index) => new(
            documentExtensions: string.Empty,
            image: doc.Get(nameof(PageMetaData.Thumbnail)),
            content: doc.Get(BaselineBaseMetadataIndexingStrategy.CRAWLER_CONTENT_FIELD_NAME),
            created: DateTime.TryParse(doc.Get("Created"), out var createdDate) ? createdDate : DateTime.MinValue,
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


        public record ScoreDocWithPositionAndPermissions(ScoreDoc ScoreDoc, int Position, string IndexName, int ContentItemID, bool CheckPermissions, bool PermissionOverride, bool IsSecure, string[] RoleTags) : IMemberPermissionConfiguration
        {
            public bool GetCheckPermissions() => CheckPermissions;

            public int GetContentID() => ContentItemID;

            public bool GetMemberPermissionIsSecure() => IsSecure;

            public bool GetMemberPermissionOverride() => PermissionOverride;

            public IEnumerable<string> GetMemberPermissionRoleTags() => RoleTags;
        }
    }
}
