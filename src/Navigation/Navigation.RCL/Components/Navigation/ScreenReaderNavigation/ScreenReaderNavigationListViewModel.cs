namespace Navigation.Components.Navigation.ScreenReaderNavigation
{
    public record ScreenReaderNavigationListViewModel
    {
        public ScreenReaderNavigationListViewModel(NavigationItem navItem, int level)
        {
            NavItem = navItem;
            Level = level;
        }
        public NavigationItem NavItem { get; init; }
        public int Level { get; init; }
    }
}
