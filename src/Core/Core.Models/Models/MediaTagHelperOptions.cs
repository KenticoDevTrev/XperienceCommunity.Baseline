namespace Core.Models
{
    public class MediaTagHelperOptions
    {
        public MediaTagHelperOptions() { }

        /// <summary>
        /// Setting this to false will disable the ImageMediaMetadataTagHelper logic (since this by default runs on ALL images)
        /// </summary>
        public bool UseMediaTagHelper { get; set; } = true;
        
        /// <summary>
        /// What extensions should allow dynamic resizing, not recommended on gif in case animated.
        /// </summary>
        public string[] SupportedDynamicResizingExtensions { get; set; } = ["png", "jpg", "jpeg", "webp"];

        /// <summary>
        /// If you want the image to be 1000 pixels wide, on screens with a higher resolution (say 125% or 150%), the image may appear fuzzy.
        /// This factor will render a larger pixel image for the actual size that will be rendered.  1.25m => 1250 pixel rendering for a 1000 pixel image
        /// </summary>
        public decimal ImageResolutionScaleFactor { get; set; } = 1.25m;
    }
}
