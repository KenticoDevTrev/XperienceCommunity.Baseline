# Installation for Kentico Xperience 13

Unlike the other modules, the value of the Account Module is less in the `RCL` features, but more in the libraries.  Because of this, the hookups are found in the `XperienceCommunity.Baseline.Account.Library.KX13` package, and you can optionally install the `RCL` if you wish.

If you do wish to use the `RCL` libraries, the nuget packages have most of the items *except* a special page type `Generic.Account` which it uses for the Page Builder version of the Account Pages


## 1. Add Nuget Packages

Install the `XperienceCommunity.Baseline.Account.Library.KX13` nuget package on your main MVC Site project.

Optionally, you can install in addition or in place of the above the package `XperienceCommunity.Baseline.Account.RCL.KX13`

[See the Modules Architecture Overview](../general/modules-architecture-overview.md) if you wish to install individual packages on your libraries based on dependencies.

## 2. Settings Add Optional Account Page Type

Log into the Kentico Admin, and proceed to Site -> Import Sites and Objects

Grab the [Baseline_Generics.1.0.0.zip](../../starting-site/kx13/Baseline_Generics.1.0.0.zip) file from this repository, and import it.

Make sure to import the Settings Keys as they contain the Url Property defaults.

Optionally if using the `RCL` libraries, you will want the Page Type `Account` also imported.

## 3. CI/CD Setup (Main)

The Account system is tightly coupled with Site Members and the ASP.Net Identity.  For this reason, you'll want to be sure to hook up the configuration ***In place*** of Kentico's default hookup.

Here's the Account Hookup (extensions in `Account` Namespace)

```csharp
services.AddLocalization()
        .AddXperienceLocalizer()
        .AddControllersWithViewsAndKenticoAuthorization()
        .AddViewLocalization()
        .AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (type, factory) =>
            {
                return factory.Create(typeof(SharedResources));
            };
        });

        // Configure Baseline Account further
        services.AddBaselineAccountAuthentication<ApplicationUser, ApplicationRole>(
            configuration: Configuration,
            identityOptions: options => {
                // Customize IdentityOptions, including Password Settings (fall back if no ChannelSettings is defined
                identityOptions.Password.RequireDigit = false;
                identityOptions.Password.RequireLowercase = false;
                identityOptions.Password.RequireNonAlphanumeric = false;
                identityOptions.Password.RequireUppercase = false;
                identityOptions.Password.RequiredLength = 0;
            },
            authenticationConfigurations: options => {
                // Customize Baseline's Authentication settings, including two form and additional roles per Auth Type
                authenticationConfigurations.UseTwoFormAuthentication = true;
            },
            cookieConfigurations: options => {
                // Customize Cookie Auth Options
                // Note that the LoginPath/LogoutPath/AccessDeniedPath are handled in SiteSettingsCookieAuthenticationEvents
                // These are not part of standard kentico but 
                if (builder.Environment.IsDevelopment()) {
                    cookieAuthenticationOptions.Cookie.SecurePolicy = CookieSecurePolicy.None;
                    cookieAuthenticationOptions.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                } else {
                    cookieAuthenticationOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    cookieAuthenticationOptions.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                }
            },
            AUTHENTICATION_COOKIE_NAME: "identity.authentication",
            defaultLoginUrl: "/Account/LogIn",
            defaultLogOutUrl:"/Account/LogOut",
            defaultAccessDeniedPath: "/Error/403");
```

Here on the other hand, is the **KENTICO DEFAULT** hookup:

```csharp
services.AddLocalization()
        .AddXperienceLocalizer()
        .AddControllersWithViews()
        .AddViewLocalization()
        .AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (type, factory) =>
            {
                return factory.Create(typeof(SharedResources));
            };
        });
```


## 4. CI/CD Setup (RCL)

If also configuring the RCL portion, also call this (under `Account` namespace)

```csharp
services.AddBaselineAccountRcl()
```

## 4. Page Template

If you are using the Account `RCL` packages, please make sure to add the Page Templates for the Account Page Type, this will allow you to add the Account Page and select the various templates to place the different features on your site.

``` csharp

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
