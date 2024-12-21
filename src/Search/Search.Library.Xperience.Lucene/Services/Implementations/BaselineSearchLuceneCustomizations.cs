using Core.Models;
using Kentico.Xperience.Lucene.Core.Indexing;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Search.Library.Xperience.Lucene.Services.Implementations
{
    public class BaselineSearchLuceneCustomizations : IBaselineSearchLuceneCustomizations
    {
        private const int _phraseSlop = 3;

        public Task<Query> GetTermQuery(string? searchText, LuceneIndex index)
        {
            var analyzer = index.LuceneAnalyzer;
            var queryBuilder = new QueryBuilder(analyzer);

            var booleanQuery = new BooleanQuery();

            if (!string.IsNullOrWhiteSpace(searchText)) {
                booleanQuery = AddToTermQuery(booleanQuery, queryBuilder.CreatePhraseQuery(nameof(PageMetaData.Title), searchText, _phraseSlop), 5);
                booleanQuery = AddToTermQuery(booleanQuery, queryBuilder.CreateBooleanQuery(nameof(PageMetaData.Title), searchText, Occur.SHOULD), 0.5f);

                if (booleanQuery.GetClauses().Count() > 0) {
                    return Task.FromResult<Query>(booleanQuery);
                }
            }

            return Task.FromResult<Query>(new MatchAllDocsQuery());
        }

        private static BooleanQuery AddToTermQuery(BooleanQuery query, Query textQueryPart, float boost)
        {
            if (textQueryPart != null) {
                textQueryPart.Boost = boost;
                query.Add(textQueryPart, Occur.SHOULD);
            }
            return query;
        }
    }
}
