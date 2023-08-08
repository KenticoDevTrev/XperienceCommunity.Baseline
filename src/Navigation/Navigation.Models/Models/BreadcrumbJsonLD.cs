using System.Text.Json.Serialization;

namespace Navigation.Models
{
    public record BreadcrumbJsonLD
    {
        public BreadcrumbJsonLD(List<ItemListElementJsonLD> itemListElement)
        {
            ItemListElement = itemListElement;
        }

        [JsonPropertyName("@context")]
        public string Context { get; } = "https://schema.org";
        [JsonPropertyName("@type")]
        public string ContentType { get; } = "BreadcrumbList";
        [JsonPropertyName("itemListElement")]
        public IEnumerable<ItemListElementJsonLD> ItemListElement { get; set; }
    }
}