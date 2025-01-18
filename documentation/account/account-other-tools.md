# Other Tools

Aside from those outlined in the other documentations, here are some additional tools.

## Configuration and Settings

These retrieve Configurations and Settings that are stored in either Channel Settings (Xperience by Kentico), Site Settings, appsettings.json, or CI/CD

- [IAuthenticationConfiguration](../../src/Account/Account.Models/Repositories/IAuthenticationConfigurations.cs)
  - Configured in CI/CD (AddBaselineAccountAuthentication(authenticationConfigurations: options => options))
    - **GetExistingInternalUserBehavior**: If an internal user uses an External login, should leave as is or set to external?  (CI/CD Configuration)
    - **AllExternalUserRoles** (Roles given to all external users)
    - **InternalUserRoles** (roles given to all internal users)
    - **FacebookUserRoles** (roles given to facebook login users)
    - **GoogleUserRoles** (roles given to google login users)
    - **MicrosoftUserRoles** (roles given to microsoft login users)
    - **TwitterUserRoles** (roles given to twitter login users)
    - **UseTwoFormAuthentication** (if two form authentication should be used)
- [IAccountSettingsRepository](../../src/Account/Account.Models/Repositories/IAccountSettingsRepository.cs)
  - Configured through Site Settings (Kentico Xpernece 13) or Channel Settings (Xperience by Kentico)
    - **GetPasswordPolicy**
    - **GetPasswordPolicyAsync**
    - **GetAccountRedirectToAccountAfterLoginAsync** (if should redirect to my account after login if no redirect url param present)
    - These are all Urls that you can set for Page Builder Account Pages, or your own custom Controller Routes
        - **GetAccountConfirmationUrlAsync**
        - **GetAccountRegistrationUrlAsync**
        - **GetAccountLoginUrlAsync**
        - **GetAccountTwoFormAuthenticationUrlAsync**
        - **GetAccountLogOutUrlAsync**
        - **GetAccountMyAccountUrlAsync**
        - **GetAccountForgotPasswordUrlAsync**
        - **GetAccountForgottenPasswordResetUrlAsync**
        - **GetAccountResetPasswordUrlAsync**
        - **GetAccessDeniedUrlAsync**

## Models

The Baseline Account comes with it's BasicUser, which contains the Username, UserEmail, FirstName, LastName, and is usable for registration.  This can be mapped to the Core's `User` class.