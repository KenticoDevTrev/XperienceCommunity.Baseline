namespace Core.Models
{
    public class MediaFileOptions
    {
        /// <summary>
        /// If set to true, will retrieve, generate, and cache all Media_File derived media items.  
        /// 
        /// For performance reasons, this will not retrieve the DirectUrl (which requires additional database operations per asset), but simply use the same /getmedia/{FileGuid}/{FileName} format.
        /// </summary>
        public bool UseCachedMediaFiles { get; set; } = false;
    }
}
