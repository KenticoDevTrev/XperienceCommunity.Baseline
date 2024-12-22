using Navigation.Models;
using Navigation.Repositories;

namespace MVC.Repositories.Implementations
{
    public class CustomDynamicNavigationRepository : IDynamicNavigationRepository
    {
        public Task<IEnumerable<NavigationItem>> GetDynamicNavication(string dynamicCodeName)
        {
            if(dynamicCodeName.Equals("ConceptBuildersNav", StringComparison.OrdinalIgnoreCase)) {
                return Task.FromResult((IEnumerable<NavigationItem>)[new NavigationItem("Chemistry") {
                    LinkHref = "https://www.physicsclassroom/chemistry"
                }]);
            }
            return Task.FromResult((IEnumerable<NavigationItem>)[]);
        }
    }
}
