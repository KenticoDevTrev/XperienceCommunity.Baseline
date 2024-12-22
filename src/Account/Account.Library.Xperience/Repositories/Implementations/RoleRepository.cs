using Account.Models;
using AngleSharp.Common;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites.Routing;
using Core.Enums;
using CSharpFunctionalExtensions;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MVCCaching;
using System.ComponentModel.Design;
using XperienceCommunity.MemberRoles.Models;

namespace Account.Repositories.Implementations
{
    public class RoleRepository<TUser, TRole>(IRoleStore<TRole> roleStore,
                                IWebsiteChannelContext websiteChannelContext,
                                IInfoProvider<TagInfo> tagInfoProvider,
                                IProgressiveCache progressiveCache,
                                IUserRoleStore<TUser> userRoleStore,
                                IUserStore<TUser> userStore) : IRoleRepository where TUser : ApplicationUser, new() where TRole : TagApplicationUserRole, new()
    {
        private readonly IRoleStore<TRole> _roleStore = roleStore;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IUserRoleStore<TUser> _userRoleStore = userRoleStore;
        private readonly IUserStore<TUser> _userStore = userStore;

        public async Task<Result<RoleItem>> GetRoleAsync(string roleName, string siteName)
        {
            var role = await _roleStore.FindByNameAsync(roleName.Normalize(), CancellationToken.None);
            if (role == null) {
                return Result.Failure<RoleItem>($"Role {roleName} not found.");
            }

            // get guid and other info from it
            if ((await TagIdToInfo()).TryGetValue(role.Id, out var tagInfo)) {
                return new RoleItem(_websiteChannelContext.WebsiteChannelID.ToObjectIdentity(), role.Id, tagInfo.TagTitle, tagInfo.TagName.Normalize(), tagInfo.TagGUID);
            } else {
                return new RoleItem(_websiteChannelContext.WebsiteChannelID.ToObjectIdentity(), role.Id, role.Name ?? roleName, role.NormalizedName ?? roleName, Guid.Empty);
            }
        }

        public async Task<Dictionary<int, TagInfo>> TagIdToInfo()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{TagInfo.OBJECT_TYPE}|all");
                }
                return (await _tagInfoProvider.Get()
                .GetEnumerableTypedResultAsync())
                .ToDictionary(key => key.TagID, value => value);
            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "GetTagIdToInfo"));
        }

        /// <summary>
        /// In Xperience by Kentico, there are no longer resource names and permissions
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="resourceName"></param>
        /// <param name="permissionName"></param>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public Task<bool> UserHasPermissionAsync(int userID, string resourceName, string permissionName, string siteName)
        {
            throw new NotSupportedException("Xperience by Kentico's Member Roles no longer are tied to Module Permissions.  Use a custom IAuthorize logic if needed or add additional member roles to accomodate these checks");
        }

        public async Task<bool> UserInRoleAsync(int userID, string roleName, string siteName)
        {
            var user = await _userStore.FindByIdAsync(userID.ToString(), CancellationToken.None);
            if(user == null) {
                return false;
            }
            return await _userRoleStore.IsInRoleAsync(user, roleName, CancellationToken.None);
        }
    }
}
