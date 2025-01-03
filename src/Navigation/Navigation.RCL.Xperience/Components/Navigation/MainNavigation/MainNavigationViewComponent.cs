﻿namespace Navigation.Components.Navigation.MainNavigation
{
    [ViewComponent(Name = "MainNavigation")]
    public class MainNavigationViewComponent(INavigationRepository _navigationRepository) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string xNavigationParentPath, string xCssClass = "MainNav", bool xIncludeScreenReaderNav = true)
        {
            xNavigationParentPath = !string.IsNullOrWhiteSpace(xNavigationParentPath) ? xNavigationParentPath : "/MasterPage/Navigation";
            var NavItems = await _navigationRepository.GetNavItemsAsync(xNavigationParentPath);
            var model = new MainNavigationViewModel(
                navItems: NavItems.ToList(),
                navWrapperClass: xCssClass,
                includeScreenReaderNavigation: xIncludeScreenReaderNav
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

            public IEnumerable<NavigationItem> NavItems { get; init; } = [];
            public string NavWrapperClass { get; init; }
            public bool IncludeScreenReaderNavigation { get; init; }
        }
    }
}
