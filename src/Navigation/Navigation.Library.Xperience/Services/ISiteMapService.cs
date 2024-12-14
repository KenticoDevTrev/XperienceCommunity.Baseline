using CMS.ContentEngine;
using Navigation.Models;

namespace Navigation.Services
{
    public interface ISiteMapService
    {
        /// <summary>
        /// Converts the array of found ContentQueryDataContainer Items into Sitemap Nodes.
        /// </summary>
        /// <param name="contentQueryDataContainerItems"></param>
        /// <param name="includeTranslationUrlsOfNonTranslatedPages">If the item is a Web Page Item, whether or not to include "Other language" urls if the translation itself doesn't exist.</param>
        /// <returns></returns>
        Task<IEnumerable<SitemapNode>> ConvertToSitemapNode(IEnumerable<IContentQueryDataContainer> contentQueryDataContainerItems, bool includeTranslationUrlsOfNonTranslatedPages);
    }
}
