using CMS.ContentEngine;
using CSharpFunctionalExtensions;
using Navigation.Models;

namespace Navigation.Services
{
    public interface ISiteMapCustomizationService
    {
        /// <summary>
        /// Return the Content Item Query Builder for the given content type code name.
        /// 
        /// Return Result.Failure to bypass and let default logic apply.
        /// </summary>
        /// <param name="contentTypeCodename">The Content Type Code Name</param>
        /// <returns></returns>
        Task<Result<ContentItemQueryBuilder>> CustomizeQueryBuilder(string contentTypeCodename);

        /// <summary>
        /// Gives you the option of converting the given items of the specific type into Sitemap nodes yourself.
        /// 
        /// Return Result.Failure to bypass or let default logic apply.
        /// </summary>
        /// <param name="contentTypeCodename">The Content Type Code Name</param>
        /// <param name="items">The Sitemap items of that type, may need to adjust the CustomizeRetrieval method so you have the columns you wish.  These may be IWebPageContentQueryDataContainer, so you can type check</param>
        /// <returns></returns>
        Task<Result<IEnumerable<SitemapNode>>> CustomizeCasting(string contentTypeCodename, IEnumerable<IContentQueryDataContainer> items);
    }
}
