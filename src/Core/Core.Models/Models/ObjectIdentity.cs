namespace Core.Models
{
    public record ObjectIdentity : ICacheKey
    {
        public Maybe<int> Id { get; init; }
        public Maybe<Guid> Guid { get; init; }
        public Maybe<string> CodeName { get; init; }

        public string GetCacheKey()
        {
            return $"{Id.GetValueOrDefault(0)}{CodeName.GetValueOrDefault(string.Empty)}{Guid.GetValueOrDefault(System.Guid.Empty)}";

        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }

    public record ObjectIdentityFilled : ICacheKey
    {
        public ObjectIdentityFilled(int id, Guid guid, string codeName)
        {
            Id = id;
            Guid = guid;
            CodeName = codeName;
        }
        public int Id { get; init; }
        public Guid Guid { get; init; }
        public string CodeName { get; init; }

        public string GetCacheKey()
        {
            return $"{Id.GetValueOrDefault(0)}{CodeName.GetValueOrDefault(string.Empty)}{Guid.GetValueOrDefault(System.Guid.Empty)}";

        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }

    public static class ObjectIdentityExtensionMethods
    {
        public static ObjectIdentity ToObjectIdentity(this ObjectIdentityFilled identity) => new() {
                Id = identity.Id,
                Guid = identity.Guid,
                CodeName = identity.CodeName
            };
        

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="className">The Content Type Code Name</param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<int>> GetOrRetrieveId(this ObjectIdentity identity, string className, IIdentityService identityService)
        {
            if (identity.Id.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateObjectIdentity(identity, className)).TryGetValue(out var hydrated, out var error) && hydrated.Id.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<int>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="className">The Content Type Code Name</param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<Guid>> GetOrRetrieveGuid(this ObjectIdentity identity, string className, IIdentityService identityService)
        {
            if (identity.Guid.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateObjectIdentity(identity, className)).TryGetValue(out var hydrated, out var error) && hydrated.Guid.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<Guid>(error);
        }

        /// <summary>
        /// Returns the requested identity value (if not present, will retrieve it)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="className">The Content Type Code Name</param>
        /// <param name="identityService"></param>
        /// <returns>The requested identity value, or Failure if it was not there and could not be retrieved by the identity service</returns>
        public static async Task<Result<string>> GetOrRetrieveCodeName(this ObjectIdentity identity, string className, IIdentityService identityService)
        {
            if (identity.CodeName.TryGetValue(out var value)) {
                return value;
            }
            return (await identityService.HydrateObjectIdentity(identity, className)).TryGetValue(out var hydrated, out var error) && hydrated.CodeName.TryGetValue(out var hydratedValue) ? hydratedValue : Result.Failure<string>(error);
        }
    }
}
