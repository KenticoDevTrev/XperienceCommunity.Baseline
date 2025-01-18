# Installation for Xperience by Kentico

Unlike the other modules, the value of the Account Module is less in the `RCL` features, but more in the libraries.  Because of this, the hookups are found in the `XperienceCommunity.Baseline.Account.Library.Xperience` package, and you can optionally install the `RCL` if you wish.

## 1. Add Nuget Packages

On the MVC Site
```
npm install XperienceCommunity.Baseline.Account.Library.Xperience
```

Optionally, if you wish to use the given Controllers and page Templates, install the RCL package as well (or instead as this RCL inherits from the XperienceCommunity.Baseline.Account.Library.Xperience)

```
npm install XperienceCommunity.Baseline.Account.RCL.Xperience
```

[See the Modules Architecture Overview](../general/modules-architecture-overview.md) if you wish to install individual packages on your libraries based on dependencies.

## 2. CI/CD Setup (Main)

The Account system is tightly coupled with Site Members and the ASP.Net Identity.  For this reason, you'll want to be sure to hook up the configuration ***In place*** of Kentico's default hookup.

Here's the Account Hookup (in a wrapper function to show things better)

```csharp
/// <summary>
/// BASELINE CUSTOMIZATION - Account - Configure this method for your own uses
/// Use this if using the Baseline Account Module to hook up Member Roles, Authorization, and Logins
/// </summary>
/// <param name="builder"></param>
public static void AddBaselineAccountIdentity<TUser, TRole>(WebApplicationBuilder builder) where TUser : ApplicationUser, new() where TRole : TagApplicationUserRole, new()
{
    // If you wish to hook up the various Microsoft.AspNetCore.Authentication types (Facebook, Google, MicrosoftAccount, Twitter),
    // please see this documentation https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-8.0&tabs=visual-studio and update your AppSettings.json with the IDs

    // NOTE: While you create your own ApplicationUser variant, if you use a Generic User Model other than Core.Model.User as your User type,
    // then you will NOT be able to leverage the Baseline.Account.RCL, and will need to clone that project, as you have to inject the
    // IUserService<User> and IUserRepository<User> of the same type you define here, it won't work if the types miss-matched
    builder.AddBaselineAccountAuthentication<TUser, TRole>(
    identityOptions => {
        // Customize IdentityOptions, including Password Settings (fall back if no ChannelSettings is defined
        identityOptions.Password.RequireDigit = false;
        identityOptions.Password.RequireLowercase = false;
        identityOptions.Password.RequireNonAlphanumeric = false;
        identityOptions.Password.RequireUppercase = false;
        identityOptions.Password.RequiredLength = 0;
    },
    authenticationConfigurations => {
        // Customize Baseline's Authentication settings, including two form and additional roles per Auth Type
        authenticationConfigurations.UseTwoFormAuthentication = true;
    },
    cookieAuthenticationOptions => {
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
    }
    );

    /* To define Channel Specific Password Settings, add the below assembly tag somewhere, then in Xperience Admin select your Channel, the side menu will show these settings configurations */
    /*
     [assembly: UIPage(parentType: typeof(Kentico.Xperience.Admin.Base.UIPages.ChannelEditSection),
        slug: "member-password-channel-custom-settings",
        uiPageType: typeof(MemberPasswordChannelSettingsExtender),
        name: "Member Password Settings",
        templateName: TemplateNames.EDIT,
        order: UIPageOrder.NoOrder)]
    */

    // If you are leveraging the Account.RCL controllers and such, use this to hook up the validation, and optionally installs the Generic.Account WebPage (will still need to add the RegisterPageTemplates, see AssemblyTags.md)
    builder.AddBaselineAccountRcl(new BaselineAccountOptions(UseAccountWebpageType: true));

    builder.Services.Configure<AdminIdentityOptions>(options => {
        // The expiration time span for admin. In production environment, set expiration according to best practices.
        options.AuthenticationOptions.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

    builder.Services.AddAuthorization();
}
```

Here on the other hand, is the **KENTICO DEFAULT** hookup:

```csharp
/// <summary>
/// Adds the standard Kentico identity (based roughly off of the Dancing Goat Sample Site) - Do not use if using Account module!
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TRole"></typeparam>
/// <param name="builder"></param>
public static void AddStandardKenticoIdentity<TUser, TRole>(WebApplicationBuilder builder) where TUser : ApplicationUser, new() where TRole : TagApplicationUserRole, new()
{
    // Adds and configures ASP.NET Identity for the application
    // XperienceCommunity.MemberRoles, make sure Role is TagApplicationUserRole or an inherited member here
    builder.Services.AddIdentity<TUser, TRole>(options => {
        // Ensures that disabled member accounts cannot sign in
        options.SignIn.RequireConfirmedAccount = true;
        // Ensures unique emails for registered accounts
        options.User.RequireUniqueEmail = true;

        // Note: Can customize password requirements here, there is no longer a 'settings' in Kentico for this.
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 10;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredUniqueChars = 3;
    })
        .AddUserStore<ApplicationUserStore<TUser>>()
        .AddMemberRolesStores<TUser, TRole>() // XperienceCommunity.MemberRoles
        .AddUserManager<UserManager<TUser>>()
        .AddSignInManager<SignInManager<TUser>>();

    // Adds authorization support to the app
    builder.Services.ConfigureApplicationCookie(options => {
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.LoginPath = new PathString("/Account/Signin");
        options.AccessDeniedPath = new PathString("/Error/403");
        options.Events.OnRedirectToAccessDenied = ctx => {
            var factory = ctx.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = factory.GetUrlHelper(new ActionContext(ctx.HttpContext, new RouteData(ctx.HttpContext.Request.RouteValues), new ActionDescriptor()));
            var url = urlHelper.Action("Signin", "Account") + new Uri(ctx.RedirectUri).Query;

            ctx.Response.Redirect(url);

            return Task.CompletedTask;
        };
        // These are not part of standard kentico but 
        if (builder.Environment.IsDevelopment()) {
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        } else {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        }
    });

    builder.Services.Configure<AdminIdentityOptions>(options => {
        // The expiration time span for admin. In production environment, set expiration according to best practices.
        options.AuthenticationOptions.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

    builder.Services.AddAuthorization();
}
```
## 3. CI/CD (RCL)

If also configuring the RCL portion, also call this (under `Account` namespace)

```csharp
services.AddBaselineAccountRcl()
```

## 4. Channel Settings for Password Management

If you want to manage Password settings per channel (similar to Kentico Xperience 13's Settings), add the below

```csharp
// BASELINE CUSTOMIZATION - Account - Add this to edit Channel Settings
[assembly: UIPage(parentType: typeof(Kentico.Xperience.Admin.Base.UIPages.ChannelEditSection),
    slug: "member-password-channel-custom-settings",
    uiPageType: typeof(MemberPasswordChannelSettingsExtender),
    name: "Member Password Settings",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]
```

These can then be configured by going to Admin -> Configuration -> Channel Management -> [Select your web channel] -> Member Password Settings


## 5. Page Templates and enable Page Builder

If you are using the Account `RCL` packages, please make sure to add the Page Templates for the Account Page Type, this will allow you to add the Account Page and select the various templates to place the different features on your site.

```csharp
// BASELINE CUSTOMIZATION - Account - Add this to edit Channel Settings
[assembly: UIPage(parentType: typeof(Kentico.Xperience.Admin.Base.UIPages.ChannelEditSection),
    slug: "member-password-channel-custom-settings",
    uiPageType: typeof(MemberPasswordChannelSettingsExtender),
    name: "Member Password Settings",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]

// BASELINE CUSTOMIZATION - Account - Here are the page template registrations 
[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_Confirmation",
    name: "Registration Confirmation",
    propertiesType: typeof(ConfirmationPageTemplateProperties),
    customViewName: "/Features/Account/Confirmation/ConfirmationPageTemplate.cshtml",
    ContentTypeNames = [Generic.Account.CONTENT_TYPE_NAME])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_ForgotPassword",
    name: "Forgot Password",
    propertiesType: typeof(ForgotPasswordPageTemplateProperties),
    customViewName: "/Features/Account/ForgotPassword/ForgotPasswordPageTemplate.cshtml",
    ContentTypeNames = [Generic.Account.CONTENT_TYPE_NAME])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_ForgottenPasswordReset",
    name: "Forgotten Password Reset",
    propertiesType: typeof(ForgottenPasswordResetPageTemplateProperties),
    customViewName: "/Features/Account/ForgottenPasswordReset/ForgottenPasswordResetPageTemplate.cshtml",
    ContentTypeNames = [Generic.Account.CONTENT_TYPE_NAME])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_LogIn",
    name: "Log In",
    propertiesType: typeof(LogInPageTemplateProperties),
    customViewName: "/Features/Account/LogIn/LogInPageTemplate.cshtml",
    ContentTypeNames = [Generic.Account.CONTENT_TYPE_NAME])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_TwoFormAuthentication",
    name: "Two Form Authentication",
    propertiesType: typeof(TwoFormAuthenticationPageTemplateProperties),
    customViewName: "/Features/Account/LogIn/TwoFormAuthenticationPageTemplate.cshtml",
    ContentTypeNames = [Generic.Account.CONTENT_TYPE_NAME])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_LogOut",
    name: "Log Out",
    propertiesType: typeof(LogOutPageTemplateProperties),
    customViewName: "/Features/Account/LogOut/LogOutPageTemplate.cshtml",
    ContentTypeNames = [Generic.Account.CONTENT_TYPE_NAME])]
[assembly: RegisterPageBuilderAuthorization(pageTemplateIdentifiers: ["Generic.Account_LogOut"], userAuthenticationRequired: true)]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_MyAccount",
    name: "My Account",
    propertiesType: typeof(MyAccountPageTemplateProperties),
    customViewName: "/Features/Account/MyAccount/MyAccountPageTemplate.cshtml",
    ContentTypeNames = [Generic.Account.CONTENT_TYPE_NAME])]
[assembly: RegisterPageBuilderAuthorization(pageTemplateIdentifiers: ["Generic.Account_MyAccount"], userAuthenticationRequired: true)]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_Registration",
    name: "Registration",
    propertiesType: typeof(RegistrationPageTemplateProperties),
    customViewName: "/Features/Account/Registration/RegistrationPageTemplate.cshtml",
    ContentTypeNames = [Generic.Account.CONTENT_TYPE_NAME])]

[assembly: RegisterPageTemplate(
    identifier: "Generic.Account_ResetPassword",
    name: "Reset Password",
    propertiesType: typeof(ResetPasswordPageTemplateProperties),
    customViewName: "/Features/Account/ResetPassword/ResetPasswordPageTemplate.cshtml",
    ContentTypeNames = [Generic.Account.CONTENT_TYPE_NAME])]
[assembly: RegisterPageBuilderAuthorization(pageTemplateIdentifiers: ["Generic.Account_ResetPassword"], userAuthenticationRequired: true)]
```

```csharp
// Enable desired Kentico Xperience features
builder.Services.AddKentico(features => {
    features.UsePageBuilder(new PageBuilderOptions {
        ContentTypeNames =
        [
            // Enables Page Builder for content types using their generated classes
            
            ...
           
            // BASELINE CUSTOMIZATION - Account - If using Accounts with Account Type, MUST add it here
            Generic.Account.CONTENT_TYPE_NAME,
        ]
    })
});
```