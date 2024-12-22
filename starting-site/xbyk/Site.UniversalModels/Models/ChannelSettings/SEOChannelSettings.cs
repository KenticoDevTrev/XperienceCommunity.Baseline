using XperienceCommunity.ChannelSettings.Attributes;

namespace Site.Models.ChannelSettings
{
    public class SEOChannelSettings
    {
        [XperienceSettingsData("SEO.ShowRobots", true)]
        public virtual bool ShowRobots { get; set; } = false;

        [XperienceSettingsData("SEO.Favicon", "~/images/favicon.png")]
        public virtual string Favicon { get; set; } = string.Empty;
    }
}
