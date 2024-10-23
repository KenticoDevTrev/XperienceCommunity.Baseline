namespace Core.Services
{
    /// <summary>
    /// These services help fill all the "Maybe" values within the various identity objects, in case you need one value but are passed another.
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>
        /// Highly Optimized and cached Hydration Service that will fill in missing values on the Identity if not present.  Use in conjuction with the TreeIdentity Extension Methods ".GetOrRetrieve____(IIdentityService)"
        /// </summary>
        /// <param name="identity">The Tree Identity</param>
        /// <returns>The Identity (all valid fields filled in), or a result failure if it could not locate the item</returns>
        Task<Result<TreeIdentity>> HydrateTreeIdentity(TreeIdentity identity);

        /// <summary>
        /// Highly Optimized and cached Hydration Service that will fill in missing values on the Identity if not present.  Use in conjuction with the TreeCultureIdentity Extension Methods ".GetOrRetrieve____(IIdentityService)"
        /// </summary>
        /// <param name="identity">The Tree Culture Identity</param>
        /// <returns>The Identity (all valid fields filled in), or a result failure if it could not locate the item</returns>
        Task<Result<TreeCultureIdentity>> HydrateTreeCultureIdentity(TreeCultureIdentity identity);

        /// <summary>
        /// Highly Optimized and cached Hydration Service that will fill in missing values on the Identity if not present.  Use in conjuction with the ContentIdentity Extension Methods ".GetOrRetrieve____(IIdentityService)"
        /// </summary>
        /// <param name="identity">The Content Identity</param>
        /// <returns>The Identity (all valid fields filled in), or a result failure if it could not locate the item</returns>
        Task<Result<ContentIdentity>> HydrateContentIdentity(ContentIdentity identity);

        /// <summary>
        /// Highly Optimized and cached Hydration Service that will fill in missing values on the Identity if not present.  Use in conjuction with the ContentCultureIdentity Extension Methods ".GetOrRetrieve____(IIdentityService)"
        /// </summary>
        /// <param name="identity">The Content Culture Identity</param>
        /// <returns>The Identity (all valid fields filled in), or a result failure if it could not locate the item</returns>
        Task<Result<ContentCultureIdentity>> HydrateContentCultureIdentity(ContentCultureIdentity identity);

        /// <summary>
        /// Highly Optimized and cached Hydration Service that will fill in missing values on the Identity if not present.  Use in conjuction with the ObjectIdentity Extension Methods ".GetOrRetrieve____(IIdentityService)"
        /// </summary>
        /// <param name="identity">The Object Identity</param>
        /// <param name="className">The Object Identity</param>
        /// <returns>The Identity (all valid fields filled in), or a result failure if it could not locate the item</returns>
        Task<Result<ObjectIdentity>> HydrateObjectIdentity(ObjectIdentity identity, string className);

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

        [Obsolete("Use HydrateTreeIdentity or HydrateContentIdentity")]
        Task<Result<NodeIdentity>> HydrateNodeIdentity(NodeIdentity identity);
        
        [Obsolete("Use HydrateTreeCultureIdentity or HydrateContentCultureIdentity")]
        Task<Result<DocumentIdentity>> HydrateDocumentIdentity(DocumentIdentity identity);

    }
}
