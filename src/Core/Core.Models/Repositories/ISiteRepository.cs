namespace Core.Repositories
{
    public interface ISiteRepository
    {

        /// <summary>
        /// Gets the ChannelID for the given channel name.
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        Maybe<int> GetChannelID(string? channelName = null);

        /// <summary>
        /// Gets the current Channel Code Name (if there is a website context, there will always be in KX13, but not always in XbyK)
        /// </summary>
        /// <returns></returns>
        Maybe<string> CurrentChannelName();

        /// <summary>
        /// Gets the current Channel Display Name (if there is a website context, there will always be in KX13, but not always in XbyK)
        /// </summary>
        /// <returns>The Site Name</returns>
        Maybe<string> CurrentChannelDisplayName();


        /// <summary>
        /// Gets the current Channel ID (if there is a website context, there will always be in KX13, but not always in XbyK)
        /// </summary>
        /// <returns></returns>
        Maybe<int> CurrentChannelID();

        /// <summary>
        /// Gets the current channel name by the ChannelID (NOT WebsiteChannelID)
        /// 
        /// Synchronous as cached and rarely changes but referenced a lot in requests.
        /// </summary>
        /// <param name="channelID">The Channel ID (or SiteID for KX13)</param>
        /// <returns>The ChannelName if found, string.empty if not, not doing a Maybe<string> on this because this is usually used to for translating references an no content would have a ChannelID that no longer exists.</returns>
        string ChannelNameById(int channelID);

        /// <summary>
        /// Gets the SiteID for the given SiteName (KX13), or the ChannelID (for XbyK)
        /// </summary>
        /// <param name="SiteName">The Site Name</param>
        /// <returns>The SiteID</returns>
        [Obsolete("Use GetChannelID, the ChannelID will be the SiteID in KX13")]
        Task<int> GetSiteIDAsync(string? siteName = null);

        /// <summary>
        /// Gets the current Site Name, synchronously available as it is usually derived from the current url and no database call needed.
        /// </summary>
        /// <returns>The Site Name</returns>
        [Obsolete("Use CurrentChannelName")]
        string CurrentSiteName();

        /// <summary>
        /// Gets the current Site Display name, synchronously available as it is usually derived from the current url and no database call needed. 
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use CurrentChannelDisplayName")]
        string CurrentSiteDisplayName();

        /// <summary>
        /// Gets the current Site Name
        /// </summary>
        /// <returns>The Site Name</returns>
        [Obsolete("Use CurrentChannelName, no point in having an async when a non async is available")]
        Task<string> CurrentSiteNameAsync();

        /// <summary>
        /// Gets the current Site ID
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use CurrentChannelID")]
        int CurrentSiteID();

        /// <summary>
        /// Gets the site name (not lowerecased, just as is) for the given SiteID.  Empty string if not found.
        /// 
        /// Synchronous as cached and rarely changes but referenced a lot in requests.
        /// </summary>
        /// <param name="siteID">The SiteID</param>
        /// <returns>The SiteName</returns>
        [Obsolete("Use ChannelNameById")]
        string SiteNameById(int siteID);

	}
}