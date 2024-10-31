namespace Account.Models
{
    public record PasswordPolicySettings
    {
        public PasswordPolicySettings(bool usePasswordPolicy, int? minLength = null, int? numNonAlphanumericChars = null, string? regex = null, string? violationMessage = null)
        {
            UsePasswordPolicy = usePasswordPolicy;
            MinLength = minLength.AsMaybe();
            NumNonAlphanumericChars = numNonAlphanumericChars.AsMaybe();
            Regex = regex.AsNullOrWhitespaceMaybe();
            ViolationMessage = violationMessage.AsNullOrWhitespaceMaybe();
        }

        public bool UsePasswordPolicy { get; init; }
        public Maybe<int> MinLength { get; init; } = Maybe.None;
        public Maybe<int> NumNonAlphanumericChars { get; init; } = Maybe.None;
        public Maybe<string> Regex { get; init; } = Maybe.None;
        public Maybe<string> ViolationMessage { get; init; } = Maybe.None;
    }
}
