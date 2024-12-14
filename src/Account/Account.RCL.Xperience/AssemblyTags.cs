// These are the page template registrations
/*
using Account.Features.Account.Confirmation;
using Account.Features.Account.ForgotPassword;
using Account.Features.Account.ForgottenPasswordReset;
using Account.Features.Account.LogIn;
using Account.Features.Account.LogOut;
using Account.Features.Account.MyAccount;
using Account.Features.Account.Registration;
using Account.Features.Account.ResetPassword;
using XperienceCommunity.Authorization;

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_Confirmation",
    name: "Registration Confirmation",
    propertiesType: typeof(ConfirmationPageTemplateProperties),
    customViewName: "/Features/Account/Confirmation/ConfirmationPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_ForgotPassword",
    name: "Forgot Password",
    propertiesType: typeof(ForgotPasswordPageTemplateProperties),
    customViewName: "/Features/Account/ForgotPassword/ForgotPasswordPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]


[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_ForgottenPasswordReset",
    name: "Forgotten Password Reset",
    propertiesType: typeof(ForgottenPasswordResetPageTemplateProperties),
    customViewName: "/Features/Account/ForgottenPasswordReset/ForgottenPasswordResetPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_LogIn",
    name: "Log In",
    propertiesType: typeof(LogInPageTemplateProperties),
    customViewName: "/Features/Account/LogIn/LogInPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_TwoFormAuthentication",
    name: "Two Form Authentication",
    propertiesType: typeof(TwoFormAuthenticationPageTemplateProperties),
    customViewName: "/Features/Account/LogIn/TwoFormAuthenticationPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_LogOut",
    name: "Log Out",
    propertiesType: typeof(LogOutPageTemplateProperties),
    customViewName: "/Features/Account/LogOut/LogOutPageTemplate.cshtml", 
    ContentTypeNames = ["Generic.Account"])]
[assembly: RegisterPageBuilderAuthorization(pageTemplateIdentifiers: ["Generic.Account_LogOut"], userAuthenticationRequired: true)]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_MyAccount",
    name: "My Account",
    propertiesType: typeof(MyAccountPageTemplateProperties),
    customViewName: "/Features/Account/MyAccount/MyAccountPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]
[assembly: RegisterPageBuilderAuthorization(pageTemplateIdentifiers: ["Generic.Account_MyAccount"], userAuthenticationRequired: true)]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_Registration",
    name: "Registration",
    propertiesType: typeof(RegistrationPageTemplateProperties),
    customViewName: "/Features/Account/Registration/RegistrationPageTemplate.cshtml", 
    ContentTypeNames = ["Generic.Account"])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_ResetPassword",
    name: "Reset Password",
    propertiesType: typeof(ResetPasswordPageTemplateProperties),
    customViewName: "/Features/Account/ResetPassword/ResetPasswordPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]
[assembly: RegisterPageBuilderAuthorization(pageTemplateIdentifiers: ["Generic.Account_ResetPassword"], userAuthenticationRequired: true)]
*/