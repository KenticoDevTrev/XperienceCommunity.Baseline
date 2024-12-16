
using Core.Repositories;
using Site.Models.ChannelSettings;
using XperienceCommunity.ChannelSettings.Repositories;

namespace BaselineSiteElements.Features.Home
{
    [ViewComponent]
    public class HomeViewComponent(IChannelCustomSettingsRepository channelCustomSettingsRepository,
        IPageContextRepository pageContextRepository) : ViewComponent
    {
        private readonly IChannelCustomSettingsRepository _channelCustomSettingsRepository = channelCustomSettingsRepository;
        private readonly IPageContextRepository _pageContextRepository = pageContextRepository;

        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var context = await _pageContextRepository.GetCurrentPageAsync();
            var settings = await _channelCustomSettingsRepository.GetSettingsModel<SEOChannelSettings>();
            var keys = _channelCustomSettingsRepository.GetSettingModelDependencyKeys<SEOChannelSettings>();

            // Any retrieval here
            var model = new HomeViewModel()
            {
                // Properties here
            };
            return View("/Features/Home/Home.cshtml", model);
        }
    }

    public record HomeViewModel
    {

    }
}
