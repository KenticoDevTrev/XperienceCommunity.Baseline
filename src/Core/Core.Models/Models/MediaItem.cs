namespace Core.Models
{
    public record MediaItem
    {
        /// <summary>
        /// Media Item, primarily sourced from Media File (MediaGuid = FileGuid) or for KX13 Page Attachment (MediaGuid = AttachmentGuid)
        /// </summary>
        /// <param name="mediaGUID"></param>
        /// <param name="mediaName"></param>
        /// <param name="mediaTitle"></param>
        /// <param name="mediaExtension"></param>
        /// <param name="mediaDirectUrl"></param>
        /// <param name="mediaPermanentUrl"></param>
        public MediaItem(Guid mediaGUID, string mediaName, string mediaTitle, string mediaExtension, string mediaDirectUrl, string mediaPermanentUrl)
        {
            MediaGUID = mediaGUID;
            ContentItemMediaIdentifiers = Maybe.None;
            MediaName = mediaName;
            MediaTitle = mediaTitle;
            MediaExtension = mediaExtension;
            MediaDirectUrl = mediaDirectUrl;
            MediaPermanentUrl = mediaPermanentUrl;
        }
        /// <summary>
        /// Media Item sourced from Content Asset Item (for Xperience by Kentico Content Asset Items)
        /// </summary>
        /// <param name="contentItemMediaIdentifiers"></param>
        /// <param name="mediaName"></param>
        /// <param name="mediaTitle"></param>
        /// <param name="mediaExtension"></param>
        /// <param name="mediaDirectUrl"></param>
        /// <param name="mediaPermanentUrl"></param>
        public MediaItem(ContentItemMediaIdentifiers contentItemMediaIdentifiers, string mediaName, string mediaTitle, string mediaExtension, string mediaDirectUrl, string mediaPermanentUrl)
        {
            ContentItemMediaIdentifiers = contentItemMediaIdentifiers;
            MediaGUID = contentItemMediaIdentifiers.ContentItemGuid;
            MediaName = mediaName;
            MediaTitle = mediaTitle;
            MediaExtension = mediaExtension;
            MediaDirectUrl = mediaDirectUrl;
            MediaPermanentUrl = mediaPermanentUrl;
        }

        /// <summary>
        /// The Media GUID, this is either the FileGuid for Media Files, AttachmentGuid for Page Attachments, or the ContentItemGuid for Content Item Assets
        /// </summary>
        public Guid MediaGUID { get; init; }

        /// <summary>
        /// The Field Identity that identifies which field this media item was sourced from.  Only set for Content Item Assets
        /// </summary>
        public Maybe<ContentItemMediaIdentifiers> ContentItemMediaIdentifiers { get; init; }

        /// <summary>
        /// The Name of the Media Item (usually filename without extension)
        /// </summary>
        public string MediaName { get; init; }

        /// <summary>
        /// The title of the Media Item
        /// </summary>
        public string MediaTitle { get; init; }

        /// <summary>
        /// Optional Description
        /// </summary>
        public Maybe<string> MediaDescription { get; init; }

        /// <summary>
        /// The Media Extension, should include the period (ex ".png")
        /// </summary>
        public string MediaExtension { get; init; }

        [Obsolete("Please use either MediaPermanentUrl (ie /getmedia), or MediaDirectUrl (ie /MySite/media/Library/file.png)")]
        public string MediaUrl => MediaDirectUrl;

        /// <summary>
        /// The URL to the file directly, not recommended to use over the Permanent Url
        /// </summary>
        public string MediaDirectUrl { get; init; }

        /// <summary>
        /// The Permanent Url, recommended usage, this will be either the /getmedia /getattachment or /getcontentasset
        /// </summary>
        public string MediaPermanentUrl { get; init; }

        /// <summary>
        /// Optional Metadata, this is often set in the Version specific MediaMetadataProvider interfaces (IMediaFileMediaMetadataProvider.cs for KX13 and XbyK, IContentItemMediaMetadataProvider.cs additionally for XbyK)
        /// </summary>
        public Maybe<IMediaMetadata> MetaData { get; set; } = Maybe.None;
    }
}
