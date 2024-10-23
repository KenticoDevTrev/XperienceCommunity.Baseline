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

    public static class TreeIdentityExtensionMethods
    {
        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<int>> GetOrRetrievePageID(this TreeIdentity identity, IIdentityService identityService)
        {
            if(identity.PageID.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateTreeIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.PageID.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<int>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<Guid>> GetOrRetrievePageGuid(this TreeIdentity identity, IIdentityService identityService)
        {
            if (identity.PageGuid.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateTreeIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.PageGuid.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<Guid>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<string>> GetOrRetrievePageName(this TreeIdentity identity, IIdentityService identityService)
        {
            if (identity.PageName.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateTreeIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.PageName.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<string>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<PathChannel>> GetOrRetrievePathChannelLookup(this TreeIdentity identity, IIdentityService identityService)
        {
            if (identity.PathChannelLookup.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateTreeIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.PathChannelLookup.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<PathChannel>(error);
        }
    }

    public static class TreeCultureIdentityExtensionMethods
    {
        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<int>> GetOrRetrievePageID(this TreeCultureIdentity identity, IIdentityService identityService)
        {
            if (identity.PageID.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateTreeCultureIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.PageID.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<int>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<Guid>> GetOrRetrievePageGuid(this TreeCultureIdentity identity, IIdentityService identityService)
        {
            if (identity.PageGuid.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateTreeCultureIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.PageGuid.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<Guid>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<string>> GetOrRetrievePageName(this TreeCultureIdentity identity, IIdentityService identityService)
        {
            if (identity.PageName.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateTreeCultureIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.PageName.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<string>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<PathChannel>> GetOrRetrievePathChannelLookup(this TreeCultureIdentity identity, IIdentityService identityService)
        {
            if (identity.PathChannelLookup.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateTreeCultureIdentity(identity)).TryGetValue(out var hydrated, out var error) && hydrated.PathChannelLookup.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<PathChannel>(error);
        }
    }
}
