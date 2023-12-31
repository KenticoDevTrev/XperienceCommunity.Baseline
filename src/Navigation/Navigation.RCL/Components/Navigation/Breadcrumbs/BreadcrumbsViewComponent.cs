namespace Navigation.Components.Navigation.Breadcrumbs
{
    [ViewComponent(Name = "Breadcrumbs")]
    public class BreadcrumbsViewComponent : ViewComponent
    {
        private readonly IPageContextRepository _pageContextRepository;
        private readonly IBreadcrumbRepository _breadcrumbRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BreadcrumbsViewComponent(IPageContextRepository pageContextRepository,
            IBreadcrumbRepository breadcrumbRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _pageContextRepository = pageContextRepository;
            _breadcrumbRepository = breadcrumbRepository;
            _httpContextAccessor = httpContextAccessor;
        }


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
                Breadcrumbs = await _breadcrumbRepository.GetBreadcrumbsAsync(xPageId, xIncludeDefaultBreadcrumb)
            };
            return View("/Components/Navigation/Breadcrumbs/Breadcrumbs.cshtml", model);
        }
    }

}
