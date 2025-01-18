using Navigation.Models;
using Navigation.Repositories;

namespace MVC.Repositories.Implementations
{
    public class CustomDynamicNavigationRepository : IDynamicNavigationRepository
    {
        public Task<IEnumerable<NavigationItem>> GetDynamicNavigation(string dynamicCodeName)
        {
            if(dynamicCodeName.Equals("SampleDynamicCode", StringComparison.OrdinalIgnoreCase)) {
                return Task.FromResult((IEnumerable<NavigationItem>)[new NavigationItem("My Dynamic Nav Item") {
                    LinkHref = "/Some-Url"
                }]);
            }
            return Task.FromResult((IEnumerable<NavigationItem>)[]);
        }
    }
}
