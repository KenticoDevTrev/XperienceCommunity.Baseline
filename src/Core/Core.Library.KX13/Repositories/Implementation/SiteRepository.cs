using CMS.Base;
using CMS.SiteProvider;

namespace Core.Repositories.Implementation
{
    [AutoDependencyInjection]
    public class SiteRepository(
        ISiteService _siteService,
        ISiteInfoProvider _siteInfoProvider,
        IProgressiveCache _progressiveCache) : ISiteRepository
    {
        public string CurrentSiteName() => CurrentChannelName().Value;

        public string CurrentSiteDisplayName() => CurrentChannelDisplayName().Value;

        public int CurrentSiteID() => CurrentChannelID().Value;

        public Task<string> CurrentSiteNameAsync() => Task.FromResult(CurrentChannelName().Value);

        public Task<int> GetSiteIDAsync(string? siteName = null) => Task.FromResult(GetChannelID(siteName).GetValueOrDefault(0));

        public string SiteNameById(int siteId) => ChannelNameById(siteId);

        public Maybe<int> GetChannelID(string? channelName = null)
        {
            if (channelName.AsNullOrWhitespaceMaybe().HasNoValue || _siteService.CurrentSite.SiteName.Equals(channelName, StringComparison.InvariantCultureIgnoreCase)) {
                return _siteService.CurrentSite.SiteID;
            } else {
                // Does have value here
                string siteNameVal = channelName.GetValueOrDefault(string.Empty);
                var siteID = _progressiveCache.Load(cs => {
                    if (cs.Cached) {
                        cs.CacheDependency = CacheHelper.GetCacheDependency($"{SiteInfo.OBJECT_TYPE}|byname|{siteNameVal}");
                    }
                    return _siteInfoProvider.Get(siteNameVal)?.SiteID ?? 0;
                }, new CacheSettings(1440, "GetChannelID", siteNameVal));
                return siteID.AsMaybeIfTrue(x => x > 0);
            }
        }

        public Maybe<string> CurrentChannelName() => _siteService.CurrentSite.SiteName;

        public Maybe<string> CurrentChannelDisplayName() => _siteService.CurrentSite.DisplayName;

        public Maybe<int> CurrentChannelID() => _siteService.CurrentSite.SiteID;

        public string ChannelNameById(int channelID)
        {
            // Doing non async as this may be referenced a lot and once cached will not impact anything.
            var siteIdToName = _progressiveCache.Load(cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{SiteInfo.OBJECT_TYPE}|all");
                }
                return _siteInfoProvider.Get()
                .ColumnsSafe(nameof(SiteInfo.SiteID), nameof(SiteInfo.SiteName))
                .GetEnumerableTypedResult()
                .ToDictionary(key => key.SiteID, value => value.SiteName);

            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "SiteNameByID"));
            return siteIdToName.GetValueOrMaybe(channelID).GetValueOrDefault(string.Empty);
        }
    }
}
