# Assembly Tags
Below are the various assembly tags that you will want to include in your project in order to leverage the Page Templates, Widgets, etc.

```csharp

[assembly: RegisterPageTemplate(
    "Generic.Account_Confirmation",
    "Registration Confirmation",
    typeof(ConfirmationPageTemplateProperties),
    "/Features/Account/Confirmation/ConfirmationPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]

[assembly: RegisterPageTemplate(
    "Generic.Account_ForgotPassword",
    "Forgot Password",
    typeof(ForgotPasswordPageTemplateProperties),
    "/Features/Account/ForgotPassword/ForgotPasswordPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]


[assembly: RegisterPageTemplate(
    "Generic.Account_ForgottenPasswordReset",
    "Forgotten Password Reset",
    typeof(ForgottenPasswordResetPageTemplateProperties),
    "/Features/Account/ForgottenPasswordReset/ForgottenPasswordResetPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]

[assembly: RegisterPageTemplate(
    "Generic.Account_LogIn",
    "Log In",
    typeof(LogInPageTemplateProperties),
    "/Features/Account/LogIn/LogInPageTemplate.cshtml")]

[assembly: RegisterPageTemplate(
    "Generic.Account_LogOut",
    "Log Out",
    typeof(LogOutPageTemplateProperties),
    "/Features/Account/LogOut/LogOutPageTemplate.cshtml", 
    ContentTypeNames = ["Generic.Account"])]
[assembly: RegisterPageBuilderAuthorization(pageTemplateIdentifiers: ["Generic.Account_LogOut"], userAuthenticationRequired: true)]

[assembly: RegisterPageTemplate(
    "Generic.Account_MyAccount",
    "My Account",
    typeof(MyAccountPageTemplateProperties),
    "/Features/Account/MyAccount/MyAccountPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]
[assembly: RegisterPageBuilderAuthorization(pageTemplateIdentifiers: ["Generic.Account_MyAccount"], userAuthenticationRequired: true)]

[assembly: RegisterPageTemplate(
    "Generic.Account_Registration",
    "Registration",
    typeof(RegistrationPageTemplateProperties),
    "/Features/Account/Registration/RegistrationPageTemplate.cshtml", 
    ContentTypeNames = ["Generic.Account"])]

[assembly: RegisterPageTemplate(
    "Generic.Account_ResetPassword",
    "Reset Password",
    typeof(ResetPasswordPageTemplateProperties),
    "/Features/Account/ResetPassword/ResetPasswordPageTemplate.cshtml",
    ContentTypeNames = ["Generic.Account"])]
[assembly: RegisterPageBuilderAuthorization(pageTemplateIdentifiers: ["Generic.Account_ResetPassword"], userAuthenticationRequired: true)]
```