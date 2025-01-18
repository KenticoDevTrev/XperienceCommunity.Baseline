# External Providers

The Baseline also shows you how you can easily set up Oath External Providers thanks to the Microsoft.AspNetCore.Authentication Packages.

There are 4 supported providers:

- Facebook
- Google
- Microsoft
- Twitter

## Configuring Providers

Configuration is relatively easy, it's all done through the `appsettings.json`, simply add any of these Authentication properties and the Baseline does the rest:

```javascript
{
  /* For Authentication Module, enable whichever you wish to leverage, follow https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-8.0&tabs=visual-studio for how to generate them.*/
  "Authentication": {
    "Google": {
      "ClientId": "000000000000-123456789abcdefghijklmnopqrstuvw.apps.googleusercontent.com",
      "ClientSecret": "SOMETH-_Ng0123456789ABcDeFgHiJk_LM0"
    },
    "Facebook": {
      "AppId": "0000000000000000",
      "AppSecret": "123456789abcdefghijklmnopqrstuvw"
    },
    "Twitter": {
      "APIKey": "Something0123456789Here00",
      "APIKeySecret": "1a2s3d4f5g6h7j8k9l0abcdefghijklmnopqrstuvwxyz12345",
      "CallbackPath": "/signin-twitter"
    },
    "Microsoft": {
      "ClientId": "00000000-1111-2222-3333-44aabbccdefg",
      "ClientSecret": "abC23~456789DeFgHiJkLmNoPQrStUvW~x~y~z01",
      "CallbackPath": "/signin-microsoft"
    }
  }
}
```

## Retrieving configuration and executing them

A sample of how this works can be seen in the [LogInViewComponent](../../src/Account/Account.RCL/Features/Account/LogIn/LogInViewComponent.cs) and it's view [LogIn.cshtml](../../src/Account/Account.RCL/Features/Account/LogIn/LogIn.cshtml)

Use the `ISignInManagerService.GetExternalAuthenticationSchemesAsync()` to retrieve the External Login Providers.

Loop through and put in the Provider Name you want to use, this will post to the [LogInController.ExternalLogIn](../../src/Account/Account.RCL/Features/Account/LogIn/LogInController.cs), which will configure and return the challenge.

Upon authenticating, it will redirect to the [LogInController.ExternalLogInCallback](../../src/Account/Account.RCL/Features/Account/LogIn/LogInController.cs) and handle the information, validating the Oath and allowing you to perform the login/registration