using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using Core.Models;
using Core.Repositories;
using XperienceCommunity.MemberRoles;

namespace Account.Repositories.Implementations
{
    /// <summary>
    /// Perhaps overly optimized, for sites with massive amounts of members and role retrieval, this should give the optimal performance.
    /// </summary>
    /// <param name="progressiveCache"></param>
    /// <param name="memberRoleTagInfoProvider"></param>
    /// <param name="categoryCachedRepository"></param>
    public class UserRoleRepository(IProgressiveCache progressiveCache, IInfoProvider<MemberRoleTagInfo> memberRoleTagInfoProvider, ICategoryCachedRepository categoryCachedRepository): IUserRoleRepository
    {
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IInfoProvider<MemberRoleTagInfo> _memberRoleTagInfoProvider = memberRoleTagInfoProvider;
        private readonly ICategoryCachedRepository _categoryCachedRepository = categoryCachedRepository;

        public Task<IEnumerable<ObjectIdentityFilled>> GetUserRoles<TUser>(TUser user) where TUser : User => GetUserRoles(user.ToObjectIdentity());
        public async Task<IEnumerable<ObjectIdentityFilled>> GetUserRoles(ObjectIdentity user)
        {
            // Kentico's Member Store sets the user.Id to the MemberId if present.
            var roleTagIds = await _progressiveCache.LoadAsync(async (cs) => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{MemberRoleTagInfo.OBJECT_TYPE}|all");
                }

                var query = _memberRoleTagInfoProvider.Get()
                .Source(x => x.InnerJoin<MemberInfo>(nameof(MemberRoleTagInfo.MemberRoleTagMemberID), nameof(MemberInfo.MemberID)))
                .Columns(nameof(MemberRoleTagInfo.MemberRoleTagTagID));
                if (user.Id.TryGetValue(out var userId)) {
                    query.WhereEquals(nameof(MemberInfo.MemberID), userId);
                } else if (user.Guid.TryGetValue(out var userGuid)) {
                    query.WhereEquals(nameof(MemberInfo.MemberGuid), userGuid);
                } else {
                    query.WhereEquals(nameof(MemberInfo.MemberName), user.CodeName);
                }

                return (await query
                .GetEnumerableTypedResultAsync()
                ).Select(x => x.MemberRoleTagTagID).ToList();
            }, new CacheSettings(20, "GetUserRoles_ByMemberTagIDs_User", user.GetCacheKey()));
            return _categoryCachedRepository.GetCategoryIdentifiertoCategoryCached(roleTagIds.Select(x => x.ToObjectIdentity())).Select(x => new ObjectIdentityFilled(x.CategoryID, x.CategoryGuid, x.CategoryName));
        }
    }
}
