using XperienceCommunity.ChannelSettings.Attributes;

namespace Account.Models
{
    public class AccountChannelSettings
    {
        [XperienceSettingsData("Baseline.Account.AccountRedirectToAccountAfterLogin", false)]
        public virtual bool AccountRedirectToAccountAfterLogin { get; set; } = false;

        [XperienceSettingsData("Baseline.Account.AccountRegistrationUrl", "")]
        public virtual string AccountRegistrationUrl { get; set; } = string.Empty;

        [XperienceSettingsData("Baseline.Account.AccountConfirmationUrl", "")]
        public virtual string AccountConfirmationUrl { get; set; } = string.Empty;

        [XperienceSettingsData("Baseline.Account.AccountLoginUrl", "")]
        public virtual string AccountLoginUrl { get; set; } = string.Empty;

        [XperienceSettingsData("Baseline.Account.AccountTwoFormAuthenticationUrl", "")]
        public virtual string AccountTwoFormAuthenticationUrl { get; set; } = string.Empty;

        [XperienceSettingsData("Baseline.Account.AccountMyAccountUrl", "")]
        public virtual string AccountMyAccountUrl { get; set; } = string.Empty;

        [XperienceSettingsData("Baseline.Account.AccountResetPassword", "")]
        public virtual string AccountResetPassword { get; set; } = string.Empty;

        [XperienceSettingsData("Baseline.Account.AccountLogOutUrl", "")]
        public virtual string AccountLogOutUrl { get; set; } = string.Empty;

        [XperienceSettingsData("Baseline.Account.AccountForgotPasswordUrl", "")]
        public virtual string AccountForgotPasswordUrl { get; set; } = string.Empty;

        [XperienceSettingsData("Baseline.Account.AccountForgottenPasswordResetUrl", "")]
        public virtual string AccountForgottenPasswordResetUrl { get; set; } = string.Empty;

        [XperienceSettingsData("Baseline.Account.AccessDeniedUrl", "")]
        public virtual string AccessDeniedUrl { get; set; } = string.Empty;
    }
}
