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

// MODIFICATION INSTRUCTIONS
// Update the slug, uiPageType (to your custom class below), and name
// Set the ChannelCustomSettingsPage<T> type to the model that contains the Kentico.Xperience.Admin Form Annotations, along with the [XperienceSettingsData] attribute

[assembly: UIPage(parentType: typeof(Kentico.Xperience.Admin.Base.UIPages.ChannelEditSection),
    slug: "channel-custom-settings",
    uiPageType: typeof(TestChannelCustomSettingsExtender),
    name: "Custom settings",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]
namespace XperienceCommunity.ChannelSettings.Admin
{
    public class TestChannelCustomSettingsExtender(Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
                                                                     IFormDataBinder formDataBinder,
                                                                     IChannelCustomSettingsRepository customChannelSettingsRepository,
                                                                     IChannelSettingsInternalHelper channelCustomSettingsInfoHandler,
                                                                     IInfoProvider<ChannelInfo> channelInfoProvider) 
        // Change type below to your settings
        : ChannelCustomSettingsPage<TestSettingsFormAnnotation>(formItemCollectionProvider, formDataBinder, customChannelSettingsRepository, channelCustomSettingsInfoHandler, channelInfoProvider)
    {
    }

    /// <summary>
    /// The Annotated version of the Model.  This is optional, but without declaring a separate model then you cannot retrieve TestSettings without referencing the Kentico.Xperience.Admin package.
    /// </summary>
    [FormCategory(Label = "Non Nullable", Order = 100, Collapsible = true, IsCollapsed = false)]
    //[FormCategory(Label = "Nullable", Order = 200, Collapsible = true, IsCollapsed = true)]
    public class TestSettingsFormAnnotation : TestSettings
    {

        [CheckBoxComponent(Label = "TestBool", Order = 101)]
        public override bool TestBool { get; set; } = false;

        [NumberInputComponent(Label = "TestInt", Order = 103)]
        public override int TestInt { get; set; } = 2;


        /*

     

        [NumberInputComponent(Label = "TestChar", Order = 109)]
        public override char TestChar { get; set; } = ' ';

        [DoubleNumberInputComponent(Label = "TestDouble", Order = 110)]
        public override double TestDouble { get; set; } = 3.14;

        [DecimalNumberInputComponent(Label = "TestDecimal", Order = 111)]
        public override decimal TestDecimal { get; set; } = 1.333333m;

        //[DateTimeInputComponent(Label = "TestDateTime", Order = 112)]
        //public override DateTime TestDateTime { get; set; } = DateTime.Now;

        //[DateInputComponent(Label = "TestDate", Order = 113)]
        //public override DateTime TestDate { get; set; } = DateTime.Now;

        [TextInputComponent(Label = "TestString", Order = 114)]
        public override string TestString { get; set; } = string.Empty;

        //[TextInputComponent(Label = "TestGuid", Order = 115)]
        //public override Guid TestGuid { get; set; } = Guid.Empty;
*/


    }
}
