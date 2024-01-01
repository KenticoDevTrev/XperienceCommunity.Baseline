namespace Core.Components.PageMetaData
{
    [ViewComponent]
    public class PageMetaDataViewComponent(
        IMetaDataRepository _metaDataRepository,
        IHttpContextAccessor _httpContextAccessor) : ViewComponent
    {
        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync(int xContentCultureId = -1)
        {
            if (_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext) && httpContext.Items.TryGetValue("ManualMetaDataAdded", out _))
            {
                // Manual page meta data added, so don't do automatic.
                return Content(string.Empty);
            }

            var metaData = xContentCultureId > 0 ? await _metaDataRepository.GetMetaDataAsync(xContentCultureId) : await _metaDataRepository.GetMetaDataAsync();
            if (metaData.TryGetValue(out var metaDataVal))
            {
                var model = new PageMetaDataViewModel()
                {
                    Title = metaDataVal.Title,
                    Keywords = metaDataVal.Keywords,
                    Description = metaDataVal.Description,
                    Thumbnail = metaDataVal.Thumbnail,
                    CanonicalUrl = metaDataVal.CanonicalUrl,
                    NoIndex = metaDataVal.NoIndex
                };
                return View("/Components/PageMetaData/PageMetaData.cshtml", model);
            }
            else
            {
                return View("/Components/PageMetaData/PageMetaData.cshtml", new PageMetaDataViewModel());
            }
        }
    }
}
