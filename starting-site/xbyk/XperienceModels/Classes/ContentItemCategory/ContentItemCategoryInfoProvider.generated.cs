using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CMS.DataEngine;

namespace RelationshipsExtended
{
    /// <summary>
    /// Class providing <see cref="ContentItemCategoryInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IContentItemCategoryInfoProvider))]
    public partial class ContentItemCategoryInfoProvider : AbstractInfoProvider<ContentItemCategoryInfo, ContentItemCategoryInfoProvider>, IContentItemCategoryInfoProvider
    {
        /// <summary>
        /// Gets an instance of the <see cref="ContentItemCategoryInfo"/> binding structure.
        /// </summary>
        /// <param name="contentitemId">Content item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <returns>Returns an instance of <see cref="ContentItemCategoryInfo"/> corresponding to given identifiers or null.</returns>
        public virtual ContentItemCategoryInfo Get(int contentitemId, int tagId)
        {
            return GetObjectQuery().TopN(1)
                .WhereEquals("ContentItemCategoryContentItemID", contentitemId)
                .WhereEquals("ContentItemCategoryTagID", tagId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Asynchronously gets an instance of the <see cref="ContentItemCategoryInfo"/> binding structure.
        /// </summary>
        /// <param name="contentitemId">Content item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Returns a task returning either an instance of <see cref="ContentItemCategoryInfo"/> corresponding to given identifiers or null.</returns>
        public async virtual Task<ContentItemCategoryInfo> GetAsync(int contentitemId, int tagId, CancellationToken? cancellationToken = null)
        {
            var query = await GetObjectQuery().TopN(1)
                .WhereEquals("ContentItemCategoryContentItemID", contentitemId)
                .WhereEquals("ContentItemCategoryTagID", tagId)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return query.FirstOrDefault();
        }


        /// <summary>
        /// Deletes <see cref="ContentItemCategoryInfo"/> binding.
        /// </summary>
        /// <param name="contentitemId">Content item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        public virtual void Remove(int contentitemId, int tagId)
        {
            var infoObj = Get(contentitemId, tagId);
            if (infoObj != null)
            {
                Delete(infoObj);
            }
        }


        /// <summary>
        /// Creates <see cref="ContentItemCategoryInfo"/> binding.
        /// </summary>
        /// <param name="contentitemId">Content item ID.</param>
        /// <param name="tagId">Tag ID.</param>
        public virtual void Add(int contentitemId, int tagId)
        {
            // Create new binding
            var infoObj = new ContentItemCategoryInfo();
            infoObj.ContentItemCategoryContentItemID = contentitemId;
            infoObj.ContentItemCategoryTagID = tagId;

            // Save to the database
            Set(infoObj);
        }
    }
}