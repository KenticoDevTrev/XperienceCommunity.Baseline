
using Site.Models.ChannelSettings;
using XperienceCommunity.ChannelSettings.Repositories;

namespace BaselineSiteElements.Features.Home
{
    [ViewComponent]
    public class HomeViewComponent(IChannelCustomSettingsRepository channelCustomSettingsRepository) : ViewComponent
    {
        private readonly IChannelCustomSettingsRepository _channelCustomSettingsRepository = channelCustomSettingsRepository;

        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
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
