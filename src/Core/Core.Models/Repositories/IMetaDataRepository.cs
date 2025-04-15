namespace Core.Repositories
{
    public interface IMetaDataRepository
    {
        /// <summary>
        /// Gets the Meta Data based on the current page
        /// </summary>
        /// <param name="thumbnail">Optional Thumbnail override</param>
        /// <returns>The Page Meta Data object</returns>
        Task<Result<PageMetaData>> GetMetaDataAsync(string? thumbnail = null);
        /// <summary>
        /// Gets the Meta Data based on the given page
        /// </summary>
        /// <param name="contentCultureId">The Content Culture ID of the page you wish to display the meta data for</param>
        /// <param name="thumbnail">Optional Thumbnail override</param>
        /// <returns>The Page Meta Data object</returns>
        [Obsolete("Use GetMetaDataAsync(TreeCultureIdentity treeCultureIdentity, string? thumbnail = null). The TreeIdentity should be the NodeID (KX13) or WebPageItemID (XbyK) along with the proper culture")]
        Task<Result<PageMetaData>> GetMetaDataAsync(int contentCultureId, string? thumbnail = null);
        /// <summary>
        /// Gets the Meta Data based on the given page
        /// </summary>
        /// <param name="contentCultureGuid">The Content Culture GUID of the page you wish to display the meta data for</param>
        /// <param name="thumbnail">Optional Thumbnail override</param>
        /// <returns>The Page Meta Data object</returns>
        [Obsolete("Use GetMetaDataAsync(TreeCultureIdentity treeCultureIdentity, string? thumbnail = null). The TreeIdentity should be the NodeGUID (KX13) or WebPageItemGUID (XbyK) along with the proper culture")]
        Task<Result<PageMetaData>> GetMetaDataAsync(Guid contentCultureGuid, string? thumbnail = null);

        /// <summary>
        /// Retrieves the Metadata given the page identified
        /// </summary>
        /// <param name="treeCultureIdentity">The Web Page Identity</param>
        /// <param name="thumbnail">Optioanl Thumbnail Override</param>
        /// <returns></returns>
        Task<Result<PageMetaData>> GetMetaDataAsync(TreeCultureIdentity treeCultureIdentity, string? thumbnail = null);

        /// <summary>
        /// Retrieves the Metadata given the content item (for non page items)
        /// </summary>
        /// <param name="contentCultureIdentity">The Content Item (non web page)</param>
        /// <param name="canonicalUrl">The URL (since this is NOT a web page)</param>
        /// <param name="thumbnail">Optioanl Thumbnail Override</param>
        /// <returns></returns>
        Task<Result<PageMetaData>> GetMetaDataForReusableContentAsync(ContentCultureIdentity contentCultureIdentity, string? canonicalUrl, string? thumbnail = null);
    }
}
