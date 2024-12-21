using Kentico.Xperience.Lucene.Core.Indexing;
using Lucene.Net.Search;

namespace Search.Library.Xperience.Lucene.Services
{
    public interface IBaselineSearchLuceneCustomizations
    {
        Task<Query> GetTermQuery(string? searchText, LuceneIndex index);
    }
}
