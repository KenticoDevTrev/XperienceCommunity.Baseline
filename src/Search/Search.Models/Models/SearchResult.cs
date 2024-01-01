using System.Text.RegularExpressions;

namespace Search.Models
{
    public record SearchResponse
    {
        public IEnumerable<SearchItem> Items { get; init; } = [];
        public int TotalPossible { get; init; } = 0;
        public IEnumerable<string> HighlightedWords { get; init; } = [];
        public Maybe<Regex> HighlightRegex { get; init; }
    }
}
