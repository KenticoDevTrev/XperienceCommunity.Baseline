﻿using CMS.Base;
using CMS.SiteProvider;

namespace Core.Repositories.Implementation
{
    [AutoDependencyInjection]
    public class SiteRepository(
        ISiteService _siteService,
        ISiteInfoProvider _siteInfoProvider,
        IProgressiveCache _progressiveCache) : ISiteRepository
    {
        public string CurrentSiteName()
        {
            return _siteService.CurrentSite.SiteName;
        }

        public string CurrentSiteDisplayName()
        {
            return _siteService.CurrentSite.DisplayName;
        }

        public int CurrentSiteID()
        {
            return _siteService.CurrentSite.SiteID;
        }

        public Task<string> CurrentSiteNameAsync()
        {
            return Task.FromResult(_siteService.CurrentSite.SiteName);
        }

        public async Task<int> GetSiteIDAsync(string? siteName = null)
        {
            if (siteName.AsNullOrWhitespaceMaybe().HasNoValue || _siteService.CurrentSite.SiteName.Equals(siteName, StringComparison.InvariantCultureIgnoreCase))
            {
                return SiteContext.CurrentSiteID;
            }
            else
            {
                // Does have value here
                string siteNameVal = siteName.GetValueOrDefault(string.Empty);
                var siteID = await _progressiveCache.LoadAsync(async cs =>
                {
                    if (cs.Cached)
                    {
                        cs.CacheDependency = CacheHelper.GetCacheDependency($"{SiteInfo.OBJECT_TYPE}|byname|{siteNameVal}");
                    }
                    return (await _siteInfoProvider.GetAsync(siteNameVal))?.SiteID ?? 0;
                }, new CacheSettings(1440, "GetSiteID", siteNameVal));
                return siteID;
            }
        }

        public string SiteNameById(int siteId)
        {
            // Doing non async as this may be referenced a lot and once cached will not impact anything.
            Dictionary<int, string> siteIdToName = _progressiveCache.Load(cs =>
            {
                if (cs.Cached)
                {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{SiteInfo.OBJECT_TYPE}|all");
                }
                return _siteInfoProvider.Get()
                .ColumnsSafe(nameof(SiteInfo.SiteID), nameof(SiteInfo.SiteName))
                .GetEnumerableTypedResult()
                .ToDictionary(key => key.SiteID, value => value.SiteName);

            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "SiteNameByID"));
            return siteIdToName.GetValueOrMaybe(siteId).GetValueOrDefault(string.Empty);
        }
    }
}
