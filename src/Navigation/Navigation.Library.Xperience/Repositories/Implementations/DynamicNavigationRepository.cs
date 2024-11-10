using Navigation.Models;

namespace Navigation.Repositories.Implementations
{
    public class DynamicNavigationRepository : IDynamicNavigationRepository
    {
        public IEnumerable<NavigationItem> GetDynamicNavication(string dynamicCodeName)
        {
            throw new NotImplementedException("Create your own implementation of IDynamicNavigationRepository and register it in the Services.AddScoped<IDynamicNavigationRepository, MyCustomDynamicNavigationRepository>()");
        }
    }
}
