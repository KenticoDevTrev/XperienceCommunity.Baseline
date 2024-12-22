namespace Search.Models
{
    public class BaselineSearchOptions(IEnumerable<string>? DefaultSearchIndexes = null)
    {
        public IEnumerable<string> DefaultSearchIndexes { get; set; } = DefaultSearchIndexes ?? [];
    }
}
