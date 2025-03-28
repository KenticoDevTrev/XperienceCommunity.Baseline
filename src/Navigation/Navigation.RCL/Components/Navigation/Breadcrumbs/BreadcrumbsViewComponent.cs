﻿namespace Navigation.Components.Navigation.Breadcrumbs
{
    [ViewComponent(Name = "Breadcrumbs")]
    public class BreadcrumbsViewComponent(
        IPageContextRepository _pageContextRepository,
        IBreadcrumbRepository _breadcrumbRepository,
        IHttpContextAccessor _httpContextAccessor) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(bool xIncludeDefaultBreadcrumb = true, int xPageId = -1)
        {
            if (_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext)
                && (
                    httpContext.Request.Path.Value.GetValueOrDefault("/").Equals("/")
                    ||
                    httpContext.Items.ContainsKey("BreadcrumbsManuallyDone")
                )
                )
            {
                return Content(string.Empty);
            }


            // Use current page if not provided
            if (xPageId <= 0)
            {
                var curPage = await _pageContextRepository.GetCurrentPageAsync();
                if (curPage.TryGetValue(out var curPageItem))
                {
                    if (curPageItem.Equals("/"))
                    {
                        return Content(string.Empty);
                    }

                    xPageId = curPageItem.PageID;
                }
            }

            if (xPageId <= 0)
            {
                return Content(string.Empty);
            }
            var model = new BreadcrumbsViewModel()
            {
                Breadcrumbs = await _breadcrumbRepository.GetBreadcrumbsAsync(xPageId.ToTreeIdentity(), xIncludeDefaultBreadcrumb)
            };
            return View("/Components/Navigation/Breadcrumbs/Breadcrumbs.cshtml", model);
        }
    }

}
