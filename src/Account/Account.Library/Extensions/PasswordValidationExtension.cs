using FluentValidation;

namespace Account.Extensions
{
    public static class PasswordValidationExtension
    {
        public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> ruleBuilder, PasswordPolicySettings settings)
        {
            string message = settings.ViolationMessage.GetValueOrDefault("Invalid Password");
            var options = ruleBuilder.NotNull();
            if (settings.UsePasswordPolicy)
            {
                if (settings.MinLength.TryGetValue(out var minLength) && minLength > 0)
                {
                    options.MinimumLength(minLength).WithMessage(message);
                }
                if (settings.Regex.TryGetValue(out var regex))
                {
                    options.Matches(regex).WithMessage(message);
                }
                if (settings.NumNonAlphanumericChars.TryGetValue(out var numNonAlphaNumericaChars))
                {
                    options.Matches($"^(?=.{{{numNonAlphaNumericaChars},999}}\\W).*$").WithMessage(message);
                }
            }
            return options;
        }

    }
}
