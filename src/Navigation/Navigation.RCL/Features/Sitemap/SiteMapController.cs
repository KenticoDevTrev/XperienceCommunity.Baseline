using System.Text;

namespace Navigation.Features.Sitemap
{
    public class SiteMapController(
        ISiteMapRepository _siteMapRepository) : Controller
    {
        // GET: SiteMap
        [HttpGet]
        public async Task<ActionResult> IndexAsync()
        {
            var nodes = new List<SitemapNode>();

            // Should customize if you want your own thing, options no longer supported.
            nodes.AddRange(await _siteMapRepository.GetSiteMapUrlSetAsync());

            // Now render manually, sadly the SimpleMVCSitemap disables output cache somehow
            return Content(SitemapNode.GetSitemap(nodes), "text/xml", Encoding.UTF8);
        }

    }
}