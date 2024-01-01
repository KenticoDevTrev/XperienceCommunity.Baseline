namespace Navigation.Models
{
    public record Breadcrumb
    {
        public Breadcrumb(string linkText, string linkUrl)
        {
            LinkText = linkText;
            LinkUrl = linkUrl;
        }
        public Breadcrumb(string linkText, string linkUrl, bool isCurrentPage)
        {
            LinkText = linkText;
            LinkUrl = linkUrl;
            IsCurrentPage = isCurrentPage;
        }

        public string LinkText { get; init; }
        public string LinkUrl { get; init; }
        public bool IsCurrentPage { get; init; } = false;

    }
}