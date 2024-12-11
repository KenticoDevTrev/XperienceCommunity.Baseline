using XperienceCommunity.ChannelSettings.Attributes;

namespace Account.Models
{
    public class MemberPasswordChannelSettings
    {
        [XperienceSettingsData("Baseline.Account.UsePasswordPolicy", false)]
        public virtual bool UsePasswordPolicy { get; set; } = false;

        [XperienceSettingsData("Baseline.Account.MinLength", 8)]
        public virtual int MinLength { get; set; } = 8;

        [XperienceSettingsData("Baseline.Account.NumNonAlphanumericChars", 1)]
        public virtual int NumNonAlphanumericChars { get; set; } = 1;

        [XperienceSettingsData("Baseline.Account.Regex", "")]
        public virtual string Regex { get; set; }  = string.Empty;

        [XperienceSettingsData("Baseline.Account.ViolationMessage", "")]
        public virtual string ViolationMessage { get; set; } = string.Empty;
    }
}
