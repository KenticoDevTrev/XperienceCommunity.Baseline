namespace Navigation.Components.Navigation.ScreenReaderNavigation
{
    public class ScreenReaderNavigationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(IEnumerable<NavigationItem> xNavigationItems, string xNavigationId, string? xNavShortcutSelector = null)
        {
            return View("/Components/Navigation/ScreenReaderNavigation/ScreenReaderNavigation.cshtml", new ScreenReaderNavigationViewModel(xNavigationItems, xNavigationId, xNavShortcutSelector));
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

        public IEnumerable<NavigationItem> NavigationItems { get; init; }
        public string Id { get; init; }
        public Maybe<string> NavShortcutSelector { get; init; }

    }
}
