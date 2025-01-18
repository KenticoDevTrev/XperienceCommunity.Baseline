using CMS.ContentEngine;
using CSharpFunctionalExtensions;
using Navigation.Models;

namespace Navigation.Services
{
    public interface ISiteMapCustomizationService
    {
        /// <summary>
        /// Gives you the option of converting the given items of the specific type into Sitemap nodes yourself.
        /// 
        /// Return Result.Failure to bypass or let default logic apply.
        /// </summary>
        /// <param name="contentTypeCodename">The Content Type Code Name</param>
        /// <param name="items">The Sitemap items of that type, may need to adjust the CustomizeRetrieval method so you have the columns you wish.  These may be IWebPageContentQueryDataContainer, so you can type check</param>
        /// <returns></returns>
        Task<Result<IEnumerable<SitemapNode>>> CustomizeCasting(string contentTypeCodename, IEnumerable<IContentQueryDataContainer> items);

        /// <summary>
        /// Overwrite to build out a list of your own SitemapNodes, will be appended during the normal ISiteMapRepository.GetSiteMapUrlSetAsync()
        /// </summary>
        /// <returns>Any additional SitemapNodes that don't fall under the default logic of inheriting the IBaseMetadata or being a Navigation Webpage Item.</returns>
        Task<IEnumerable<SitemapNode>> GetAdditionalSitemapNodes();
    }
}
