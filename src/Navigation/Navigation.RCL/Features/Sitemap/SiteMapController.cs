using System.Text;

namespace Navigation.Features.Sitemap
{
    public class SiteMapController(
        ISiteMapRepository _siteMapRepository,
        SitemapConfiguration _sitemapConfiguration,
        ISiteRepository _siteRepository) : Controller
    {
        // GET: SiteMap
        [HttpGet]
        public async Task<ActionResult> IndexAsync()
        {
            var nodes = new List<SitemapNode>();

            var siteName = _siteRepository.CurrentChannelName().GetValueOrDefault(string.Empty).ToLower();
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