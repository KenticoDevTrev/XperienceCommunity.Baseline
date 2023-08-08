using MVCCaching;

namespace Core.Models
{
    public record ObjectIdentity : ICacheKey
    {
        public Maybe<int> Id { get; init; }
        public Maybe<string> CodeName { get; init; }
        public Maybe<Guid> Guid { get; init; }

        public string GetCacheKey()
        {
            return $"{Id.GetValueOrDefault(0)}{CodeName.GetValueOrDefault(string.Empty)}{Guid.GetValueOrDefault(System.Guid.Empty)}";

        }

        public override int GetHashCode()
        {
            return GetCacheKey().GetHashCode();
        }
    }
}
