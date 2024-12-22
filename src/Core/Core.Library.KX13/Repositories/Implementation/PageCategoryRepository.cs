using System.Data;

namespace Core.Repositories.Implementation
{
    [AutoDependencyInjection]
    [Obsolete("Use IContentCategoryRepository")]
    public class PageCategoryRepository(IContentCategoryRepository contentCategoryRepository) : IPageCategoryRepository
    {
        private readonly IContentCategoryRepository _contentCategoryRepository = contentCategoryRepository;

        public Task<IEnumerable<CategoryItem>> GetCategoriesByNodeAsync(int nodeID) => _contentCategoryRepository.GetCategoriesByContentIdentity(nodeID.ToContentIdentity());

        public Task<IEnumerable<CategoryItem>> GetCategoriesByNodesAsync(IEnumerable<int> nodeIDs) => _contentCategoryRepository.GetCategoriesByContentIdentities(nodeIDs.Select(x => x.ToContentIdentity()));

        public Task<IEnumerable<CategoryItem>> GetCategoryItemsByPathAsync(string path) => _contentCategoryRepository.GetCategoriesByTreeIdentity(path.ToTreeIdentity());
    }
}
