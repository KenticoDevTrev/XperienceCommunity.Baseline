using CMS.Membership;
using Kentico.Membership;
using Microsoft.AspNetCore.Http;

namespace Core.Repositories.Implementation
{
    public class UserRepository(IUserRepository<User> userRepository) : IUserRepository
    {
        private readonly IUserRepository<User> _userRepository = userRepository;

        public Task<User> GetCurrentUserAsync() => _userRepository.GetCurrentUserAsync();

        public Task<Result<User>> GetUserAsync(int userID) => _userRepository.GetUserAsync(userID);

        public Task<Result<User>> GetUserAsync(string userName) => _userRepository.GetUserAsync(userName);

        public Task<Result<User>> GetUserAsync(Guid userGuid) => _userRepository.GetUserAsync(userGuid);

        public Task<Result<User>> GetUserByEmailAsync(string email) => _userRepository.GetUserByEmailAsync(email);
    }

    public class UserRepository<TUser, TGenericUser>(
        IUserInfoProvider _userInfoProvider,
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory,
        IProgressiveCache _progressiveCache,
        IHttpContextAccessor _httpContextAccessor,
        IBaselineUserMapper<TUser, TGenericUser> _baselineUserMapper) : IUserRepository<TGenericUser> where TUser : ApplicationUser, new () where TGenericUser : User, new ()
    {
        public async Task<TGenericUser> GetCurrentUserAsync()
        {
            var username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "public";
            var user = await GetUserAsync(username);
            if (user.IsSuccess)
            {
                return user.Value;
            }
            return new TGenericUser() {
                UserName = "Public",
                FirstName = "Public",
                LastName = "User",
                Email = "public@public.com",
                Enabled = true,
                IsExternal = false,
                IsPublic = true
            };
                
        }

        public async Task<Result<TGenericUser>> GetUserAsync(int userID)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.Object(UserInfo.OBJECT_TYPE, userID);

            var user = await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var user = await _userInfoProvider.GetAsync(userID);
                return user;
            }, new CacheSettings(15, "GetUserAsync", userID));
            if (user != null)
            {
                return await _baselineUserMapper.ToUser(user);
            }
            return Result.Failure<TGenericUser>("Couldn't find User by ID");
        }

        public async Task<Result<TGenericUser>> GetUserAsync(string userName)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.Object(UserInfo.OBJECT_TYPE, userName);

            var user = await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var user = await _userInfoProvider.GetAsync(userName);
                return user;
            }, new CacheSettings(15, "GetUserAsync", userName));
            if (user != null)
            {
                return await _baselineUserMapper.ToUser(user);
            }
            return Result.Failure<TGenericUser>("Could not find user by username");
        }

        public async Task<Result<TGenericUser>> GetUserAsync(Guid userGuid)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.Object(UserInfo.OBJECT_TYPE, userGuid);

            var user = await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var user = await _userInfoProvider.GetAsync(userGuid);
                return user;
            }, new CacheSettings(15, "GetUserAsync", userGuid));
            if (user != null)
            {
                return await _baselineUserMapper.ToUser(user);
            }
            return Result.Failure<TGenericUser>("Could not find user by guid");

        }

        public async Task<Result<TGenericUser>> GetUserByEmailAsync(string email)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.ObjectType(UserInfo.OBJECT_TYPE);

            var user = await _progressiveCache.LoadAsync(async cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var user = (await _userInfoProvider.Get()
                .WhereEquals(nameof(UserInfo.Email), email)
                .TopN(1)
                .GetEnumerableTypedResultAsync()).FirstOrDefault();
                return user;
            }, new CacheSettings(15, "GetUserByEmailAsync", email));
            if (user != null)
            {
                return await _baselineUserMapper.ToUser(user);
            }
            return Result.Failure<TGenericUser>("Could not find user by email");
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
        [Obsolete("Use IBaselineUserMapper instead")]
        public static User ToUser(this UserInfo userInfo)
        {
            return new User(
                userID: userInfo.UserID,
                userName: userInfo.UserName,
                userGUID: userInfo.UserGUID,
                email: userInfo.Email,
                firstName: userInfo.FirstName,
                middleName: userInfo.MiddleName,
                lastName: userInfo.LastName,
                enabled: userInfo.Enabled,
                isExternal: userInfo.IsExternal,
                isPublic: userInfo.IsPublic()
                )
            {
                MiddleName = userInfo.MiddleName.AsNullOrWhitespaceMaybe()
            };
        }
    }
}
