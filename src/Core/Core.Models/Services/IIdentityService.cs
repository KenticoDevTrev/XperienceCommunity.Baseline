namespace Core.Services
{
    /// <summary>
    /// These services help fill all the "Maybe" values within the various identity objects, in case you need one value but are passed another.
    /// </summary>
    public interface IIdentityService
    {
        Task<Result<TreeIdentity>> HydrateTreeIdentity(TreeIdentity identity);

        Task<Result<TreeCultureIdentity>> HydrateTreeCultureIdentity(TreeCultureIdentity identity);

        Task<Result<ContentIdentity>> HydrateContentIdentity(ContentIdentity identity);

        Task<Result<ContentCultureIdentity>> HydrateContentCultureIdentity(ContentCultureIdentity identity);

        Task<Result<ObjectIdentity>> HydrateObjectIdentity(ObjectIdentity identity, string className);

        [Obsolete("Use HydrateTreeIdentity or HydrateContentIdentity")]
        Task<Result<NodeIdentity>> HydrateNodeIdentity(NodeIdentity identity);
        
        [Obsolete("Use HydrateTreeCultureIdentity or HydrateContentCultureIdentity")]
        Task<Result<DocumentIdentity>> HydrateDocumentIdentity(DocumentIdentity identity);

    }
}
