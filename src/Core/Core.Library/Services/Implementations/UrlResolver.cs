using Microsoft.AspNetCore.Http;

namespace Core.Services.Implementations
{
    public class UrlResolver(IHttpContextAccessor _httpContextAccessor) : IUrlResolver
    {
        public string GetAbsoluteUrl(string relativeUrl)
        {
            if(string.IsNullOrWhiteSpace(relativeUrl))
            {
                relativeUrl = string.Empty;
            }
            if(relativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) || relativeUrl.StartsWith("//"))
            {
                return relativeUrl;
            }
            return GetUri(ResolveUrl(relativeUrl)).AbsoluteUri;
        }

        public string ResolveUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }
            if (url.StartsWith("~/"))
            {
                url = url.Replace("~/", "/");
            }
            return url;
        }
        private Uri GetUri(string relativeUrl)
        {
            relativeUrl = !string.IsNullOrWhiteSpace(relativeUrl) ? relativeUrl : "/";
            if (_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext))
            {
                var request = httpContext.Request;
                if(request.Host.Value.Contains(':') && int.TryParse(request.Host.Value.Split(':')[1], out var port))
                {
                    return new UriBuilder
                    {
                        Scheme = request.Scheme,
                        Host = request.Host.Value.Split(':')[0],
                        Port = port,
                        Path = relativeUrl.Split('?')[0],
                        Query = (relativeUrl.Contains('?') ? relativeUrl.Split('?')[1] : "")
                    }.Uri;
                } else
                {
                    return new UriBuilder
                    {
                        Scheme = request.Scheme,
                        Host = request.Host.Value.Split(':')[0],
                        Path = relativeUrl.Split('?')[0],
                        Query = (relativeUrl.Contains('?') ? relativeUrl.Split('?')[1] : "")
                    }.Uri;
                }
                
            }
            else
            {
                return new Uri(relativeUrl);
            }
        }
    }
}
