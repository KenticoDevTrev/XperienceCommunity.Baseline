namespace Admin.Installer
{
    public class StartingSiteInstallationOptions()
    {
        public bool CreateWebChannelIfNone { get; set; } = true;
        public bool AddHomePageType { get; set; } = true;
        public bool AddBasicPageType { get; set; } = true;
        public bool AddImageContentType { get; set; } = true;
        public bool AddFileContentType { get; set; } = true;
        public bool AddAudioContentType { get; set; } = false;
        public bool AddVideoContentType { get; set; } = false;
        public string ImageFormatsSupported { get; set; } = "jpg;jpeg;webp;gif;png;apng;bmp;ico;avif";
        public string VideoFormatsSupported { get; set; } = "mp4;webm;ogg;ogv;avi;wmv";
        public string AudioFormatsSupported { get; set; } = "wav;mp3;mp4;aac";
        public string NonMediaFileFormatsSupported { get; set; } = "txt;pdf;docx;pptx;xlsx;zip";
    }
}
