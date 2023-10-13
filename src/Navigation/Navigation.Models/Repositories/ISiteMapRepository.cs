namespace Navigation.Repositories
{
    public interface ISiteMapRepository
    {
        /// <summary>
        /// Gets the SiteMapUrlSet given the SiteMapOptions
        /// </summary>
        /// <param name="options">The SiteMapOptions you wish to pass to get your SitemapNodes</param>
        /// <returns>List of all the SitemapNodes</returns>
        Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetAsync(SiteMapOptions options);

        /// <summary>
        /// Gets the Site map nodes using default settings.  In Kentico this is through the 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetAsync();

        /// <summary>
        /// Gets the SiteMapUrlSet given the SiteMapOptionsPageBuidlerOnly, this method is quicker and more accurate than the GetSiteMapUrlSet but can't handle "UrlColumn" items.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetPageBuilderAsync(SiteMapOptionsPageBuilderOnly options);
    }
}