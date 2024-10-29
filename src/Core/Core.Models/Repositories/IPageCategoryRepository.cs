namespace Core.Repositories
{
    [Obsolete("Use IContentCategoryRepository")]
    public interface IPageCategoryRepository
    {
        /// <summary>
        /// Gets the page's categories (node categories)
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns>The Categories</returns>
        [Obsolete("Use IContentCategoryRepository.GetCategoriesByContentIdentity(nodeId.ToContentIdentity()), NOTE This now will get Document Categories (CMS_DocumentCategory) and Node Categories (CMS_TreeCategory)")]
        Task<IEnumerable<CategoryItem>> GetCategoriesByNodeAsync(int nodeID);

        /// <summary>
        /// Gets the pages categories (node categories)
        /// </summary>
        /// <param name="nodeIDs">the nodes</param>
        /// <returns>The Categories of all the nodes</returns>
        [Obsolete("Use IContentCategoryRepository.GetCategoriesByContentIdentities(nodeIDs.Select(x => x.ToContentIdentity())), NOTE This now will get Document Categories (CMS_DocumentCategory) and Node Categories (CMS_TreeCategory)")]
        Task<IEnumerable<CategoryItem>> GetCategoriesByNodesAsync(IEnumerable<int> nodeIDs);

        /// <summary>
        /// Gets the page's categories (node categories)
        /// </summary>
        /// <param name="path">The page's path</param>
        /// <returns>The Categories</returns>
        [Obsolete("Use IContentCategoryRepository.GetCategoriesByTreeIdentity(path.ToTreeIdentity()), NOTE This now will get Document Categories (CMS_DocumentCategory) and Node Categories (CMS_TreeCategory)")]
        Task<IEnumerable<CategoryItem>> GetCategoryItemsByPathAsync(string path);
    }
}
