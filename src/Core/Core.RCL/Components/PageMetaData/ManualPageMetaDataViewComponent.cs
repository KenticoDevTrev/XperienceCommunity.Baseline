namespace Core.Components.PageMetaData
{
    [ViewComponent]
    public class ManualPageMetaDataViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ManualPageMetaDataViewComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Render Page meta data with a custom meta data item
        /// </summary>
        /// <param name="xMetaData"></param>
        /// <returns></returns>
        public IViewComponentResult Invoke(Models.PageMetaData xMetaData)
        {
            // Store that this was manually invoked
            if(_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext))
            {
                httpContext.Items.Add("ManualMetaDataAdded", true);
            }

            var model = new PageMetaDataViewModel()
            {
                Title = xMetaData.Title,
                Keywords = xMetaData.Keywords,
                Description = xMetaData.Description,
                Thumbnail = xMetaData.Thumbnail,
                CanonicalUrl = xMetaData.CanonicalUrl,
                NoIndex = xMetaData.NoIndex
            };
            return View("~/Components/PageMetaData/PageMetaData.cshtml", model);
        }
    }
   
}
