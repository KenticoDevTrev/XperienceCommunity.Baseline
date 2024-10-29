using CMS.Membership;
using Microsoft.AspNetCore.Http;

namespace Core.Repositories.Implementation
{
    public class UserRepository(IHttpContextAccessor httpContextAccessor,
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory,
        IProgressiveCache _progressiveCache,
        IInfoProvider<MemberInfo> memberInfoProvider,
        IUserMetadataProvider userMetadataProvider,
        ICacheDependenciesScope cacheDependenciesScope) : IUserRepository
    {
        private const string _userName = "Public";
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IInfoProvider<MemberInfo> _memberInfoProvider = memberInfoProvider;
        private readonly IUserMetadataProvider _userMetadataProvider = userMetadataProvider;
        private readonly ICacheDependenciesScope _cacheDependenciesScope = cacheDependenciesScope;

        public async Task<User> GetCurrentUserAsync()
        {
            var username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "public";
            var user = await GetUserInternal(
                username: username,
                email: Maybe.None,
                guid: Maybe.None,
                id: Maybe.None
                );
            return user.TryGetValue(out var foundUser) ? foundUser : new User(
                userName: _userName,
                firstName: "Public",
                lastName: "User",
                email: "public@public.com",
                enabled: true,
                isExternal: false,
                isPublic: true);
        }

        public Task<Result<User>> GetUserAsync(int userID) => GetUserInternal(
                username: Maybe.None,
                email: Maybe.None,
                guid: Maybe.None,
                id: userID);

        public Task<Result<User>> GetUserAsync(string userName) => GetUserInternal(
                username: userName.AsNullOrWhitespaceMaybe(),
                email: Maybe.None,
                guid: Maybe.None,
                id: Maybe.None);

        public Task<Result<User>> GetUserAsync(Guid userGuid) => GetUserInternal(
                username: Maybe.None,
                email: Maybe.None,
                guid: userGuid,
                id: Maybe.None);

        public Task<Result<User>> GetUserByEmailAsync(string email) => GetUserInternal(
                username: Maybe.None,
                email: email.AsNullOrWhitespaceMaybe(),
                guid: Maybe.None,
                id: Maybe.None);

        private async Task<Result<User>> GetUserInternal(Maybe<string> username, Maybe<string> email, Maybe<Guid> guid, Maybe<int> id)
        {
            var result = await _progressiveCache.LoadAsync(async cs => {
                var userquery = _memberInfoProvider.Get()
                .TopN(1);

                var internalBuilder = _cacheDependencyBuilderFactory.Create(false);

                if (id.TryGetValue(out var userIdVal)) {
                    userquery.WhereEquals(nameof(MemberInfo.MemberID), userIdVal);
                } else if (guid.TryGetValue(out var userGuidVal)) {
                    userquery.WhereEquals(nameof(MemberInfo.MemberGuid), userGuidVal);
                } else if (username.TryGetValue(out var usernameVal)) {
                    userquery.WhereEquals(nameof(MemberInfo.MemberName), usernameVal);
                } else if (email.TryGetValue(out var emailVal)) {
                    userquery.WhereEquals(nameof(MemberInfo.MemberEmail), emailVal);
                } else {
                    userquery.WhereEquals(nameof(MemberInfo.MemberID), -1);
                }

                var user = (await userquery.GetEnumerableTypedResultAsync()).FirstOrMaybe();

                if (user.TryGetValue(out var userInfo)) {
                    internalBuilder.Object(MemberInfo.OBJECT_TYPE, userInfo.MemberID);
                    if (cs.Cached) {
                        cs.CacheDependency = internalBuilder.GetCMSCacheDependency();
                    }
                    return new DTOWithDependencies<Result<MemberInfo>>(Result.Success(userInfo), internalBuilder.GetKeys().ToList());
                }
                // Not found
                internalBuilder.ObjectType(MemberInfo.OBJECT_TYPE);
                if (cs.Cached) {
                    cs.CacheDependency = internalBuilder.GetCMSCacheDependency();
                }
                return new DTOWithDependencies<Result<MemberInfo>>(Result.Failure<MemberInfo>("Could not find member by identifiers"), internalBuilder.GetKeys().ToList());
            }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetUserInternal", email));

            // Add Keys to global scope now
            var builder = _cacheDependencyBuilderFactory.Create()
                .AddKeys(result.AdditionalDependencies);

            if (result.Result.TryGetValue(out var memberInfo, out var error)) {
                var user = memberInfo.ToUser();
                _cacheDependenciesScope.Begin();

                if ((await _userMetadataProvider.GetUserMetadata(memberInfo, user)).TryGetValue(out var metaData)) {
                    user = user with { MetaData = metaData.AsMaybe() };
                };
                builder.AddKeys(_cacheDependenciesScope.End());

                return user;
            }
            return Result.Failure<User>(error);
        }
    }
}

namespace CMS.Membership
{
    public static class UserInfoExtensions
    {
        /// <summary>
        /// Convert UserInfo to User, used in multiple files so made extension
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public static User ToUser(this MemberInfo userInfo)
        {
            return new User(
                userID: userInfo.MemberID,
                userName: userInfo.MemberName,
                userGUID: userInfo.MemberGuid,
                email: userInfo.MemberEmail,
                enabled: userInfo.MemberEnabled,
                isExternal: userInfo.MemberIsExternal,
                isPublic: userInfo.MemberName.Equals("public", StringComparison.OrdinalIgnoreCase)
                ) {
            };
        }
    }
}
