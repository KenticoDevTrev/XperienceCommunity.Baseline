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
                    options.MinimumLength(minLength).WithMessage($"Password minimum Length is {minLength}.");
                }
                if(settings.UniqueChars.TryGetValue(out var uniqueChars) && uniqueChars > 0) {
                    options.Must(x => x.ToCharArray().Distinct().Count() >= uniqueChars).WithMessage($"Must have at least {uniqueChars} unique characters.");
                }
                if (settings.Regex.TryGetValue(out var regex))
                {
                    options.Matches(regex).WithMessage(message);

                    // If both regex and numNon, use 'Must' in this case so HTML validation elements won't have 2 regex
                    if (settings.NumNonAlphanumericChars.TryGetValue(out var numNonAlphaNumericaChars)) {
                        options.Must(password => {
                            int counter = 0;
                            foreach (char c in password) {
                                if (!Char.IsLetterOrDigit(c)) {
                                    counter++;
                                }
                            }
                            return counter >= numNonAlphaNumericaChars;
                        }).WithMessage($"Must have at least {numNonAlphaNumericaChars} Non Alpha-Numeric Character{(numNonAlphaNumericaChars > 1 ? "s" : "")}.");
                    }
                } 
                else if (settings.NumNonAlphanumericChars.TryGetValue(out var numNonAlphaNumericaChars))
                {
                    options.Matches($"^(?=.{{{numNonAlphaNumericaChars},999}}\\W).*$").WithMessage($"Must have at least {numNonAlphaNumericaChars} Non Alpha-Numeric Character{(numNonAlphaNumericaChars > 1 ? "s" : "")}.");
                }
            }
            return options;
        }

    }
}
