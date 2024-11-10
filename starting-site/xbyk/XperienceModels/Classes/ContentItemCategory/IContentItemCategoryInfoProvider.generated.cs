using System.Threading;
using System.Threading.Tasks;

using CMS.DataEngine;

namespace RelationshipsExtended
{
    /// <summary>
    /// Declares members for <see cref="ContentItemCategoryInfo"/> management.
    /// </summary>
    public partial interface IContentItemCategoryInfoProvider : IInfoProvider<ContentItemCategoryInfo>
    {
        /// <summary>
        /// Gets an instance of the <see cref="ContentItemCategoryInfo"/> binding structure.
        /// </summary>
        /// <param name="contentitemId">Content item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <returns>Returns an instance of <see cref="ContentItemCategoryInfo"/> corresponding to given identifiers or null.</returns>
        ContentItemCategoryInfo Get(int contentitemId, int tagId);


        /// <summary>
        /// Asynchronously gets an instance of the <see cref="ContentItemCategoryInfo"/> binding structure.
        /// </summary>
        /// <param name="contentitemId">Content item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Returns a task returning either an instance of <see cref="ContentItemCategoryInfo"/> corresponding to given identifiers or null.</returns>
        Task<ContentItemCategoryInfo> GetAsync(int contentitemId, int tagId, CancellationToken? cancellationToken = null);


        /// <summary>
        /// Deletes <see cref="ContentItemCategoryInfo"/> binding.
        /// </summary>
        /// <param name="contentitemId">Content item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        void Remove(int contentitemId, int tagId);


        /// <summary>
        /// Creates <see cref="ContentItemCategoryInfo"/> binding.
        /// </summary>
        /// <param name="contentitemId">Content item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        void Add(int contentitemId, int tagId);
    }
}