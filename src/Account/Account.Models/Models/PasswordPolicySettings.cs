namespace Account.Models
{
    public record PasswordPolicySettings
    {
        public PasswordPolicySettings(bool usePasswordPolicy, int minLength, int numNonAlphanumericChars, string regex, string violationMessage)
        {
            UsePasswordPolicy = usePasswordPolicy;
            MinLength = minLength;
            NumNonAlphanumericChars = numNonAlphanumericChars;
            Regex = regex;
            ViolationMessage = violationMessage;
        }

        public bool UsePasswordPolicy { get; init; }
        public int MinLength { get; init; }
        public int NumNonAlphanumericChars { get; init; }
        public string Regex { get; init; }
        public string ViolationMessage { get; init; }
    }
}
