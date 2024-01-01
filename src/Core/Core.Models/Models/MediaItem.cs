namespace Core.Models
{
    public record MediaItem
    {
        public MediaItem(Guid mediaGUID, string mediaName, string mediaTitle, string mediaExtension, string mediaUrl, string mediaPermanentUrl)
        {
            MediaGUID = mediaGUID;
            MediaName = mediaName;
            MediaTitle = mediaTitle;
            MediaExtension = mediaExtension;
            MediaUrl = mediaUrl;
            MediaPermanentUrl = mediaPermanentUrl;
        }

        public Guid MediaGUID { get; init; }
        public string MediaName { get; init; }
        public string MediaTitle { get; init; }
        public Maybe<string> MediaDescription { get; init; }
        public string MediaExtension { get; init; }
        public string MediaUrl { get; init; }
        public string MediaPermanentUrl { get; init; }

    }
}
