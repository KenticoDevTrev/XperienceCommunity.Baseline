using System.Net;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Middleware
{
    public static class AdminRedirectBuilderExtension
    {
        public static IServiceCollection UseAdminRedirect(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSingleton<IStartupFilter>(new AdminRedirectStartupFilter(configuration));
        }
    }

    /// <summary>
    /// Adds a rewriter with <see cref="AdminRedirectRule"/> into the pipeline.
    /// </summary>
    public class AdminRedirectStartupFilter(IConfiguration _configuration) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                // Ensures redirect to the administration instance based on URL defined in settings
                builder.UseRewriter(new RewriteOptions()
                    .Add(new AdminRedirectRule(_configuration)));

                next(builder);
            };
        }


        /// <summary>
        /// Redirects a request to "/admin" to the administration site specified in <c>DancingGoatAdminUrl</c> app setting.
        /// </summary>
        private class AdminRedirectRule(IConfiguration configuration) : IRule
        {
            private readonly string _adminUrl = configuration["CustomAdminUrl"] ?? String.Empty;

            public void ApplyRule(RewriteContext context)
            {
                if (!_adminUrl.AsNullOrWhitespaceMaybe().TryGetValue(out var adminUrlVal))
                {
                    return;
                }

                var request = context.HttpContext.Request;

                if ((request.Path.Value ?? string.Empty).TrimEnd('/').Equals("/admin", StringComparison.OrdinalIgnoreCase))
                {
                    var response = context.HttpContext.Response;

                    response.StatusCode = (int)HttpStatusCode.MovedPermanently;
                    response.Headers[HeaderNames.Location] = adminUrlVal;
                    context.Result = RuleResult.EndResponse;
                }
            }
        }
    }
}
