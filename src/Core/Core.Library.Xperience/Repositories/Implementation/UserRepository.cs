using CMS.Membership;
using Kentico.Membership;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Core.Repositories.Implementation
{
    public class UserRepository<TUser>(IHttpContextAccessor httpContextAccessor,
        ICacheDependencyScopedBuilderFactory cacheDependencyBuilderFactory,
        IProgressiveCache progressiveCache,
        IInfoProvider<MemberInfo> memberInfoProvider,
        ICacheDependenciesScope cacheDependenciesScope,
        IBaselineUserMapper<TUser> baselineUserMapper,
        UserManager<TUser> userManager) : IUserRepository where TUser : ApplicationUser, new()
    {
        private const string _userName = "Public";
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ICacheDependencyScopedBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IInfoProvider<MemberInfo> _memberInfoProvider = memberInfoProvider;
        private readonly ICacheDependenciesScope _cacheDependenciesScope = cacheDependenciesScope;
        private readonly IBaselineUserMapper<TUser> _baselineUserMapper = baselineUserMapper;
        private readonly UserManager<TUser> _userManager = userManager;

        public async Task<User> GetCurrentUserAsync()
        {
            var username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "public";
            var user = await GetUserInternal(
                username: username,
                email: Maybe.None,
                guid: Maybe.None,
                id: Maybe.None
                );
            return user.TryGetValue(out var foundUser) ? foundUser : new User() {
                UserName = _userName,
                FirstName = "Public",
                LastName = "User",
                Email = "public@localhost",
                Enabled = true,
                IsExternal = false,
                IsPublic = true
            };
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

                if (user.TryGetValue(out var userMember)) {
                    internalBuilder.Object(MemberInfo.OBJECT_TYPE, userMember.MemberID);

                    // In case in converting MemberInfo to the object type, there are other dependencies added to scope
                    _cacheDependenciesScope.Begin();
                    var applicationUser = new TUser();
                    applicationUser.MapFromMemberInfo(userMember);
                    //var applicationUser = await _userManager.FindByNameAsync(userMember.MemberName);
                    internalBuilder.AddKeys(_cacheDependenciesScope.End());

                    if (cs.Cached) {
                        cs.CacheDependency = internalBuilder.GetCMSCacheDependency();
                    }

                    if (applicationUser != null) { 
                        return new DTOWithDependencies<Result<TUser>>(Result.Success(applicationUser), internalBuilder.GetKeys().ToList());
                    }
                }
                // Not found
                internalBuilder.ObjectType(MemberInfo.OBJECT_TYPE);
                if (cs.Cached) {
                    cs.CacheDependency = internalBuilder.GetCMSCacheDependency();
                }
                return new DTOWithDependencies<Result<TUser>>(Result.Failure<TUser>("Could not find member by identifiers"), internalBuilder.GetKeys().ToList());
            }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetUserInternal", username.GetValueOrDefault(string.Empty), email.GetValueOrDefault(string.Empty), guid.GetValueOrDefault(Guid.Empty), id.GetValueOrDefault(0)));

            // Add Keys to global scope now
            var builder = _cacheDependencyBuilderFactory.Create()
                .AddKeys(result.AdditionalDependencies);

            if (result.Result.TryGetValue(out var appUser, out var error)) {
                _cacheDependenciesScope.Begin();
                var user = await _baselineUserMapper.ToUser(appUser);
                builder.AddKeys(_cacheDependenciesScope.End());
                return user;
            }
            return Result.Failure<User>(error);
        }
    }
}
