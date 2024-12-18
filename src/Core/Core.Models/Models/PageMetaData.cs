namespace Core.Models
{
    public record PageMetaData
    {
        public Maybe<string> Title { get; init; }
        public Maybe<string> Keywords { get; init; }
        public Maybe<string> Description { get; init; }
        public Maybe<string> Thumbnail { get; init; }
        [Obsolete("No longer supporting in Xperience by Kentico, feel free to use a custom Reusable Schema and the 'Pages and Reusable Content' type, along with specifying the Content Type for images you use on your site.")]
        public Maybe<string> ThumbnailLarge { get; init; }
        public Maybe<string> CanonicalUrl { get; init; }
        public Maybe<bool> NoIndex { get; init; }
    }
}
