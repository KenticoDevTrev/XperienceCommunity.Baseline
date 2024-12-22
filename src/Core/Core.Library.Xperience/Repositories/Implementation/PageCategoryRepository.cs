namespace Core.Repositories.Implementation
{
    [Obsolete("Use IContentCategoryRepository, this was just here for legacy KX13 compatibility")]
    public class PageCategoryRepository : IPageCategoryRepository
    {
        public Task<IEnumerable<CategoryItem>> GetCategoriesByNodeAsync(int nodeID)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CategoryItem>> GetCategoriesByNodesAsync(IEnumerable<int> nodeIDs)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CategoryItem>> GetCategoryItemsByPathAsync(string path)
        {
            throw new NotImplementedException();
        }
    }
}
