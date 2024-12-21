﻿using CMS.Core;
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

namespace Search.Repositories.Implementations
{
    public class LuceneSearchRepository(ILuceneIndexManager luceneIndexManager,
        ILuceneSearchService luceneSearchService,
        IBaselineSearchLuceneCustomizations baselineSearchLuceneCustomizations,
        IEventLogService eventLogService) : ISearchRepository
    {
        private readonly ILuceneIndexManager _luceneIndexManager = luceneIndexManager;
        private readonly ILuceneSearchService _luceneSearchService = luceneSearchService;
        private readonly IBaselineSearchLuceneCustomizations _baselineSearchLuceneCustomizations = baselineSearchLuceneCustomizations;
        private readonly IEventLogService _eventLogService = eventLogService;

        public async Task<SearchResponse> Search(string searchValue, IEnumerable<string> indexes, int page, int pageSize)
        {
            return indexes.Count() switch {
                0 => new SearchResponse(),
                1 => await SearchInternalSingleIndex(searchValue, indexes.First(), page, pageSize),
                _ => await SearchInternalMultipleIndexes(searchValue, indexes, page, pageSize),
            };
        }

        /// <summary>
        /// Single index is much simpler as you don't need all the results parsed, only the ones you want to return.
        /// </summary>
        /// <param name="searchValue"></param>
        /// <param name="indexName"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private async Task<SearchResponse> SearchInternalSingleIndex(string searchValue, string indexName, int page, int pageSize)
        {
            int MAX_RESULTS = 1000;

            var index = _luceneIndexManager.GetIndex(indexName);
            if (index == null) {
                _eventLogService.LogWarning("LuceneSearchRepository", "IndexMissing", eventDescription: $"Index {indexName} requested in code but not found, please create this Lucene index!");
                return new SearchResponse();
            }
            var query = await _baselineSearchLuceneCustomizations.GetTermQuery(searchValue ?? string.Empty, index);
            return _luceneSearchService.UseSearcher(index, (searcher) => {
                var results = searcher.Search(query, MAX_RESULTS);
                var allScoreDocs = results.ScoreDocs.OrderByDescending(x => x.Score).ToArray();
                var totalHits = results.TotalHits;

                pageSize = Math.Max(1, pageSize);
                page = Math.Max(1, page);

                int offset = pageSize * (page - 1);
                int limit = pageSize;

                // get additional info from topDocs
                var docsWithPosition = new List<ScoreDocWithPosition>();
                for (var i = 0; i < allScoreDocs.Length; i++) {
                    docsWithPosition.Add(new ScoreDocWithPosition(allScoreDocs[i], i, indexName));
                }

                var items = docsWithPosition
                        .Skip(offset)
                        .Take(limit)
                        .Select(d => MapToSearchItem(d, searcher.Doc(d.ScoreDoc.Doc), indexName))
                        .ToList();

                return new SearchResponse() {
                    Items = items,
                    TotalPossible = totalHits,
                    HighlightedWords = [], // not supported
                    HighlightRegex = Maybe.None // not supported
                };
            });

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

        private async Task<SearchResponse> SearchInternalMultipleIndexes(string searchValue, IEnumerable<string> indexes, int page, int pageSize)
        {
            int MAX_RESULTS = 1000;
            if (!indexes.Any()) {
                return new SearchResponse();
            }
            var combinedTopDocs = new List<TopDocsListWithIndex>();
            var indexDocIdKeyToDoc = new Dictionary<string, Document>();
            foreach (var indexName in indexes) {
                var index = _luceneIndexManager.GetIndex(indexName);
                if (index == null) {
                    _eventLogService.LogWarning("LuceneSearchRepository", "IndexMissing", eventDescription: $"Index {indexName} requested in code but not found, please create this Lucene index!");
                    continue;
                }
                var query = await _baselineSearchLuceneCustomizations.GetTermQuery(searchValue ?? string.Empty, index);
                var results = _luceneSearchService.UseSearcher(index, (searcher) => {
                    var results = searcher.Search(query, MAX_RESULTS);
                    foreach (var doc in results.ScoreDocs) {
                        indexDocIdKeyToDoc.Add($"{indexName}|{doc.Doc}", searcher.Doc(doc.Doc));
                    }
                    return results;
                });
                combinedTopDocs.Add(new TopDocsListWithIndex(results, indexName));
            }

            var allScoreDocs = combinedTopDocs.SelectMany(x => x.SearchTopDocs.ScoreDocs.Select(doc => new TopDocWithIndex(doc, x.IndexName))).OrderByDescending(x => x.Doc.Score).ToArray();
            var totalHits = combinedTopDocs.Sum(x => x.SearchTopDocs.TotalHits);

            pageSize = Math.Max(1, pageSize);
            page = Math.Max(1, page);

            int offset = pageSize * (page - 1);
            int limit = pageSize;

            // get additional info from topDocs
            var docsWithPosition = new List<ScoreDocWithPosition>();
            for (var i = 0; i < allScoreDocs.Length; i++) {
                docsWithPosition.Add(new ScoreDocWithPosition(allScoreDocs[i].Doc, i, allScoreDocs[i].IndexName));
            }

            var items = docsWithPosition
                    .Skip(offset)
                    .Take(limit)
                    .Select(d => MapToSearchItem(d, indexDocIdKeyToDoc[$"{d.IndexName}|{d.ScoreDoc.Doc}"], d.IndexName))
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

        private record TopDocWithIndex(ScoreDoc Doc, string IndexName);

        private record TopDocsListWithIndex(TopDocs SearchTopDocs, string IndexName);

        private static SearchItem MapToSearchItem(ScoreDocWithPosition scoreDoc, Document doc, string index) => new(
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


        public record ScoreDocWithPosition(ScoreDoc ScoreDoc, int Position, string IndexName);

    }
}