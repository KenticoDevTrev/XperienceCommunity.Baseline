namespace Core.Models
{
    [Obsolete("Use ContentIdentity or TreeIdentity")]
    public record NodeIdentity : ICacheKey
    {
        public Maybe<int> NodeId { get; init; }
        public Maybe<Tuple<string, Maybe<int>>> NodeAliasPathAndSiteId { get; init; }
        public Maybe<Guid> NodeGuid { get; init; }

        public string GetCacheKey()
        {
            var nodeAliasPathKey = "";
            if(NodeAliasPathAndSiteId.TryGetValue(out var value))
            {
                nodeAliasPathKey = $"{value.Item1}{value.Item2.GetValueOrDefault(0)}";
            }
            return $"{NodeId.GetValueOrDefault(0)}{nodeAliasPathKey}{NodeGuid.GetValueOrDefault(Guid.Empty)}";
        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }

        public TreeIdentity ToTreeIdentity()
        {
            return new TreeIdentity()
            {
                PageID = NodeId,
                PathAndAndChannelId = NodeAliasPathAndSiteId,
                PageGuid = NodeGuid
            };
        }

        public ContentIdentity ToContentIdentity()
        {
            return new ContentIdentity()
            {
                ContentID = NodeId,
                PathAndAndChannelId = NodeAliasPathAndSiteId,
                ContentGuid = NodeGuid
            };
        }
    }

    /// <summary>
    /// Used as a lookup for Tree Items specifically
    /// </summary>
    public record TreeIdentity : ICacheKey
    {
        public Maybe<int> PageID { get; init; }
        public Maybe<Tuple<string, Maybe<int>>> PathAndAndChannelId { get; init; }
        public Maybe<Guid> PageGuid { get; init; }

        public string GetCacheKey()
        {
            var nodeAliasPathKey = "";
            if (PathAndAndChannelId.TryGetValue(out var valuePath))
            {
                nodeAliasPathKey = $"{valuePath.Item1}{valuePath.Item2.GetValueOrDefault(0)}";
            }

            return $"{PageID.GetValueOrDefault(0)}{nodeAliasPathKey}{PageGuid.GetValueOrDefault(Guid.Empty)}";
        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }

    public record ContentIdentity : ICacheKey
    {
        public Maybe<int> ContentID { get; init; }
        public Maybe<Tuple<string, Maybe<int>>> PathAndAndChannelId { get; init; }
        public Maybe<Guid> ContentGuid { get; init; }

        public string GetCacheKey()
        {
            var nodeAliasPathKey = "";
            if (PathAndAndChannelId.TryGetValue(out var valuePath))
            {
                nodeAliasPathKey = $"{valuePath.Item1}{valuePath.Item2.GetValueOrDefault(0)}";
            }
           
            return $"{ContentID.GetValueOrDefault(0)}{nodeAliasPathKey}{ContentGuid.GetValueOrDefault(Guid.Empty)}";
        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }
}
