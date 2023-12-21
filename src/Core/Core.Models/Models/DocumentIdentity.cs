namespace Core.Models
{
    [Obsolete("Use ContentCultureIdentity")]
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

        public ContentCultureIdentity ToContentCultureIdentity()
        {
            return new ContentCultureIdentity()
            {
                ContentCultureID = DocumentId,
                PathAndMaybeCultureAndChannelId = NodeAliasPathAndMaybeCultureAndSiteId,
                ContentCultureGuid = DocumentGuid
            };
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
