namespace Navigation.Components.Navigation
{
    public class NavigationViewModel
    {
        public NavigationViewModel(List<NavigationItem> navItems, string navWrapperClass, string startingPath, string currentPagePath, bool includeCurrentPageSelector, bool includeScreenReaderNavigation)
        {
            NavItems = navItems;
            NavWrapperClass = navWrapperClass;
            StartingPath = startingPath;
            CurrentPagePath = currentPagePath;
            IncludeCurrentPageSelector = includeCurrentPageSelector;
            IncludeScreenReaderNavigation = includeScreenReaderNavigation;
        }

        public List<NavigationItem> NavItems { get; set; }
        public string NavWrapperClass { get; set; }
        public string StartingPath { get; set; }
        public string CurrentPagePath { get; set; }
        public bool IncludeCurrentPageSelector { get; set; }
        public bool IncludeScreenReaderNavigation { get; set; }
    }
}