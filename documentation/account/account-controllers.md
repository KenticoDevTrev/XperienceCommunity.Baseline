# Account Controller

The `Account.RCL` has in it all the Controllers and Views to perform the User Management functions.

Below are the default Routes for the various functions:


- [RegistrationController](../../src/Account/Account.RCL/Features/Account/Registration/RegistrationController.cs)
  - `/Account/Registration` - Registration Page
  - `/Account/ResendRegistration` [POST only] - Regenerates Registration Verification email token if not received
- [ConfirmationController](../../src/Account/Account.RCL/Features/Account/Confirmation/ConfirmationController.cs)
  - `/Account/Confirmation` - Confirmation that Account has been activated
- [LogInController](../../src/Account/Account.RCL/Features/Account/LogIn/LogInController.cs)
  - `/Account/LogIn` - Login Page
  - `/Account/TwoFormAuthentication` - Page where user inputs the 2 Factor Authentication code (email code) - if enabled
- [ForgotPasswordController](../../src/Account/Account.RCL/Features/Account/ForgotPassword/ForgotPasswordController.cs)
  - `/Account/ForgotPassword` - For Requesting a Password Reset Email
- [ForgotPasswordResetController](../../src/Account/Account.RCL/Features/Account/ForgotPasswordReset/ForgotPasswordResetController.cs)
  - `/Account/ForgottenPasswordReset` - Where the Forgotten Password Reset email leads to where they can enter a new password after the token is validated.
- [ResetPasswordController](../../src/Account/Account.RCL/Features/Account/ResetPassword/ResetPasswordController.cs)
  - `/Account/ResetPassword` - If Logged in, allows to reset their password
- [MyAccountController](../../src/Account/Account.RCL/Features/Account/MyAccount/MyAccountController.cs)
  - `/Account/MyAccount` - A user's Account Page
- [LogOutController](../../src/Account/Account.RCL/Features/Account/LogOut/LogOutController.cs)
  - `/Account/LogOut` - Allows user to Log Out

## View Components

Each of these elements has a `____Manual.cshtml` page which calls a View Component to render out the logic.

This is because the Page Template version of each of these also calls that same View Component, allowing both Page Template and Controller to share the same rendering and logic.

## Customization

If you leverage the `RCL` and want to customize the pages, feel free to use the standard [View Overrides](../general/customization-points.md) to place a .cshtml in the same location as the ones in the [Account.RCL Account Feature](../../src/Account/Account.RCL/Features/Account/) Pages.  

- `/Features/Account/Confirmation/Confirmation.cshtml`
- `/Features/Account/ForgotPassword/ForgotPassword.cshtml`
- `/Features/Account/ForgottenPasswordReset/ForgottenPasswordReset.cshtml`
- `/Features/Account/LogIn/LogIn.cshtml`
- `/Features/Account/LogIn/TwoFormAuthentication.cshtml`
- `/Features/Account/LogOut/LogOut.cshtml`
- `/Features/Account/MyAccount/MyAccount.cshtml`
- `/Features/Account/Registration/Registration.cshtml`
- `/Features/Account/ResetPassword/ResetPassword.cshtml`