using Search.Models;

namespace Search.Repositories.Implementations
{
    public class SearchRepository() : ISearchRepository
    {
        public Task<SearchResponse> Search(string searchValue, IEnumerable<string> indexes, int page, int pageSize)
        {
            throw new NotImplementedException("This must be implemented by the appropriate Search.Library.Xperience.SEARCHPROVIDER");
        }
    }
}
