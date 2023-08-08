using System.Text.RegularExpressions;

namespace Search.Models
{
    public record SearchResponse
    {
        public IEnumerable<SearchItem> Items { get; init; } = Array.Empty<SearchItem>();
        public int TotalPossible { get; init; } = 0;
        public IEnumerable<string> HighlightedWords { get; init; } = Array.Empty<string>();
        public Maybe<Regex> HighlightRegex { get; init; }
    }
}
