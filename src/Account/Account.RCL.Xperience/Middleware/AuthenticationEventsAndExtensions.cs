using Account.Features.Account.ForgottenPasswordReset;
using Account.Features.Account.Registration;
using Account.Features.Account.ResetPassword;
using Account.Models;
using Core.Models;
using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class AuthenticationServiceExtensions

    {
        public static WebApplicationBuilder AddBaselineAccountRcl<TGenericUser>(this WebApplicationBuilder builder) 
            where TGenericUser : User, new()
        {
            
            // Register Validators from Fluent Validation
            builder.Services.AddScoped<IValidator<BasicUser>, BasicUserValidator<TGenericUser>>()
                .AddScoped<IValidator<ForgottenPasswordResetViewModel>, ForgottenPasswordResetViewModelValidator>()
                .AddScoped<IValidator<RegistrationViewModel>, RegistrationViewModelValidator<TGenericUser>>()
                .AddScoped<IValidator<ResetPasswordViewModel>, ResetPasswordValidator<TGenericUser>>();

            return builder;
        }
    }
}
