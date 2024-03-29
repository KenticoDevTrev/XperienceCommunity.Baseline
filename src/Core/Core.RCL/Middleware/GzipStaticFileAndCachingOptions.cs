﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Core.Middleware
{
    public static class GZipStaticFileOptionsMiddleware
    {
        /// <summary>
        /// Enables proper content-type setting for *.js.gz, *.css.gz, and *.map.gz files
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection UseGzipAndCacheControlFileHandling(this IServiceCollection services)
        {
            return services.ConfigureOptions<GzipStaticFileAndCachingOptions>();
        }
    }

    public class GzipStaticFileAndCachingOptions : IPostConfigureOptions<StaticFileOptions>
    {
        public void PostConfigure(string? name, StaticFileOptions options)
        {
            options.OnPrepareResponse = context =>
            {
                var headers = context.Context.Response.Headers;
                var contentType = headers.ContentType;
                if (contentType == "application/x-gzip")
                {
                    if (context.File.Name.EndsWith("js.gz"))
                    {
                        contentType = "application/javascript";
                        headers.Append("Content-Encoding", "gzip");
                        headers.ContentType = contentType;
                    }
                    else if (context.File.Name.EndsWith("css.gz"))
                    {
                        contentType = "text/css";
                        headers.Append("Content-Encoding", "gzip");
                        headers.ContentType = contentType;
                    }
                    else if (context.File.Name.EndsWith(".map.gz"))
                    {
                        contentType = "application/json";
                        headers.Append("Content-Encoding", "gzip");
                        headers.ContentType = contentType;
                    }
                }

                // If it has the v= key, then the build # is appended so safe to cache for a year, also images can be cached fully
                if (((context.Context.Request.Query?.ContainsKey("v") ?? false) || ((context.File?.PhysicalPath?.IndexOf($"wwwroot{Path.DirectorySeparatorChar}images") ?? -1) > -1) || ((context.File?.PhysicalPath?.IndexOf($"{Path.DirectorySeparatorChar}fonts{Path.DirectorySeparatorChar}") ?? -1) > -1)))
                {
                    const int durationInSeconds = 60 * 60 * 24 * 365; // 1 year
                    context.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
                }
                else
                {
                    // Otherwise just 24 hours
                    const int durationInSeconds = 60 * 60 * 24; // 24 hour
                    context.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=" + durationInSeconds;
                }
            };
        }
    }
}
