﻿namespace Core.Models
{
    public record PageMetaData
    {
        public Maybe<string> Title { get; set; }
        public Maybe<string> Keywords { get; set; }
        public Maybe<string> Description { get; set; }
        public Maybe<string> Thumbnail { get; set; }
        public Maybe<string> ThumbnailLarge { get; set; }
        public Maybe<string> CanonicalUrl { get; set; }
        public Maybe<bool> NoIndex { get; set; }
    }
}
