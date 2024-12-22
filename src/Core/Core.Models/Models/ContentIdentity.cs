namespace Core.Models
{
    public record ContentIdentity : ICacheKey
    {
        public Maybe<int> ContentID { get; init; }

        public Maybe<Guid> ContentGuid { get; init; }

        public Maybe<string> ContentName { get; init; }

        [Obsolete("Path Lookup will not be populated in Xperience by Kentico (Use TreeIdentity), however remains temporarily for KX13 for tree support.  When migrating, try to move content items into a Content Items typed field on the parent.", false)]
        public Maybe<PathChannel> PathChannelLookup { get; init; }

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
#pragma warning disable CS0618 // Type or member is obsolete
            if (PathChannelLookup.TryGetValue(out var pathChannelLookup))
            {
                nodeAliasPathKey = $"{pathChannelLookup.Path}{pathChannelLookup.ChannelId.GetValueOrDefault(0)}";
            }
#pragma warning restore CS0618 // Type or member is obsolete

            return $"{ContentID.GetValueOrDefault(0)}|{ContentGuid.GetValueOrDefault(Guid.Empty)}|{ContentName.GetValueOrDefault(string.Empty)}|{nodeAliasPathKey}";
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

        [Obsolete("Path Lookup will not be populated in Xperience by Kentico (Use TreeIdentity), however remains temporarily for KX13 for tree support.  When migrating, try to move content items into a Content Items typed field on the parent.")]
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
#pragma warning disable CS0618 // Type or member is obsolete
            if (PathCultureChannelLookup.TryGetValue(out var pathCultureChannelLookup))
            {
                nodeAliasPathKey = $"{pathCultureChannelLookup.Path}{pathCultureChannelLookup.Culture.GetValueOrDefault(string.Empty)}{pathCultureChannelLookup.ChannelId.GetValueOrDefault(0)}";
            }
#pragma warning restore CS0618 // Type or member is obsolete
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

    public record ContentCulture(int ContentId, Maybe<string> Culture) : ICacheKey
    {
        public string GetCacheKey()
        {
            return $"{ContentId}_{Culture.GetValueOrDefault(string.Empty)}";
        }
    }

    public static class ContentIdentityExtensionMethods
    {
        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<int>> GetOrRetrieveContentID(this ContentIdentity identity, IIdentityService identityService)
        {
            if (identity.ContentID.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateContentIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.ContentID.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<int>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<Guid>> GetOrRetrieveContentGuid(this ContentIdentity identity, IIdentityService identityService)
        {
            if (identity.ContentGuid.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateContentIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.ContentGuid.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<Guid>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<string>> GetOrRetrieveContentName(this ContentIdentity identity, IIdentityService identityService)
        {
            if (identity.ContentName.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateContentIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.ContentName.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<string>(error);
        }
    }

    public static class ContentCultureIdentityExtensionMethods
    {
        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<int>> GetOrRetrieveContentCultureID(this ContentCultureIdentity identity, IIdentityService identityService)
        {
            if (identity.ContentCultureID.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateContentCultureIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.ContentCultureID.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<int>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<Guid>> GetOrRetrieveContentCultureGuid(this ContentCultureIdentity identity, IIdentityService identityService)
        {
            if (identity.ContentCultureGuid.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateContentCultureIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.ContentCultureGuid.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<Guid>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<ContentCulture>> GetOrRetrieveContentCultureLookup(this ContentCultureIdentity identity, IIdentityService identityService)
        {
            if (identity.ContentCultureLookup.TryGetValue(out var value) && value.Culture.HasValue) {
                return value;
            }
            return (await identityService.HydrateContentCultureIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.ContentCultureLookup.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<ContentCulture>(error);
        }
    }
}
