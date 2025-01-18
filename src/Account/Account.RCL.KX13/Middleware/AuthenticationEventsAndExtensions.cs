using Account.Features.Account.ForgottenPasswordReset;
using Account.Features.Account.Registration;
using Account.Features.Account.ResetPassword;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Account
{
    public static class AuthenticationServiceExtensions

    {
        public static ServiceCollection AddBaselineAccountRcl<TGenericUser>(this ServiceCollection services)
            where TGenericUser : User, new()
        {
            // Register Validators from Fluent Validation
            services.AddScoped<IValidator<BasicUser>, BasicUserValidator>()
                .AddScoped<IValidator<ForgottenPasswordResetViewModel>, ForgottenPasswordResetViewModelValidator>()
                .AddScoped<IValidator<RegistrationViewModel>, RegistrationViewModelValidator>()
                .AddScoped<IValidator<ResetPasswordViewModel>, ResetPasswordValidator>();

            return services;
        }
    }
}
