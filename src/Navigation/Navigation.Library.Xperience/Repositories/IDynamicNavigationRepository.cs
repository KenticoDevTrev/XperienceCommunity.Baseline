using Navigation.Models;

namespace Navigation.Repositories
{
    public interface IDynamicNavigationRepository
    {
        IEnumerable<NavigationItem> GetDynamicNavication(string dynamicCodeName);
    }
}
