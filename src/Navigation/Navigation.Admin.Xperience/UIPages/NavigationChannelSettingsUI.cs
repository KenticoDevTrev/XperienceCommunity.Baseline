using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Navigation.Models;
using XperienceCommunity.ChannelSettings.Admin;
using XperienceCommunity.ChannelSettings.Admin.UI.ChannelCustomSettings;
using XperienceCommunity.ChannelSettings.Repositories;

[assembly: UIPage(parentType: typeof(Kentico.Xperience.Admin.Base.UIPages.ChannelEditSection),
    slug: "navigation-channel-custom-settings",
    uiPageType: typeof(NavigationChannelSettingsExtender),
    name: "Navigation Channel Settings",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]

namespace XperienceCommunity.ChannelSettings.Admin
{
    public class NavigationChannelSettingsExtender(Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
                                                                     IFormDataBinder formDataBinder,
                                                                     IChannelCustomSettingsRepository customChannelSettingsRepository,
                                                                     IChannelSettingsInternalHelper channelCustomSettingsInfoHandler,
                                                                     IInfoProvider<ChannelInfo> channelInfoProvider)
        // Change type below to your settings
        : ChannelCustomSettingsPage<NavigationChannelSettings>(formItemCollectionProvider, formDataBinder, customChannelSettingsRepository, channelCustomSettingsInfoHandler, channelInfoProvider)
    {
    }
}