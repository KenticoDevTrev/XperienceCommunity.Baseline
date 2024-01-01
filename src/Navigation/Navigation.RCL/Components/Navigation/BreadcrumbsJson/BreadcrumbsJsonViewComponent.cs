using System.Text.Json;

namespace Navigation.Components.Navigation.BreadcrumbsJson
{
    [ViewComponent(Name = "BreadcrumbsJson")]
    public class BreadcrumbsJsonViewComponent(
        IPageContextRepository _pageContextRepository,
        IBreadcrumbRepository _breadcrumbRepository,
        IHttpContextAccessor _httpContextAccessor) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(bool xIncludeDefaultBreadcrumb = true, int xPageId = -1)
        {
            if(_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext)
                && httpContext.Items.ContainsKey("BreadcrumbJsonLDManuallyDone"))
            {
                return Content(String.Empty);
            }
            // Use current page if not provided
            if(xPageId <= 0)
            {
                var currentPage = await _pageContextRepository.GetCurrentPageAsync();
                if (currentPage.TryGetValue(out var curPage))
                {
                    xPageId = curPage.PageID;
                }
            }

            if(xPageId <= 0)
            {
                return Content(string.Empty);
            }
            var breadcrumbs = await _breadcrumbRepository.GetBreadcrumbsAsync(xPageId, xIncludeDefaultBreadcrumb);
            var breadcrumbsJson = await _breadcrumbRepository.BreadcrumbsToJsonLDAsync(breadcrumbs, !xIncludeDefaultBreadcrumb);
            // Serialize into the raw JSON data
            var model = new BreadcrumbsJsonViewModel(
                serializedBreadcrumbJsonLD: JsonSerializer.Serialize(breadcrumbsJson)
            );
            return View("/Components/Navigation/BreadcrumbsJson/BreadcrumbsJson.cshtml", model);
        }
    }
}
