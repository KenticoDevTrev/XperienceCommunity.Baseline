namespace Core.Models
{
    public record PageMetaData
    {
        public Maybe<string> Title { get; init; }
        public Maybe<string> Keywords { get; init; }
        public Maybe<string> Description { get; init; }
        public Maybe<string> Thumbnail { get; init; }
        public Maybe<string> ThumbnailLarge { get; init; }
        public Maybe<string> CanonicalUrl { get; init; }
        public Maybe<bool> NoIndex { get; init; }
    }
}
