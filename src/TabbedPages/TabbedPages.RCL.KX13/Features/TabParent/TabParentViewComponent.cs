using TabbedPages.Models;
using TabbedPages.Repositories;

namespace TabbedPages.Features.TabParent
{
    [ViewComponent]
    public class TabParentViewComponent : ViewComponent
    {
        private readonly ITabRepository _tabRepository;

        public TabParentViewComponent(ITabRepository tabRepository)
        {
            _tabRepository = tabRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(PageIdentity xPage)
        {
            var model = new TabParentViewModel(
                name: xPage.Name,
                tabs: await _tabRepository.GetTabsAsync(xPage.TreeIdentity)
            );
            return View("/Features/TabParent/TabParent.cshtml", model);
        }
    }

    public record TabParentViewModel
    {
        public TabParentViewModel(IEnumerable<TabItem> tabs, string name)
        {
            Tabs = tabs;
            Name = name;
        }

        public IEnumerable<TabItem> Tabs { get; init; }
        public string Name { get; init; }
    }
}
