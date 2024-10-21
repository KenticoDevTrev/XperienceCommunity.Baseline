namespace Core.Repositories
{
    public interface IMediaRepository
    {
        /// <summary>
        /// Gets the Attachment Url, if using this method the page must include the DocumentID column if using Kentico
        /// </summary>
        /// <param name="documentID">The Page's document id, if not provided will use the current page context</param>
        /// <returns></returns>
        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemLanguageMetadataAssets(int? contentItemMetadataId). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        Task<IEnumerable<MediaItem>> GetPageAttachmentsAsync(int? documentID = null);

        /// <summary>
        /// Gets the Attachment Url, if using this method the page must include the DocumentID column if using Kentico
        /// </summary>
        /// <param name="attachmentGuid">The Attachment Guid</param>
        /// <param name="documentID">The page's document id, if not provided will use the current page context</param>
        /// <returns></returns>
        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAsset(attachmentGuid.ToObjectIdentity(), \"EventualContentAssetFieldName\"). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        Task<Result<MediaItem>> GetPageAttachmentAsync(Guid attachmentGuid, int? documentID = null);

        /// <summary>
        /// Gets the Attachment item
        /// </summary>
        /// <param name="attachmentGuid">The Attachment Guid</param>
        /// <returns></returns>
        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAsset(attachmentGuid.ToObjectIdentity(), \"EventualContentAssetFieldName\"). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        Task<Result<MediaItem>> GetAttachmentItemAsync(Guid attachmentGuid);

        /// <summary>
        /// Gets the IEnumerable of Attachment Items
        /// </summary>
        /// <param name="attachmentGuids">IEnumerable of Attachment Guids</param>
        /// <returns></returns>
        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAssets(attachmentGuids.Select(x => x.ToObjectIdentity())). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        Task<IEnumerable<MediaItem>> GetAttachmentsAsync(IEnumerable<Guid> attachmentGuids);

        /// <summary>
        /// Gets the Content Item Assets from any Asset Field.  If language is not provided, will use current language.
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        Task<IEnumerable<MediaItem>> GetContentItemAssets(IEnumerable<ContentIdentity> contentItems, string? language = null);

        /// <summary>
        /// Gets the Content Item Assets from the specified Asset Field.
        /// </summary>
        /// <param name="contentItems"></param>
        /// <param name="assetFieldGuid"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        Task<IEnumerable<MediaItem>> GetContentItemAssets(IEnumerable<ContentIdentity> contentItems, Guid assetFieldGuid, string? language = null);

        /// <summary>
        /// Gets the Content Item Assets from any Asset Field.  If language is not provided, will use current language.
        /// </summary>
        /// <param name="contentItems"></param>
        /// <param name="assetFieldName"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        Task<IEnumerable<MediaItem>> GetContentItemAssets(IEnumerable<ContentIdentity> contentItems, string assetFieldName, string? language = null);

        /// <summary>
        /// Gets the Content Item Assets from any Asset Field.  If language is not provided, will use current language.
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        Task<IEnumerable<MediaItem>> GetContentItemAssets(ContentIdentity contentItem, string? language = null);

        /// <summary>
        /// Gets the Content Item Asset from the specified Asset Field.  If language is not provided, will use current language.
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="assetFieldGuid"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        Task<Result<MediaItem>> GetContentItemAsset(ContentIdentity contentItem, Guid assetFieldGuid, string? language = null);

        /// <summary>
        /// Gets the Content Item Asset from the specified Asset Field.  If language is not provided, will use current language.
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="assetFieldGuid"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        Task<Result<MediaItem>> GetContentItemAsset(ContentIdentity contentItem, string assetFieldName, string? language = null);


        /// <summary>
        /// Gets the Content Item Assets from any Asset Field for the specific content item.
        /// </summary>
        /// <param name="contentItemMetadataId">The Content Item Metadata Id, if not provided will use the current context to try to find it</param>
        /// <returns></returns>
        Task<IEnumerable<MediaItem>> GetContentItemLanguageMetadataAssets(int? contentItemMetadataId);

        /// <summary>
        /// Gets the Content Item Assets from the specified Asset Field for the specific content item.
        /// </summary>
        /// <param name="contentItemMetadataId">The Content Item Metadata Id, if not provided will use the current context to try to find it</param>
        /// <param name="assetFieldGuid"></param>
        /// <returns></returns>
        Task<Result<MediaItem>> GetContentItemLanguageMetadataAsset(int? contentItemMetadataId, Guid assetFieldGuid);

        /// <summary>
        /// Gets the Content Item Assets from the specified Asset Field for the specific content item.
        /// </summary>
        /// <param name="contentItemMetadataId">The Content Item Metadata Id, if not provided will use the current context to try to find it</param>
        /// <param name="assetFieldName"></param>
        /// <returns></returns>
        Task<Result<MediaItem>> GetContentItemLanguageMetadataAsset(int? contentItemMetadataId, string assetFieldName);

        /// <summary>
        /// Gets the Media File Item
        /// </summary>
        /// <param name="fileGuid"></param>
        /// <returns></returns>
        Task<Result<MediaItem>> GetMediaItemAsync(Guid fileGuid);

        /// <summary>
        /// Gets the Media Items by the given Library Code Name
        /// </summary>
        /// <param name="libraryName"></param>
        /// <returns></returns>
        Task<IEnumerable<MediaItem>> GetMediaItemsByLibraryAsync(string libraryName);

        /// <summary>
        /// The path of the files, either the full path (including the library 
        /// </summary>
        /// <param name="path">The path within the media library folder</param>
        /// <param name="libraryName">Optional Library name</param>
        /// <returns></returns>
        Task<IEnumerable<MediaItem>> GetMediaItemsByPathAsync(string path, string? libraryName = null);

        /// <summary>
        /// Gets the sitename of the media or attachment by Guid so it can be appended, used primarily in the middleware to update getattachment
        /// </summary>
        /// <returns></returns>
        Task<Result<string>> GetMediaAttachmentSiteNameAsync(Guid mediaOrAttachmentGuid);

    }
}
