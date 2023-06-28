namespace Navigation.Models
{
    public class SitemapNode
    {
        public SitemapNode(string url)
        {
            Url = url;
        }

        public string Url { get; set; }
        public Maybe<DateTime> LastModificationDate { get; set; }
        public Maybe<ChangeFrequency> ChangeFrequency { get; set; }
        public Maybe<decimal> Priority { get; set; }

        /// <summary>
        /// Other Language ISO code name(ex: es-ES) to the Url, do not include the main Url in this.
        /// </summary>
        public Maybe<Dictionary<string, string>> OtherLanguageCodeToUrl { get; set; }

        /// <summary>
        /// Helper to get an array of Sitemap Nodes to a valid sitemap xml
        /// </summary>
        /// <param name="sitemapNodes"></param>
        /// <returns></returns>
        public static string GetSitemap(IEnumerable<SitemapNode> sitemapNodes)
        {
            return $"<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\" xmlns:xhtml=\"http://www.w3.org/1999/xhtml\">{string.Join("\n", sitemapNodes.Select(x => x.SitemapNodeToXmlString()))}</urlset>";
        }

        public string SitemapNodeToXmlString()
        {
            string changeFreq = string.Empty;
            if (this.ChangeFrequency.HasValue)
            {
                switch (this.ChangeFrequency.Value)
                {
                    case Navigation.Models.ChangeFrequency.Always:
                        changeFreq = "always";
                        break;
                    case Navigation.Models.ChangeFrequency.Daily:
                        changeFreq = "daily";
                        break;
                    case Navigation.Models.ChangeFrequency.Hourly:
                        changeFreq = "hourly";
                        break;
                    case Navigation.Models.ChangeFrequency.Monthly:
                        changeFreq = "monthly";
                        break;
                    case Navigation.Models.ChangeFrequency.Never:
                        changeFreq = "never";
                        break;
                    case Navigation.Models.ChangeFrequency.Weekly:
                        changeFreq = "weekly";
                        break;
                    case Navigation.Models.ChangeFrequency.Yearly:
                        changeFreq = "yearly";
                        break;
                }
            }

            return string.Format("<url>{0}{1}{2}{3}{4}</url>",
                $"<loc>{Url}</loc>",
            LastModificationDate.HasValue ? $"<lastmod>{LastModificationDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzz")}</lastmod>" : "",
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
