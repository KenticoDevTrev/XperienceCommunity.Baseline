using CMS.Core;
using CMS.Websites;
using Microsoft.Net.Http.Headers;

namespace Search.WebCrawler
{
    public class BaselineSearchLuceneWebCrawlerService
    {
        private readonly HttpClient _httpClient;
        private readonly IEventLogService _log;
        private readonly IWebPageUrlRetriever _webPageUrlRetriever;

        public BaselineSearchLuceneWebCrawlerService(
            HttpClient httpClient,
            IEventLogService log,
            IWebPageUrlRetriever webPageUrlRetriever,
            IAppSettingsService appSettingsService)
        {
            string baseUrl = appSettingsService["WebCrawlerBaseUrl"];

            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "SearchCrawler");
            _httpClient.BaseAddress = new Uri(baseUrl);

            _log = log;
            _webPageUrlRetriever = webPageUrlRetriever;
        }

        public async Task<string> CrawlWebPage(IWebPageFieldsSource page)
        {
            try {
                var url = await _webPageUrlRetriever.Retrieve(page);
                string path = url.RelativePath.TrimStart('~').TrimStart('/');

                return await CrawlPage(path);
            } catch (Exception ex) {
                _log.LogException(
                    nameof(BaselineSearchLuceneWebCrawlerService),
                    nameof(CrawlWebPage),
                    ex,
                    $"Tree Path: {page.SystemFields.WebPageItemTreePath}");
            }
            return string.Empty;
        }

        public async Task<string> CrawlPage(string url)
        {
            try {
                var response = await _httpClient.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            } catch (Exception ex) {
                _log.LogException(
                    nameof(BaselineSearchLuceneWebCrawlerService),
                    nameof(CrawlPage),
                    ex,
                    $"Url: {url}");
            }
            return string.Empty;
        }
    }
}
