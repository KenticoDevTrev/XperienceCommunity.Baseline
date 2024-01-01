namespace Localization.Models
{
    public record LocalizationConfiguration
    {
        public LocalizationConfiguration(string defaultCulture)
        {
            DefaultCulture = defaultCulture;
        }

        public string DefaultCulture { get; init; }
    }
}
