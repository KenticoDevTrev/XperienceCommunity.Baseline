using System.Text.Json.Serialization;

namespace Navigation.Models
{
    public record ItemListElementJsonLD
    {
        public ItemListElementJsonLD(int position, string name, string item)
        {
            Position = position;
            Name = name;
            Item = item;
        }

        [JsonPropertyName("@type")]
        public string ContentType { get; } = "ListItem";
        [JsonPropertyName("position")]
        public int Position { get; init; }
        [JsonPropertyName("name")]
        public string Name { get; init; }
        [JsonPropertyName("item")]
        public string Item { get; init; }
    }
}