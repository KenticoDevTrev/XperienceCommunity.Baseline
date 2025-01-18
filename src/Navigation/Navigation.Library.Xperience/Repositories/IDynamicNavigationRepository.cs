using Navigation.Models;

namespace Navigation.Repositories
{
    public interface IDynamicNavigationRepository
    {
        Task<IEnumerable<NavigationItem>> GetDynamicNavigation(string dynamicCodeName);
    }
}
