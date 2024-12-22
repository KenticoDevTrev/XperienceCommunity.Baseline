using CMS.ContentEngine;
using CMS.DataEngine;
using XperienceCommunity.ChannelSettings.Admin;
using XperienceCommunity.ChannelSettings.Repositories;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.ChannelSettings.Admin.UI.ChannelCustomSettings;
using XperienceCommunity.ChannelSettings.Models;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using XperienceCommunity.ChannelSettings.Attributes;
using Kentico.Xperience.Admin.Base.FormAnnotations.Internal;
using Site.Models.ChannelSettings;

// MODIFICATION INSTRUCTIONS
// Update the slug, uiPageType (to your custom class below), and name
// Set the ChannelCustomSettingsPage<T> type to the model that contains the Kentico.Xperience.Admin Form Annotations, along with the [XperienceSettingsData] attribute

[assembly: UIPage(parentType: typeof(Kentico.Xperience.Admin.Base.UIPages.ChannelEditSection),
    slug: "seo-channel-custom-settings",
    uiPageType: typeof(SEOChannelSettingsExtender),
    name: "SEO Channel settings",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]
namespace XperienceCommunity.ChannelSettings.Admin
{
    public class SEOChannelSettingsExtender(Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
                                                                     IFormDataBinder formDataBinder,
                                                                     IChannelCustomSettingsRepository customChannelSettingsRepository,
                                                                     IChannelSettingsInternalHelper channelCustomSettingsInfoHandler,
                                                                     IInfoProvider<ChannelInfo> channelInfoProvider) 
        // Change type below to your settings
        : ChannelCustomSettingsPage<SEOChannelSettingsFormAnnotated>(formItemCollectionProvider, formDataBinder, customChannelSettingsRepository, channelCustomSettingsInfoHandler, channelInfoProvider)
    {
    }

    
    public class SEOChannelSettingsFormAnnotated : SEOChannelSettings
    {

        [CheckBoxComponent(Label = "Show Robots", Order = 101)]
        public override bool ShowRobots { get; set; } = false;

        [UrlSelectorComponent(Label = "Favicon", Order = 102)]
        public override string Favicon { get; set; } = "";
    }
}
