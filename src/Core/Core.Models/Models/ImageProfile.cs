namespace Core.Models
{
    public record ImageProfile
    {
        /// <summary>
        /// This will define the MAX image render size
        /// </summary>
        /// <param name="imageRenderWidth"></param>
        public ImageProfile(int imageRenderWidth)
        {
            ImageRenderWidth = imageRenderWidth;
        }

        /// <summary>
        /// These define smaller image rendering sizes.  You should set your max screen widths typically to 1 pixel below break points.
        /// </summary>
        /// <param name="imageRenderWidth"></param>
        /// <param name="maxScreenWidth"></param>
        public ImageProfile(int imageRenderWidth, int maxScreenWidth)
        {
            ImageRenderWidth = imageRenderWidth;
            MaxScreenWidth = maxScreenWidth;
        }

        public int ImageRenderWidth { get; init; }
        public Maybe<int> MaxScreenWidth { get; init; } = Maybe.None;
    }
}
