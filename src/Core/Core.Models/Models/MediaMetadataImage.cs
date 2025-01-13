namespace Core.Models
{
    public record MediaMetadataImage : IMediaMetadata
    {
        public MediaMetadataImage(int width, int height, FocalPercents? focalPercet = null, IEnumerable<ImageProfile>? imageProfiles = null)
        {
            Width = width;
            Height = height;
            FocalPercent = focalPercet.AsMaybe();
            ImageProfiles = imageProfiles ?? [];
        }

        /// <summary>
        /// The Width of the Image in pixels
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// The Height of the Image in pixels
        /// </summary>
        public int Height { get; init; }

        /// <summary>
        /// The Focal percentage
        /// </summary>
        public Maybe<FocalPercents> FocalPercent { get; init; }

        /// <summary>
        /// Can set Image Profiles which will allow for different profiles by different screen sizes
        /// </summary>
        public IEnumerable<ImageProfile> ImageProfiles { get; init; } = [];
    }

    /// <summary>
    /// The Focal point of the image, in percent (50 = 50%)
    /// </summary>
    /// <param name="XPercent">The X Focal percentage.  0 = Left, 50 = center, 100% = right</param>
    /// <param name="YPercent">The Y Focal percentage. 0 = Top, 50 = middle, 100 = bottom </param>
    public record FocalPercents(decimal XPercent, decimal YPercent);
}
