# User and Role Management

The Baseline's Account system provides a plethora of Services and Repositories for all your user and role management needs.

## User Management

It would take a while to comment on every function, but each function has Class comments on them, so i'll just list them out and you can review them, most are pretty self-explanitory from the name

- [IUserService](../../src/Account/Account.Models/Services/IUserService.cs)
  - Registration
    - **SendRegistrationConfirmationEmailAsync**
    - **ConfirmRegistrationConfirmationTokenAsync**
  - User Creation
    - **CreateUser**
    - **CreateExternalUser**
  - Password Reset
    - **SendPasswordResetEmailAsync**
    - **ResetPasswordFromTokenAsync**
  - Password Validity Check
    - **ValidatePasswordPolicyAsync**
  - User Password Check
    - **ValidateUserPasswordAsync**
  - Reset Password
    - **ResetPasswordAsync**
  - Two Step Verification Code
    - **SendVerificationCodeEmailAsync**

- [IUserManagerService](../../src/Account/Account.Models/Services/IUserManagerService.cs)
  - Most of the below have a "ByName" "ByEmail" "ById" and "ByLogin" variant
  - **CheckPasswordBy____Async**: Returns if password is a match
  - **GenerateTwoFactorTokenBy____Async**: Generates a Two Factor Token
  - **VerifyTwoFactorTokenBy____Async**: Validates that Two Factor Token
  - **UserExistsBy___Async**: Returns true if user exists
  - **GetUserIDBy____Async**: Gets the UserID given the information
  - **EnableUserBy____Async**: Enables the User (usually after email confirmation)
  - **GetSecurityStampAsync**: By username only, gets the [security stamp](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.identityuser-1.securitystamp?view=aspnetcore-8.0)

- [ISignInManagerService](../../src/Account/Account.Models/Services/ISignInManagerService.cs)
  - Most of the below have a "ByName", "ByEmail", "ById", and "ByLogin" variant
  - **IsTwoFactorClientRememberedBy____Async**: If two factor is saved and remembered for the user (check before doing two factor prompt)
  - **RememberTwoFactorClientRememberedBy___Async**: Sets that the two factor client is remembered for the user (use after two factor authentication successful)
  - **SignInBy___Async**: Signs in the user
  - **PasswordSignInBy___Async**: Signs in the user if the given password is correct
  - **SignOutAsync**: Signs the user out
  - **ConfigureExternalAuthenticationProperties**: Configures the redirect URL and user identifier for the specified external login
  - **GetExternalLoginInfoAsync**: Attempts to get the External Login Information of the current user (Result.Failure if none)
  - **GetExternalAuthenticationSchemesAsync**: Gets the Authentication Schemas available (various .net Identity providers, like Google, Facebook, etc.  Configured through applicationsettings)

- [IRoleService](../../src/Account/Account.Models/Services/IRoleService.cs)
  - **SetUserRole**: Adds the role to the user
  - **CreateRoleIfNotExisting**: Creates the given role for the site if it doesn't exist.

## Role Managment

- [IRoleRepository](../../src/Account/Account.Models/Repositories/IRoleRepository.cs)
  - **GetRoleAsync**
  - **UserInRoleAsync** 
  - **UserHasPermissionAsync** Checks current user against Resource Name and Permission (NOTE: Not supported in Xperience by Kentico, will throw `NotSupportedException`)
