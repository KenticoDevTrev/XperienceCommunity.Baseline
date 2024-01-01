namespace Navigation.Models
{
    public record SitemapNode
    {
        public SitemapNode(string url)
        {
            Url = url;
        }

        public string Url { get; init; }
        public Maybe<DateTime> LastModificationDate { get; init; }
        public Maybe<ChangeFrequency> ChangeFrequency { get; init; }
        public Maybe<decimal> Priority { get; init; }

        /// <summary>
        /// Other Language ISO code name(ex: es-ES) to the Url, do not include the main Url in this.
        /// </summary>
        public Maybe<Dictionary<string, string>> OtherLanguageCodeToUrl { get; init; }

        /// <summary>
        /// Helper to get an array of Sitemap Nodes to a valid sitemap xml
        /// </summary>
        /// <param name="sitemapNodes"></param>
        /// <returns></returns>
        public static string GetSitemap(IEnumerable<SitemapNode> sitemapNodes)
        {
            return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"https://sitemaps.org/schemas/sitemap/0.9\" xmlns:xhtml=\"http://www.w3.org/1999/xhtml\">\r\n{string.Join("\n", sitemapNodes.Select(x => x.SitemapNodeToXmlString()))}</urlset>";
        }

        public string SitemapNodeToXmlString()
        {
            string changeFreq = string.Empty;
            if (ChangeFrequency.HasValue)
            {
                switch (ChangeFrequency.Value)
                {
                    case Models.ChangeFrequency.Always:
                        changeFreq = "always";
                        break;
                    case Models.ChangeFrequency.Daily:
                        changeFreq = "daily";
                        break;
                    case Models.ChangeFrequency.Hourly:
                        changeFreq = "hourly";
                        break;
                    case Models.ChangeFrequency.Monthly:
                        changeFreq = "monthly";
                        break;
                    case Models.ChangeFrequency.Never:
                        changeFreq = "never";
                        break;
                    case Models.ChangeFrequency.Weekly:
                        changeFreq = "weekly";
                        break;
                    case Models.ChangeFrequency.Yearly:
                        changeFreq = "yearly";
                        break;
                }
            }

            return string.Format("<url>{0}{1}{2}{3}{4}</url>",
                $"<loc>{Url}</loc>",
            LastModificationDate.HasValue ? $"<lastmod>{LastModificationDate.Value:yyyy-MM-ddTHH:mm:sszzz}</lastmod>" : "",
            !string.IsNullOrWhiteSpace(changeFreq) ? $"<changefreq>{changeFreq}</changefreq>" : "",
            Priority.HasValue ? $"<priority>{Priority.Value}</priority>" : "",
            OtherLanguageCodeToUrl.TryGetValue(out var otherLanguageCodeToUrl) ? string.Join("", otherLanguageCodeToUrl.Keys.Select(langCode => $"<xhtml:link rel=\"alternate\" hreflang=\"{langCode}\" href=\"{otherLanguageCodeToUrl[langCode]}\"/>")) : string.Empty
            );
        }
    }



    //
    // Summary:
    //     Change frequency of the linked document
    public enum ChangeFrequency
    {
        //
        // Summary:
        //     The value "always" should be used to describe documents that change each time
        //     they are accessed.
        Always = 0,
        //
        // Summary:
        //     Hourly change
        Hourly = 1,
        //
        // Summary:
        //     Daily change
        Daily = 2,
        //
        // Summary:
        //     Weekly change
        Weekly = 3,
        //
        // Summary:
        //     Monthly change
        Monthly = 4,
        //
        // Summary:
        //     Yearly change
        Yearly = 5,
        //
        // Summary:
        //     The value "never" should be used to describe archived URLs.
        Never = 6
    }
}
