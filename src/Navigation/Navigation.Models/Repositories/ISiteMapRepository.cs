namespace Navigation.Repositories
{
    public interface ISiteMapRepository
    {
        /// <summary>
        /// Gets the SiteMapUrlSet given the SiteMapOptions
        /// </summary>
        /// <param name="options">The SiteMapOptions you wish to pass to get your SitemapNodes</param>
        /// <returns>List of all the SitemapNodes</returns>
        [Obsolete(@"Will not be used in Xperience by Kentico

Instead use your own logic to retrieve the IEnumerable<IContentQueryDataContainer> of your items and then use the ISiteMapService.ConvertToSitemapNode to convert.

May wish to overwrite and implement a custom ISiteMapCustomizationService to parsing individual page types (using the IContentQueryModelTypeMapper or IContentItemQueryResultMapper) to map to your own data")]
        Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetAsync(SiteMapOptions options);

        /// <summary>
        /// Gets the Site map nodes using default settings.  In Kentico this is through the 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SitemapNode>> GetSiteMapUrlSetAsync();
    }
}