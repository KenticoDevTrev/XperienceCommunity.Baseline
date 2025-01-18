# Account Features Overview

Here's a brief list of the features the Account system shows.  

- [Registration](../../src/Account/Account.RCL/Features/Account/Registration/RegistrationController.cs)
  - [Email Confirmation](../../src/Account/Account.RCL/Features/Account/Confirmation/ConfirmationController.cs)
  - [Resend Email Confirmation](../../src/Account/Account.RCL/Features/Account/Registration/RegistrationController.cs)
- [Logging In](../../src/Account/Account.RCL/Features/Account/LogIn/LogInController.cs)
  - [Form Based Login (username + password)]((../../src/Account/Account.RCL/Features/Account/LogIn/LogInController.cs))
  - [Optional Two factor Authentication (Email Token)](../../src/Account/Account.RCL/Features/Account/LogIn/LogInController.cs)
  - [External Provider Login](account-external-providers.md) (Facebook, Google, Microsoft and Twitter/X)
  - [Forgot Password](../../src/Account/Account.RCL/Features/Account/ForgotPasswordReset/ForgotPasswordResetController.cs)
  - [ReturnUrl Logic](../../src/Account/Account.RCL/Features/Account/LogIn/LogInController.cs)
- [Password Reset](../../src/Account/Account.RCL/Features/Account/ResetPassword/ResetPasswordController.cs)
- [Log Out](../../src/Account/Account.RCL/Features/Account/LogOut/LogOutController.cs)
- [My Account with Redirect](../../src/Account/Account.RCL/Features/Account/LogOut/LogOutController.cs)