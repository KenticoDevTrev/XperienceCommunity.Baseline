namespace Core.Models
{
    public record ContentIdentity : ICacheKey
    {
        public Maybe<int> ContentID { get; init; }

        public Maybe<PathChannel> PathChannelLookup { get; init; }

        public Maybe<Guid> ContentGuid { get; init; }

        [Obsolete("Use PathChannelLookup instead")]
        public Maybe<Tuple<string, Maybe<int>>> PathAndChannelId
        {
            get
            {
                return PathChannelLookup.TryGetValue(out var pathChannelLookup) ? new Tuple<string, Maybe<int>>(pathChannelLookup.Path, pathChannelLookup.ChannelId) : Maybe.None;
            } init
            {
                PathChannelLookup = value.TryGetValue(out var tuple) ? new PathChannel(tuple.Item1, tuple.Item2) : Maybe.None;
            }
        }

        public string GetCacheKey()
        {
            var nodeAliasPathKey = "";
            if (PathChannelLookup.TryGetValue(out var pathChannelLookup))
            {
                nodeAliasPathKey = $"{pathChannelLookup.Path}{pathChannelLookup.ChannelId.GetValueOrDefault(0)}";
            }

            return $"{ContentID.GetValueOrDefault(0)}{nodeAliasPathKey}{ContentGuid.GetValueOrDefault(Guid.Empty)}";
        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }

    public record ContentCultureIdentity : ICacheKey
    {
        public Maybe<int> ContentCultureID { get; init; }
        public Maybe<Guid> ContentCultureGuid { get; init; }
        public Maybe<ContentCulture> ContentCultureLookup { get; init; }
        public Maybe<PathCultureChannel> PathCultureChannelLookup { get; init; }


        [Obsolete("Use ContentCultureLookup instead")]
        public Maybe<Tuple<int, Maybe<string>>> MaybeContentIDAndMaybeCulture
        {
            get
            {
                return ContentCultureLookup.TryGetValue(out var contentCultureLookup) ? new Tuple<int, Maybe<string>>(contentCultureLookup.ContentId, contentCultureLookup.Culture) : Maybe.None;
            }
            init
            {
                ContentCultureLookup = value.TryGetValue(out var tuple) ? new ContentCulture(tuple.Item1, tuple.Item2) : Maybe.None;
            }
        }

        [Obsolete("Use PathCultureChannelLookup instead")]
        public Maybe<Tuple<string, Maybe<string>, Maybe<int>>> PathAndMaybeCultureAndChannelId
        {
            get
            {
                return PathCultureChannelLookup.TryGetValue(out var pathCultureChannelLookup) ? new Tuple<string, Maybe<string>, Maybe<int>>(pathCultureChannelLookup.Path, pathCultureChannelLookup.Culture, pathCultureChannelLookup.ChannelId) : Maybe.None;
            } init
            {
                PathCultureChannelLookup = value.TryGetValue(out var tuple) ? new PathCultureChannel(tuple.Item1, tuple.Item2, tuple.Item3) : Maybe.None;
            }
        }


        public string GetCacheKey()
        {
            var nodeAliasPathKey = "";
            var contentKey = "";
            if (PathCultureChannelLookup.TryGetValue(out var pathCultureChannelLookup))
            {
                nodeAliasPathKey = $"{pathCultureChannelLookup.Path}{pathCultureChannelLookup.Culture.GetValueOrDefault(string.Empty)}{pathCultureChannelLookup.ChannelId.GetValueOrDefault(0)}";
            }
            if (ContentCultureLookup.TryGetValue(out var valueContent))
            {
                contentKey = $"{valueContent.ContentId}{valueContent.Culture.GetValueOrDefault(string.Empty)}";
            }
            return $"{ContentCultureID.GetValueOrDefault(0)}{nodeAliasPathKey}{contentKey}{ContentCultureGuid.GetValueOrDefault(System.Guid.Empty)}";
        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }

    public record PathCultureChannel(string Path, Maybe<string> Culture, Maybe<int> ChannelId);

    public record ContentCulture(int ContentId, Maybe<string> Culture);
}
