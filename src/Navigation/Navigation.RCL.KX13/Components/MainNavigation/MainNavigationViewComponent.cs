

namespace Navigation.Components.Navigation.MainNavigation
{
    [ViewComponent(Name = "MainNavigation")]
    public class MainNavigationViewComponent : ViewComponent
    {
        private readonly INavigationRepository _navigationRepository;

        public MainNavigationViewComponent(INavigationRepository navigationRepository)
        {
            _navigationRepository = navigationRepository;
        }
        public async Task<IViewComponentResult> InvokeAsync(string NavigationParentPath, string CssClass = "MainNav", bool includeScreenReaderNav = true)
        {
            NavigationParentPath = !string.IsNullOrWhiteSpace(NavigationParentPath) ? NavigationParentPath : "/MasterPage/Navigation";
            var NavItems = await _navigationRepository.GetNavItemsAsync(NavigationParentPath);
            var model = new MainNavigationViewModel(
                navItems: NavItems.ToList(),
                navWrapperClass: CssClass,
                includeScreenReaderNavigation: includeScreenReaderNav
            );

            return View("/Components/Navigation/MainNavigation/MainNavigation.cshtml", model);
        }
        public record MainNavigationViewModel
        {
            public MainNavigationViewModel(IEnumerable<NavigationItem> navItems, string navWrapperClass, bool includeScreenReaderNavigation)
            {
                NavItems = navItems;
                NavWrapperClass = navWrapperClass;
                IncludeScreenReaderNavigation = includeScreenReaderNavigation;
            }

            public IEnumerable<NavigationItem> NavItems { get; set; } = Array.Empty<NavigationItem>();
            public string NavWrapperClass { get; set; }
            public bool IncludeScreenReaderNavigation { get; set; }
        }
    }
}
