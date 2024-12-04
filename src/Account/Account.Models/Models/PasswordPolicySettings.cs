namespace Account.Models
{
    public record PasswordPolicySettings
    {
        public PasswordPolicySettings()
        {
            UsePasswordPolicy = false;
        }
        public PasswordPolicySettings(bool usePasswordPolicy, int? minLength = null, int? numNonAlphanumericChars = null, string? regex = null, string? violationMessage = null, int? uniqueChars = null)
        {
            UsePasswordPolicy = usePasswordPolicy;
            MinLength = minLength.AsMaybe();
            NumNonAlphanumericChars = numNonAlphanumericChars.AsMaybe();
            Regex = regex.AsNullOrWhitespaceMaybe();
            ViolationMessage = violationMessage.AsNullOrWhitespaceMaybe();
            UniqueChars = uniqueChars.AsMaybe();
        }

        public bool UsePasswordPolicy { get; init; }
        public Maybe<int> MinLength { get; init; } = Maybe.None;
        public Maybe<int> NumNonAlphanumericChars { get; init; } = Maybe.None;
        public Maybe<int> UniqueChars { get; init; } = Maybe.None;
        public Maybe<string> Regex { get; init; } = Maybe.None;
        public Maybe<string> ViolationMessage { get; init; } = Maybe.None;
    }
}
