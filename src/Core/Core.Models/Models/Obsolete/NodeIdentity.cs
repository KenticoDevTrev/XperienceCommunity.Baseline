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
            if (NodeAliasPathAndSiteId.TryGetValue(out var value))
            {
                nodeAliasPathKey = $"{value.Item1}{value.Item2.GetValueOrDefault(0)}";
            }
            return $"{NodeId.GetValueOrDefault(0)}{nodeAliasPathKey}{NodeGuid.GetValueOrDefault(Guid.Empty)}";
        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }

        /// <summary>
        /// Helper to convert to Tree Identity
        /// </summary>
        public TreeIdentity ToTreeIdentity()
        {
            return new TreeIdentity()
            {
                PageID = NodeId,
                PathAndChannelId = NodeAliasPathAndSiteId,
                PageGuid = NodeGuid
            };
        }

        /// <summary>
        /// Helper to convert to Content Identity
        /// </summary>
        public ContentIdentity ToContentIdentity()
        {
            return new ContentIdentity()
            {
                ContentID = NodeId,
                ContentName = (NodeAliasPathAndSiteId.TryGetValue(out var pathAndSite) ? pathAndSite.Item1.Split("/", StringSplitOptions.RemoveEmptyEntries).Last() ?? string.Empty : string.Empty).AsNullOrWhitespaceMaybe(),
                PathAndChannelId = NodeAliasPathAndSiteId,
                ContentGuid = NodeGuid
            };
        }
    }
}
