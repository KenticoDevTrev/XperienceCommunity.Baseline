namespace System
{
    public static class ToContentAndContentCultureIdentityHelper
    {
        public static ContentCultureIdentity ToContentCultureIdentity(this int value) => new()
        {
            ContentCultureID = value
        };

        public static ContentCultureIdentity ToContentCultureIdentity(this Guid value) => new()
        {
            ContentCultureGuid = value
        };

        public static ContentCultureIdentity ToContentCultureIdentity(this string value, string? culture = null, int? channelId = null) => new()
        {
            PathAndMaybeCultureAndChannelId = new Tuple<string, Maybe<string>, Maybe<int>>(value, (culture ?? string.Empty).AsNullOrWhitespaceMaybe(), (channelId ?? 0) <= 0 ? Maybe<int>.None : Maybe<int>.From(channelId ?? 0))
        };

        public static ContentIdentity ToContentIdentity(this int value) => new()
        {
            ContentID = value
        };

        public static ContentIdentity ToContentIdentity(this Guid value) => new()
        {
            ContentGuid = value
        };

        public static ContentIdentity ToContentIdentity(this string value, int? channelId = null) => new()
        {
            PathAndChannelId = new Tuple<string, Maybe<int>>(value, (channelId ?? 0) <= 0 ? Maybe<int>.None : Maybe<int>.From(channelId ?? 0))
        };

        public static TreeIdentity ToTreeIdentity(this string value, int? channelId = null) => new()
        {
            PathAndChannelId = new Tuple<string, Maybe<int>>(value, (channelId ?? 0) <= 0 ? Maybe<int>.None : Maybe<int>.From(channelId ?? 0))
        };

        public static TreeIdentity ToTreeIdentity(this int pageId) => new()
        {
            PageID = pageId
        };

        public static TreeIdentity ToTreeIdentity(this Guid value) => new()
        {
            PageGuid = value
        };

        public static TreeCultureIdentity ToTreeCultureIdentity(this int pageId, string culture) => new(culture)
        {
            PageID = pageId
        };

        public static TreeCultureIdentity ToTreeCultureIdentity(this Guid value, string culture) => new(culture)
        {
            PageGuid = value
        };

        public static TreeCultureIdentity ToTreeCultureIdentity(this string value, string culture, int? channelId) => new(culture)
        {
            PathAndChannelId = new Tuple<string, Maybe<int>>(value, (channelId ?? 0) <= 0 ? Maybe<int>.None : Maybe<int>.From(channelId ?? 0))
        };

        public static TreeCultureIdentity ToTreeCultureIdentity(this TreeIdentity treeIdentity, string culture) => new(culture)
        {
            PageID = treeIdentity.PageID,
            PageGuid = treeIdentity.PageGuid,
            PathAndChannelId = treeIdentity.PathAndChannelId
        };
    }
}
