namespace Navigation.Components.Navigation.Breadcrumbs
{
    public class BreadcrumbsManualViewComponent(
        IBreadcrumbRepository _breadcrumbRepository, 
        IHttpContextAccessor _httpContextAccessor) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(IEnumerable<Breadcrumb> xBreadcrumbs, bool xIncludeDefaultBreadcrumb = true)
        {

            if(_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext))
            {
                httpContext.Items.TryAdd("BreadcrumbsManuallyDone", true);
            }
            var breadcrumbList = xBreadcrumbs.ToList();

            // If none set as current page, set the last one to it.
            if(!breadcrumbList.Where(x => x.IsCurrentPage).Any() && breadcrumbList.Count != 0)
            {
                var lastBreadcrumb = breadcrumbList.Last() with { IsCurrentPage = true };
                breadcrumbList.RemoveAt(breadcrumbList.Count - 1);
                breadcrumbList.Add(lastBreadcrumb);
            }

            if(xIncludeDefaultBreadcrumb)
            {
                breadcrumbList.Insert(0, await _breadcrumbRepository.GetDefaultBreadcrumbAsync());
            }
            var model = new BreadcrumbsViewModel()
            {
                Breadcrumbs = breadcrumbList
            };

            return View("/Components/Navigation/Breadcrumbs/Breadcrumbs.cshtml", model);
        }
    }
}
