namespace Core.Models
{
    public record ContentIdentity : ICacheKey
    {
        public Maybe<int> ContentID { get; init; }
        public Maybe<Tuple<string, Maybe<int>>> PathAndChannelId { get; init; }
        public Maybe<Guid> ContentGuid { get; init; }

        public string GetCacheKey()
        {
            var nodeAliasPathKey = "";
            if (PathAndChannelId.TryGetValue(out var valuePath))
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

    public record ContentCultureIdentity : ICacheKey
    {
        public Maybe<int> ContentCultureID { get; init; }
        public Maybe<Tuple<int, Maybe<string>>> MaybeContentIDAndMaybeCulture { get; init; }
        public Maybe<Tuple<string, Maybe<string>, Maybe<int>>> PathAndMaybeCultureAndChannelId { get; init; }
        public Maybe<Guid> ContentCultureGuid { get; init; }

        public string GetCacheKey()
        {
            var nodeAliasPathKey = "";
            var contentKey = "";
            if (PathAndMaybeCultureAndChannelId.TryGetValue(out var valuePath))
            {
                nodeAliasPathKey = $"{valuePath.Item1}{valuePath.Item2.GetValueOrDefault(string.Empty)}{valuePath.Item3.GetValueOrDefault(0)}";
            }
            if (MaybeContentIDAndMaybeCulture.TryGetValue(out var valueContent))
            {
                contentKey = $"{valueContent.Item1}{valueContent.Item2.GetValueOrDefault(string.Empty)}";
            }
            return $"{ContentCultureID.GetValueOrDefault(0)}{nodeAliasPathKey}{contentKey}{ContentCultureGuid.GetValueOrDefault(System.Guid.Empty)}";
        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }
}
