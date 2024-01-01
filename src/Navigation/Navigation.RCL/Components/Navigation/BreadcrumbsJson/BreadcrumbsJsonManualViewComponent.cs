using System.Text.Json;

namespace Navigation.Components.Navigation.BreadcrumbsJson
{
    [ViewComponent]
    public class BreadcrumbsJsonManualViewComponent(
        IBreadcrumbRepository _breadcrumbRepository,
        IHttpContextAccessor _httpContextAccessor) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(IEnumerable<Breadcrumb> xBreadcrumbs, bool xIncludeDefaultBreadcrumb = true)
        {
            if(_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext))
            {
                httpContext.Items.TryAdd("BreadcrumbJsonLDManuallyDone", true);
            }
            var breadcrumbsList = xBreadcrumbs.ToList();
            if(xIncludeDefaultBreadcrumb)
            {
                breadcrumbsList.Insert(0, await _breadcrumbRepository.GetDefaultBreadcrumbAsync());
            }
            var breadcrumbsJson = await _breadcrumbRepository.BreadcrumbsToJsonLDAsync(breadcrumbsList, !xIncludeDefaultBreadcrumb);
            // Serialize into the raw JSON data
            var model = new BreadcrumbsJsonViewModel(
                serializedBreadcrumbJsonLD: JsonSerializer.Serialize(breadcrumbsJson)
            );
            return View("/Components/Navigation/BreadcrumbsJson/BreadcrumbsJson.cshtml", model);
        }
    }
}
