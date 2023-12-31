using MVCCaching;

namespace Navigation.Components.Navigation.SecondaryNavigation
{
    [ViewComponent(Name = "SecondaryNavigation")]
    public class SecondaryNavigationViewComponent : ViewComponent
    {
        private readonly INavigationRepository _navigationRepository;
        private readonly ICacheDependenciesScope _cacheDependenciesScope;
        private readonly IPageContextRepository _pageContextRepository;

        public SecondaryNavigationViewComponent(INavigationRepository navigationRepository,
            ICacheDependenciesScope cacheDependenciesScope,
            IPageContextRepository pageContextRepository)
        {
            _navigationRepository = navigationRepository;
            _cacheDependenciesScope = cacheDependenciesScope;
            _pageContextRepository = pageContextRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(SecondaryNavigationProperties xNavigationProperties)
        {
            // Begin Cache Scope, this is 'ended' in the view
            _cacheDependenciesScope.Begin();

            xNavigationProperties ??= new SecondaryNavigationProperties();

            // if NodeAliasPath is empty, use current page
            if (xNavigationProperties.Path.HasNoValue)
            {
                var page = await _pageContextRepository.GetCurrentPageAsync();
                if (page.TryGetValue(out var pageItem))
                {
                    xNavigationProperties = xNavigationProperties with { Path = pageItem.Path };
                }
            }

            // If include secondary navigation, need a css class
            if (xNavigationProperties.IncludeSecondaryNavSelector && !string.IsNullOrWhiteSpace(xNavigationProperties.CssClass))
            {
                xNavigationProperties = xNavigationProperties with { CssClass = "secondary-navigation" };
            }

            var ancestorPath = await _navigationRepository.GetAncestorPathAsync(xNavigationProperties.Path.GetValueOrDefault("/"), xNavigationProperties.Level, xNavigationProperties.LevelIsRelative, xNavigationProperties.MinimumAbsoluteLevel);
            var navItems = await _navigationRepository.GetSecondaryNavItemsAsync(ancestorPath, Enums.PathSelectionEnum.ParentAndChildren);
            var model = new NavigationViewModel(

                navItems: navItems.ToList(),
                navWrapperClass: xNavigationProperties.CssClass,
                startingPath: ancestorPath,
                currentPagePath: xNavigationProperties.Path.GetValueOrDefault("/"),
                includeCurrentPageSelector: xNavigationProperties.IncludeSecondaryNavSelector,
                includeScreenReaderNavigation: xNavigationProperties.IncludeScreenReaderNav
            );

            return View("SecondaryNavigation", model);
        }
    }
}
