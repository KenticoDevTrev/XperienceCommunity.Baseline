﻿using MVCCaching;

namespace Core.Models
{
    public record DocumentIdentity : ICacheKey
    {
        public Maybe<int> DocumentId { get; init; }
        public Maybe<Tuple<string, Maybe<string>, Maybe<int>>> NodeAliasPathAndMaybeCultureAndSiteId { get; init; }
        public Maybe<Guid> DocumentGuid { get; init; }

        public string GetCacheKey()
        {
            var nodeAliasPathKey = "";
            if (NodeAliasPathAndMaybeCultureAndSiteId.TryGetValue(out var value))
            {
                nodeAliasPathKey = $"{value.Item1}{value.Item2.GetValueOrDefault(string.Empty)}{value.Item3.GetValueOrDefault(0)}";
            }
            return $"{DocumentId.GetValueOrDefault(0)}{nodeAliasPathKey}{DocumentGuid.GetValueOrDefault(System.Guid.Empty)}";

        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }
}
