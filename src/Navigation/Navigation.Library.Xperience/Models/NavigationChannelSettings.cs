using Kentico.Xperience.Admin.Base.FormAnnotations;
using XperienceCommunity.ChannelSettings.Attributes;

namespace Navigation.Models
{
    public class NavigationChannelSettings
    {
        [XperienceSettingsData("Navigation.PageTypes", "")]
        [TextInputComponent(Label = "Navigation Page Types", ExplanationText = "semi-colon (;) separated code names of class names that should show up in breadcrumbs and navigation, if empty all Webpage Typed content types items will be included.")]
        public virtual string NavigationPageTypes { get; set; } = string.Empty;

        [XperienceSettingsData("Navigation.DefaultBreadcrumb", "Home")]
        [TextInputComponent(Label = "Default Breadcrumb Text", ExplanationText = "Can be a value or a localization string key", WatermarkText = "Home")]
        public virtual string DefaultBreadcrumbText { get; set; } = string.Empty;

        [XperienceSettingsData("Navigation.DefaultBreadcrumb", "/")]
        [TextInputComponent(Label = "Default Breadcrumb Url", ExplanationText = "Can be a url path or a localization string key", WatermarkText = "/")]
        public virtual string DefaultBreadcrumbUrl { get; set; } = string.Empty;

    }
}
