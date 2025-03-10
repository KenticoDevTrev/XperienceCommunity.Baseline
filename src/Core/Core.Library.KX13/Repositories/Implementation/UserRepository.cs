﻿using CMS.Membership;
using Kentico.Membership;
using Microsoft.AspNetCore.Http;

namespace Core.Repositories.Implementation
{
    public class UserRepository<TUser>(
        IUserInfoProvider _userInfoProvider,
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory,
        IProgressiveCache _progressiveCache,
        IHttpContextAccessor _httpContextAccessor,
        IBaselineUserMapper<TUser> _baselineUserMapper) : IUserRepository where TUser : ApplicationUser, new ()
    {
        public async Task<User> GetCurrentUserAsync()
        {
            var username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "public";
            var user = await GetUserAsync(username);
            if (user.IsSuccess)
            {
                return user.Value;
            }
            return new User() {
                UserName = "Public",
                FirstName = "Public",
                LastName = "User",
                Email = "public@public.com",
                Enabled = true,
                IsExternal = false,
                IsPublic = true
            };
        }

        public async Task<Result<User>> GetUserAsync(int userID)
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
            return Result.Failure<User>("Couldn't find User by ID");
        }

        public async Task<Result<User>> GetUserAsync(string userName)
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
            return Result.Failure<User>("Could not find user by username");
        }

        public async Task<Result<User>> GetUserAsync(Guid userGuid)
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
            return Result.Failure<User>("Could not find user by guid");

        }

        public async Task<Result<User>> GetUserByEmailAsync(string email)
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
            return Result.Failure<User>("Could not find user by email");
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
