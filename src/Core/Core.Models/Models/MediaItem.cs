namespace Core.Models
{
    public record MediaItem
    {
        public MediaItem(Guid mediaGUID, string mediaName, string mediaTitle, string mediaExtension, string mediaDirectUrl, string mediaPermanentUrl)
        {
            MediaGUID = mediaGUID;
            MediaName = mediaName;
            MediaTitle = mediaTitle;
            MediaExtension = mediaExtension;
            MediaDirectUrl = mediaDirectUrl;
            MediaPermanentUrl = mediaPermanentUrl;
        }

        public Guid MediaGUID { get; init; }
        public string MediaName { get; init; }
        public string MediaTitle { get; init; }
        public Maybe<string> MediaDescription { get; init; }
        public string MediaExtension { get; init; }

        [Obsolete("Please use either MediaPermanentUrl (ie /getmedia), or MediaDirectUrl (ie /MySite/media/Library/file.png)")]
        public string MediaUrl => MediaDirectUrl;

        public string MediaDirectUrl { get; init; }
        public string MediaPermanentUrl { get; init; }

    }
}
