using Microsoft.Extensions.Primitives;
using Search.Repositories;
namespace Search.Features.Search
{
    [ViewComponent]
    public class SearchViewComponent(
        ISearchRepository _searchRepository,
        IHttpContextAccessor _httpContextAccessor) : ViewComponent
    {

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get values from Query String
            Maybe<string> searchValue = Maybe.None;
            int page = 1;
            int pageSize = 100;
            if (_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext))
            {
                if (httpContext.Request.Query.TryGetValue("searchValue", out StringValues querySearchValue) 
                    && querySearchValue.FirstOrMaybe(x => !string.IsNullOrWhiteSpace(x)).TryGetValue(out var querySearchVal))
                {
                    searchValue = querySearchVal;
                }
                if (httpContext.Request.Query.TryGetValue("page", out StringValues queryPage) 
                    && queryPage.FirstOrMaybe(x => !string.IsNullOrWhiteSpace(x)).TryGetValue(out var queryPageVal))
                {
                    _ = int.TryParse(queryPageVal, out page);
                }
                if (httpContext.Request.Query.TryGetValue("pageSize", out StringValues queryPageSize) 
                    && queryPageSize.FirstOrMaybe(x => !string.IsNullOrWhiteSpace(x)).TryGetValue(out var queryPageSizeVal))
                {
                    _ = int.TryParse(queryPageSizeVal, out pageSize);
                }
            }

            
            Maybe<SearchResponse> results = Maybe.None;
            if (searchValue.TryGetValue(out var searchVal))
            {
                var indexes = new string[] { "SearchIndexName" };

                // Perform search
                results = await _searchRepository.Search(searchVal, indexes, page, pageSize);
            }
            var model = new SearchViewModel(
                searchValue: searchValue.GetValueOrDefault(string.Empty),
                currentPage: page,
                pageSize: pageSize
            )
            {
                SearchResults = results
            };
            return View("/Features/Search/Search.cshtml", model);
        }
    }

    public record SearchViewModel
    {
        public SearchViewModel(string searchValue, int currentPage, int pageSize)
        {
            SearchValue = searchValue;
            CurrentPage = currentPage;
            PageSize = pageSize;
        }

        public string SearchValue { get; init; }
        public Maybe<SearchResponse> SearchResults { get; init; }
        public int CurrentPage { get; init; } = 1;
        public int PageSize { get; init; } = 100;
    }
}
