using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using Microsoft.CodeAnalysis;

namespace Admin.Installer
{
    public class StartingSiteInstallerChannels(
        StartingSiteInstallationOptions startingSiteInstallationOptions,
        IEventLogService eventLogService,
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider
        )
    {
        private readonly StartingSiteInstallationOptions _startingSiteInstallationOptions = startingSiteInstallationOptions;
        private readonly IEventLogService _eventLogService = eventLogService;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider = channelInfoProvider;
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider = contentLanguageInfoProvider;

        public bool InstallationRan { get; set; } = false;
        public async Task Install()
        {
            if (_startingSiteInstallationOptions.CreateWebChannelIfNone) {
                await CheckAndCreateWebChannel();
            }

            InstallationRan = true;
        }

        private async Task CheckAndCreateWebChannel()
        {
            if (!(await _channelInfoProvider.Get().GetEnumerableTypedResultAsync()).Any()) {
                var webChannel = new ChannelInfo() {
                    ChannelDisplayName = "Web",
                    ChannelName = "Web",
                    ChannelType = ChannelType.Website,
                    ChannelSize = ChannelSize.Standard
                };
                _channelInfoProvider.Set(webChannel);

                var languages = await _contentLanguageInfoProvider.Get().GetEnumerableTypedResultAsync();
                ContentLanguageInfo? language = languages.FirstOrDefault();
                if (language == null) {
                    language = new ContentLanguageInfo() {
                        ContentLanguageDisplayName = "English",
                        ContentLanguageName = "en",
                        ContentLanguageIsDefault = true,
                        ContentLanguageCultureFormat = "en-US",
                        ContentLanguageGUID = Guid.NewGuid()
                    };
                    _contentLanguageInfoProvider.Set(language);
                }


                var websiteChannel = new WebsiteChannelInfo() {
                    WebsiteChannelDomain = "localhost:44373",
                    WebsiteChannelChannelID = webChannel.ChannelID,
                    WebsiteChannelHomePage = "/Home",
                    WebsiteChannelPrimaryContentLanguageID = language?.ContentLanguageID ?? 1,
                    WebsiteChannelDefaultCookieLevel = 1000,
                    WebsiteChannelStoreFormerUrls = true
                };
                websiteChannelInfoProvider.Set(websiteChannel);


            }
        }
    }
}
