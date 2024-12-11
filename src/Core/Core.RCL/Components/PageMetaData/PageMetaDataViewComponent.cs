namespace Core.Components.PageMetaData
{
    [ViewComponent]
    public class PageMetaDataViewComponent(
        IMetaDataRepository _metaDataRepository,
        IHttpContextAccessor _httpContextAccessor) : ViewComponent
    {
        /// <summary>
        /// Uses the current page context to render meta data.
        /// 
        /// the xContentCultureId is only in there for reverse compatability, it's obsoleted now.
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync(TreeCultureIdentity? xTreeCultureIdentity = null, int xContentCultureId = -1)
        {
            if (_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext) && httpContext.Items.TryGetValue("ManualMetaDataAdded", out _))
            {
                // Manual page meta data added, so don't do automatic.
                return Content(string.Empty);
            }

#pragma warning disable CS0618 // Type or member is obsolete - Keeping in for now for reverse compatability
            var metaData =
                xTreeCultureIdentity != null ? await _metaDataRepository.GetMetaDataAsync(xTreeCultureIdentity) : (
                    xContentCultureId > 0 ? await _metaDataRepository.GetMetaDataAsync(xContentCultureId) : 
                    await _metaDataRepository.GetMetaDataAsync()
                );
#pragma warning restore CS0618 // Type or member is obsolete

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
