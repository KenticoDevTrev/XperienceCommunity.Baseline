﻿using Core.Services;
using Microsoft.AspNetCore.Http;
using MVCCaching;

namespace Core.Services.Implementations
{
    [AutoDependencyInjection]
    public class UrlResolver : IUrlResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetAbsoluteUrl(string relativeUrl)
        {
            return GetUri(ResolveUrl(relativeUrl)).AbsoluteUri;
        }

        public string ResolveUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
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
