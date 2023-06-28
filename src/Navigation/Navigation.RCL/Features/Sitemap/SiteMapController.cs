using System.Text;

namespace Navigation.Features.Sitemap
{
    public class SiteMapController : Controller
    {

        private readonly ISiteMapRepository _siteMapRepository;
        private readonly SitemapConfiguration _sitemapConfiguration;
        private readonly ISiteRepository _siteRepository;

        public SiteMapController(ISiteMapRepository siteMapRepository,
            SitemapConfiguration sitemapConfiguration,
            ISiteRepository siteRepository)
        {
            _siteMapRepository = siteMapRepository;
            _sitemapConfiguration = sitemapConfiguration;
            _siteRepository = siteRepository;
        }

        // GET: SiteMap
        [HttpGet]
        public async Task<ActionResult> IndexAsync()
        {
            var nodes = new List<SitemapNode>();

            var siteName = _siteRepository.CurrentSiteName().ToLower();
            if(_sitemapConfiguration.SiteNameToConfigurations.TryGetValue(siteName, out var configs))
            {
                foreach(var config in configs)
                {
                    nodes.AddRange(await _siteMapRepository.GetSiteMapUrlSetAsync(config));
                }
            }

            // Now render manually, sadly the SimpleMVCSitemap disables output cache somehow
            return Content(SitemapNode.GetSitemap(nodes), "text/xml", Encoding.UTF8);
        }

    }
}