using CMS.Websites;
using CMS.Websites.Routing;
using System.Data;

namespace Core.Repositories.Implementation
{
    /// <summary>
    /// This can be a bit confusing, but in Xperience by Kentico, there is the WebSiteChannel which has a normal channel.
    /// 
    /// THe WebsiteChannelContext WebsiteChannelID is the CMS_WebsiteChannel.WebsiteChannelID, but the WebsiteChannelName is the linked CMS_Channel.ChannelName
    /// </summary>
    /// <param name="websiteChannelContext"></param>
    /// <param name="progressiveCache"></param>
    /// <param name="cacheDependencyBuilderFactory"></param>
    /// <param name="websiteChannelInfoProvider"></param>
    public class SiteRepository(IWebsiteChannelContext websiteChannelContext,
        IProgressiveCache progressiveCache,
        ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IInfoProvider<ChannelInfo> channelInfoProvider) : ISiteRepository
    {
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider = websiteChannelInfoProvider;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider = channelInfoProvider;

        /// <summary>
        /// Not doing a Maybe on this, this is almost always translating a channel ID to the name, and shouldn't be a case where you have a channel ID that no longer exists.
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        public string ChannelNameById(int channelID) => GetWebsiteChannelIdToChannelContext().TryGetValue(channelID, out var channel) ? channel.ChannelName : string.Empty;

        public Maybe<string> CurrentChannelDisplayName() => GetWebsiteChannelIdToChannelContext().TryGetValue(_websiteChannelContext.WebsiteChannelID, out var channel) ? channel.ChannelDisplayName : Maybe.None;

        public Maybe<int> CurrentChannelID() => _websiteChannelContext.WebsiteChannelID.AsMaybeIfTrue(x => x > 0);

        /// <summary>
        /// The WebsiteChannelName is actually the CMS_Channel.ChannelName, confusing I know.
        /// </summary>
        /// <returns></returns>
        public Maybe<string> CurrentChannelName() => _websiteChannelContext.WebsiteChannelID > 0 ? _websiteChannelContext.WebsiteChannelName : Maybe.None;
 
        public Maybe<int> GetChannelID(string? channelName = null) => GetChannelById().TryGetValue((channelName ?? _websiteChannelContext.WebsiteChannelName ?? string.Empty).ToLowerInvariant(), out var channelID) ? channelID : Maybe.None;

        #region "Obsolete"

        [Obsolete("Not applicable in XbyK")]
        public string CurrentSiteDisplayName()
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not applicable in XbyK")]
        public int CurrentSiteID()
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not applicable in XbyK")]
        public string CurrentSiteName()
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not applicable in XbyK")]
        public Task<string> CurrentSiteNameAsync()
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not applicable in XbyK")]
        public Task<int> GetSiteIDAsync(string? siteName = null)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Not applicable in XbyK")]
        public string SiteNameById(int siteID)
        {
            throw new NotImplementedException();
        }

        #endregion

        private Dictionary<int, WebsitesChannelContextInfo> GetWebsiteChannelIdToChannelContext()
        {
            return _progressiveCache.Load(cs => {
                if (cs.Cached) {
                    cs.CacheDependency = _cacheDependencyBuilderFactory.Create(false).ObjectType(WebsiteChannelInfo.OBJECT_TYPE).GetCMSCacheDependency();
                }

                return _websiteChannelInfoProvider.Get()
                        .Source(x => x.InnerJoin<ChannelInfo>(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), nameof(ChannelInfo.ChannelID)))
                        .Result.Tables[0].Rows.Cast<DataRow>()
                        .ToDictionary(key => (int)key[nameof(WebsiteChannelInfo.WebsiteChannelID)], value => new WebsitesChannelContextInfo(
                            ChannelID: (int)value[nameof(ChannelInfo.ChannelID)],
                            ChannelName: (string)value[nameof(ChannelInfo.ChannelName)],
                            ChannelDisplayName: (string)value[nameof(ChannelInfo.ChannelDisplayName)]
                            ));
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetWebsiteChannelById_SiteRepository"));
        }

        private Dictionary<string, int> GetChannelById()
        {
            return _progressiveCache.Load(cs => {
                if (cs.Cached) {
                    cs.CacheDependency = _cacheDependencyBuilderFactory.Create(false).ObjectType(WebsiteChannelInfo.OBJECT_TYPE).GetCMSCacheDependency();
                }

                return _channelInfoProvider.Get()
                        .Columns(nameof(ChannelInfo.ChannelID), nameof(ChannelInfo.ChannelName))
                        .GetEnumerableTypedResult()
                        .ToDictionary(key => key.ChannelName.ToLowerInvariant(), value => value.ChannelID);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetChannelById_SiteRepository"));
        }

        record WebsitesChannelContextInfo(int ChannelID, string ChannelName, string ChannelDisplayName);
    }
}
