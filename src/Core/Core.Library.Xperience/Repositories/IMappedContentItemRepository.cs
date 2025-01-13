namespace Core.Repositories
{
    public interface IMappedContentItemRepository
    {
        /// <summary>
        /// Given the Content Identity, retrieves the fully mapped object and all children.  Useful if you do NOT know the content type.
        /// </summary>
        /// <param name="identity">The Content Identity</param>
        /// <param name="language">Optional Preferred language, if not provided will use the IPreferredLanguageRetriever culture.</param>
        /// <returns>The mapped Content Item, or Result.Failure if it was not found, no strongly Typed class is registered for the found item.</returns>
        Task<Result<object>> GetContentItem(ContentIdentity identity, string? language = null);

        /// <summary>
        /// Given the Content Identity, retrieves the fully mapped object and all children IF the primary object is of the type (or inherits the type) you specify.  Useful if you do NOT know the content type.
        /// </summary>
        /// <param name="identity">The Content Identity</param>
        /// <param name="language">Optional Preferred language, if not provided will use the IPreferredLanguageRetriever culture.</param>
        /// <returns>The mapped Content Item, or Result.Failure if it was not found, no strongly Typed class is registered, or it's not the type you specified or </returns>
        Task<Result<T>> GetContentItem<T>(ContentIdentity identity, string? language = null);
    }
}
