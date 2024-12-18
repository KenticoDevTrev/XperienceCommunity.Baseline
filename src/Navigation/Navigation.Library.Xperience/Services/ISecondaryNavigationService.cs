using CMS.Websites;
using Navigation.Models;

namespace Navigation.Services
{
    public interface ISecondaryNavigationService
    {
        /// <summary>
        /// Since the API requires actions instead of string Where Conditions and the like, you can use this to retrieve your own Web Page Items and convert them to Navigation Items
        /// </summary>
        /// <param name="webPageContentQueryDataContainers"></param>
        /// <returns></returns>
        Task<IEnumerable<NavigationItem>> FilterAndConvertIWebPageContentQueryDataContainerItems(IEnumerable<IWebPageContentQueryDataContainer> webPageContentQueryDataContainers);
    }
}
