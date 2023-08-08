using MVCCaching;

namespace Core.Models
{
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
            return $"{NodeId.GetValueOrDefault(0)}{nodeAliasPathKey}{NodeGuid.GetValueOrDefault(System.Guid.Empty)}";

        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }
}
