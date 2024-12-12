namespace Core.Models
{
    public record BaselineCoreInstallerOptions(
        bool AddMemberFields = true, 
        bool AddHomePageType = true, 
        bool AddBasicPageType = true,
        bool AddMediaPageTypes = true,
        string ImageFormatsSupported = "jpg;jpeg;webp;gif;svg;png;apng;bmp;ico",
        string VideoFormatsSupported = "mp4;webm;ogg;ogv;avi;wmv",
        string AudioFormatsSupported = "wav;mp3;mp4;aac",
        string NonMediaFileFormatsSupported = "txt;pdf;docx;pptx;xlsx;zip"
        );
}
