using Account.Admin.Xperience.Models;
using Account.Models;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.ChannelSettings.Admin.UI.ChannelCustomSettings;
using XperienceCommunity.ChannelSettings.Repositories;

[assembly: UIPage(parentType: typeof(Kentico.Xperience.Admin.Base.UIPages.ChannelEditSection),
    slug: "account-channel-custom-settings",
    uiPageType: typeof(AccountChannelSettingsExtender),
    name: "Account URL Settings",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]

namespace Account.Admin.Xperience.Models
{

    public class AccountChannelSettingsExtender(Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
                                                                     IFormDataBinder formDataBinder,
                                                                     IChannelCustomSettingsRepository customChannelSettingsRepository,
                                                                     IChannelSettingsInternalHelper channelCustomSettingsInfoHandler,
                                                                     IInfoProvider<ChannelInfo> channelInfoProvider)
        // Change type below to your settings
        : ChannelCustomSettingsPage<AccountChannelSettingsFormAnnotated>(formItemCollectionProvider, formDataBinder, customChannelSettingsRepository, channelCustomSettingsInfoHandler, channelInfoProvider)
    {
    }


    public class AccountChannelSettingsFormAnnotated : AccountChannelSettings
    {

        [CheckBoxComponent(Label = "Redirect To Account After Login", Order = 1, ExplanationText = "If the user should be directed to the \"My Account\" page upon login (unless logging in after hitting a restricted page).")]
        public override bool AccountRedirectToAccountAfterLogin { get; set; } = false;

        [TextInputComponent(Label = "Registration Url", Order = 2, ExplanationText = "Url to the Account page with \"Registration\" template, if not provided will use the default /Account/Registration")]
        public override string AccountRegistrationUrl { get; set; } = string.Empty;

        [TextInputComponent(Label = "Registration Confirmation Url", Order = 3, ExplanationText = "Url to the Account page with \"Registration Confirmation\" template, if not provided will use the default /Account/Confirmation")]
        public override string AccountConfirmationUrl { get; set; } = string.Empty;

        [TextInputComponent(Label = "Log in Url", Order = 4, ExplanationText = "Url to the Account page with \"Log In\" template, if not provided will use the default /Account/LogIn")]
        public override string AccountLoginUrl { get; set; } = string.Empty;

        [TextInputComponent(Label = "My Account Url", Order = 5, ExplanationText = "Url to the Account page with \"My Account\" template, if not provided will use the default /Account/MyAccount")]
        public override string AccountMyAccountUrl { get; set; } = string.Empty;

        [TextInputComponent(Label = "Reset Password Url", Order = 6, ExplanationText = "Url to the Account page with \"Reset Password\" template, if not provided will use the default /Account/ResetPassword")]
        public override string AccountResetPassword { get; set; } = string.Empty;

        [TextInputComponent(Label = "Log Out Url", Order = 7, ExplanationText = "Url to the Account page with \"Log Out\" template, if not provided will use the default /Account/LogOut")]
        public override string AccountLogOutUrl { get; set; } = string.Empty;

        [TextInputComponent(Label = "Forgot Password Url", Order = 8, ExplanationText = "Url to the Account page with \"Forgot Password\" template, if not provided will use the default /Account/ForgotPassword")]
        public override string AccountForgotPasswordUrl { get; set; } = string.Empty;

        [TextInputComponent(Label = "Forgotten Password Reset Url", Order = 9, ExplanationText = "Url to the Account page with \"Forgotten Password Reset\" template, if not provided will use the default /Account/ForgottenPasswordReset")]
        public override string AccountForgottenPasswordResetUrl { get; set; } = string.Empty;

        [TextInputComponent(Label = "Access Denied Url", Order = 10, ExplanationText = "Url when someone is logged in but is denied access to the page. If not provided will use the default /error/403 (or whatever your 403 error handling page is)")]
        public override string AccessDeniedUrl { get; set; } = string.Empty;

    }
}
