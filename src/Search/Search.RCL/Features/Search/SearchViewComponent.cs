using Microsoft.Extensions.Primitives;
using Search.Repositories;
namespace Search.Features.Search
{
    
    [ViewComponent]
    public class SearchViewComponent : ViewComponent
    {
        private readonly ISearchRepository _searchRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SearchViewComponent(ISearchRepository searchRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _searchRepository = searchRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get values from Query String
            Maybe<string> searchValue = Maybe.None;
            int page = 1;
            int pageSize = 100;
            if (_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext))
            {
                if (httpContext.Request.Query.TryGetValue("searchValue", out StringValues querySearchValue) && querySearchValue.Any())
                {
                    searchValue = querySearchValue.First();
                }
                if (httpContext.Request.Query.TryGetValue("page", out StringValues queryPage) && queryPage.Any())
                {
                    _ = int.TryParse(queryPage.First(), out page);
                }
                if (httpContext.Request.Query.TryGetValue("pageSize", out StringValues queryPageSize) && queryPageSize.Any())
                {
                    _ = int.TryParse(queryPageSize.First(), out pageSize);
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
