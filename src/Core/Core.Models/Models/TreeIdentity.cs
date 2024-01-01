namespace Core.Models
{
    /// <summary>
    /// Used as a lookup for Tree Items specifically
    /// </summary>
    public record TreeIdentity : ICacheKey
    {
        public Maybe<int> PageID { get; init; }
        public Maybe<Tuple<string, Maybe<int>>> PathAndChannelId { get; init; }
        public Maybe<Guid> PageGuid { get; init; }

        public string GetCacheKey()
        {
            var nodeAliasPathKey = "";
            if (PathAndChannelId.TryGetValue(out var valuePath))
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
            if (PathAndChannelId.TryGetValue(out var valuePath))
            {
                nodeAliasPathKey = $"{valuePath.Item1}{valuePath.Item2.GetValueOrDefault(0)}";
            }

            return $"{PageID.GetValueOrDefault(0)}{nodeAliasPathKey}{PageGuid.GetValueOrDefault(Guid.Empty)}{Culture}";
        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }
}
