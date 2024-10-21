namespace Core.Models
{
    /// <summary>
    /// Used as a lookup for Tree Items specifically
    /// </summary>
    public record TreeIdentity : ICacheKey
    {
        public Maybe<int> PageID { get; init; }
        
        public Maybe<Guid> PageGuid { get; init; }

        public Maybe<string> PageName { get; init; }
        
        public Maybe<PathChannel> PathChannelLookup { get; init; }

        [Obsolete("Use PathChannelLookup")]
        public Maybe<Tuple<string, Maybe<int>>> PathAndChannelId
        {
            get
            {
                return PathChannelLookup.TryGetValue(out var pathChannelLookup) ? new Tuple<string, Maybe<int>>(pathChannelLookup.Path, pathChannelLookup.ChannelId) : Maybe.None;
            }
            init
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

            return $"{PageID.GetValueOrDefault(0)}{nodeAliasPathKey}{PageGuid.GetValueOrDefault(Guid.Empty)}";
        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }

    /// <summary>
    /// Used as a lookup for Tree Items specifically, with a culture
    /// </summary>
    public record TreeCultureIdentity : TreeIdentity, ICacheKey
    {
        public TreeCultureIdentity(string culture)
        {
            Culture = culture;
        }

        public string Culture { get; init; }

        public new string GetCacheKey()
        {
            var nodeAliasPathKey = "";
            if (PathChannelLookup.TryGetValue(out var pathChannelLookup))
            {
                nodeAliasPathKey = $"{pathChannelLookup.Path}{pathChannelLookup.ChannelId.GetValueOrDefault(0)}";
            }

            return $"{PageID.GetValueOrDefault(0)}{nodeAliasPathKey}{PageGuid.GetValueOrDefault(Guid.Empty)}{Culture}";
        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }

    public record PathChannel(string Path, Maybe<int> ChannelId) : ICacheKey
    {
        public string GetCacheKey()
        {
            return $"{Path}|{ChannelId.GetValueOrDefault(0)}";
        }
    }
}
