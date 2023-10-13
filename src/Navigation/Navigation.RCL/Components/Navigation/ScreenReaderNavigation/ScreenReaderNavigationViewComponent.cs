namespace Navigation.Components.Navigation.ScreenReaderNavigation
{
    public class ScreenReaderNavigationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(IEnumerable<NavigationItem> navigationItems, string navigationId, string? navShortcutSelector = null)
        {
            return View("/Components/Navigation/ScreenReaderNavigation/ScreenReaderNavigation.cshtml", new ScreenReaderNavigationViewModel(navigationItems, navigationId, navShortcutSelector));
        }
    }

    public record ScreenReaderNavigationViewModel
    {
        public ScreenReaderNavigationViewModel(IEnumerable<NavigationItem> navigationItems, string id, string? navShortcutSelector = null)
        {
            NavigationItems = navigationItems;
            Id = id;
            NavShortcutSelector = navShortcutSelector.AsMaybe();
        }

        public IEnumerable<NavigationItem> NavigationItems { get; set; }
        public string Id { get; set; }
        public Maybe<string> NavShortcutSelector { get; set; }
    }
}
