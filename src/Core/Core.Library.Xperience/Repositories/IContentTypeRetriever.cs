namespace Core.Repositories
{
    /// <summary>
    /// Internal usage mainly, it's Transient so can be used by Search Index.  IdentityService uses this for it's same methods.
    /// </summary>
    public interface IContentTypeRetriever
    {
        /// <summary>
        /// Highly Optimized and cached retrieval of the Content Type Name (ClassName) of the given Content Item
        /// </summary>
        /// <param name="identity">The Tree Identity</param>
        /// <returns>The Content Type Code Name</returns>
        Task<Result<string>> GetContentType(TreeIdentity identity);

        /// <summary>
        /// Highly Optimized and cached retrieval of the Content Type Name (ClassName) of the given Content Item
        /// </summary>
        /// <param name="identity">The Content Identity</param>
        /// <returns>The Content Type Code Name</returns>
        Task<Result<string>> GetContentType(ContentIdentity identity);

        /// <summary>
        /// Highly Optimized and cached retrieval of the Content Type Name (ClassName) of the given Content Item
        /// </summary>
        /// <param name="identity">The Tree Culture Identity</param>
        /// <returns>The Content Type Code Name</returns>
        Task<Result<string>> GetContentType(TreeCultureIdentity identity);

        /// <summary>
        /// Highly Optimized and cached retrieval of the Content Type Name (ClassName) of the given Content Item
        /// </summary>
        /// <param name="identity">The Content Culture Identity</param>
        /// <returns>The Content Type Code Name</returns>
        Task<Result<string>> GetContentType(ContentCultureIdentity identity);
    }
}
