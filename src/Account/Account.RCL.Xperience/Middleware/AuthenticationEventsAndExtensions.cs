using Account.Features.Account.ForgottenPasswordReset;
using Account.Features.Account.Registration;
using Account.Features.Account.ResetPassword;
using Account.Installers;
using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class AuthenticationServiceExtensions

    {
        public static WebApplicationBuilder AddBaselineAccountRcl(this WebApplicationBuilder builder, BaselineAccountOptions baselineOptions) 
        {
            // Register installer and options
            builder.Services.AddSingleton(baselineOptions)
                .AddSingleton<BaselineAccountModuleInstaller>();

            // Register Validators from Fluent Validation
            builder.Services.AddScoped<IValidator<BasicUser>, BasicUserValidator>()
                .AddScoped<IValidator<ForgottenPasswordResetViewModel>, ForgottenPasswordResetViewModelValidator>()
                .AddScoped<IValidator<RegistrationViewModel>, RegistrationViewModelValidator>()
                .AddScoped<IValidator<ResetPasswordViewModel>, ResetPasswordValidator>();

            return builder;
        }
    }
}
