namespace Core.Repositories
{
    public interface IMetaDataRepository
    {
        /// <summary>
        /// Gets the Meta Data based on the current page
        /// </summary>
        /// <param name="Thumbnail">Optional Thumbnail override</param>
        /// <returns>The Page Meta Data object</returns>
        Task<Result<PageMetaData>> GetMetaDataAsync(string? thumbnail = null);
        /// <summary>
        /// Gets the Meta Data based on the given page
        /// </summary>
        /// <param name="contentCultureId">The Content Culture ID of the page you wish to display the meta data for</param>
        /// <param name="Thumbnail">Optional Thumbnail override</param>
        /// <returns>The Page Meta Data object</returns>
        Task<Result<PageMetaData>> GetMetaDataAsync(int contentCultureId, string? thumbnail = null);
        /// <summary>
        /// Gets the Meta Data based on the given page
        /// </summary>
        /// <param name="contentCultureGuid">The Content Culture GUID of the page you wish to display the meta data for</param>
        /// <param name="Thumbnail">Optional Thumbnail override</param>
        /// <returns>The Page Meta Data object</returns>
        Task<Result<PageMetaData>> GetMetaDataAsync(Guid contentCultureGuid, string? thumbnail = null);
    }
}
