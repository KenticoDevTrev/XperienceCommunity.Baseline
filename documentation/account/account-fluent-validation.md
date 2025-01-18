# Fluent Validation

The Baseline Account uses Fluent Validation for to help do asynchronous validation rules when needed.

It also has extensions such as `ValidPassword` for creating Rules for Password that hook into all the accounts:

Below is a sample, but please see [Fluent Documentation](https://docs.fluentvalidation.net/en/latest/) for all uses
```csharp
   public class ForgottenPasswordResetViewModelValidator : AbstractValidator<ForgottenPasswordResetViewModel>
   {
       public ForgottenPasswordResetViewModelValidator(IAccountSettingsRepository _accountSettingsRepository)
       {
           var passwordSettings = _accountSettingsRepository.GetPasswordPolicy();

           RuleFor(model => model.Password).ValidPassword(passwordSettings);
           RuleFor(model => model.PasswordConfirm).Equal(model => model.Password);

       }
   }

   // Registered through CI/CD
   services.AddScoped<IValidator<ForgottenPasswordResetViewModel>, ForgottenPasswordResetViewModelValidator>()
```