using Account.Features.Account.ForgottenPasswordReset;
using Account.Features.Account.Registration;
using Account.Features.Account.ResetPassword;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Account
{
    public static class AuthenticationServiceExtensions

    {
        public static WebApplicationBuilder AddBaselineAccountRcl<TGenericUser>(this WebApplicationBuilder builder)
            where TGenericUser : User, new()
        {
            // Register Validators from Fluent Validation
            builder.Services.AddScoped<IValidator<BasicUser>, BasicUserValidator>()
                .AddScoped<IValidator<ForgottenPasswordResetViewModel>, ForgottenPasswordResetViewModelValidator>()
                .AddScoped<IValidator<RegistrationViewModel>, RegistrationViewModelValidator>()
                .AddScoped<IValidator<ResetPasswordViewModel>, ResetPasswordValidator>();

            return builder;
        }
    }
}
