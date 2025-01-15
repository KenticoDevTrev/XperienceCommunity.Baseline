using CMS.ContentEngine;
using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Core.Enums;
using Core.Models;
using Core.Repositories;
using Core.Repositories.Implementation;
using Core.Services;
using CSharpFunctionalExtensions;
using Generic;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using MVCCaching;
using XperienceCommunity.MVCCaching.Implementations;
using XperienceCommunity.QueryExtensions.ContentItems;

namespace Core.Middleware
{
    public class BaseRedirectMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        // IMessageWriter is injected into InvokeAsync
        public async Task InvokeAsync(HttpContext httpContext, 
                                      IPageContextRepository pageContextRepository, 
                                      IUrlResolver urlResolver, 
                                      IWebsiteChannelContext websiteChannelContext, 
                                      ISiteRepository siteRepository, 
                                      IContentQueryExecutor contentQueryExecutor,
                                      ICacheRepositoryContext cacheRepositoryContext,
                                      IProgressiveCache progressiveCache,
                                      ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
                                      IEventLogService eventLogService)
        {
            try {
                if ((await pageContextRepository.GetCurrentPageAsync<IBaseRedirect>()).TryGetValue(out var redirectPage)
                    && redirectPage.Data.TryGetValue(out var baseRedirect)
                    && !baseRedirect.PageRedirectionType.Equals("None", StringComparison.OrdinalIgnoreCase)
                    ) {
                    // redirect
                    switch (baseRedirect.PageRedirectionType.ToLowerInvariant()) {
                        case "internal":
                            if (baseRedirect.PageInternalRedirectPage.FirstOrMaybe().TryGetValue(out var pageInternalRedirectWebpageGuid)
                                && (await pageContextRepository.GetPageAsync(pageInternalRedirectWebpageGuid.WebPageGuid.ToTreeIdentity())).TryGetValue(out var foundPage)) {
                                if (websiteChannelContext.WebsiteChannelID.Equals(foundPage.ChannelID)) {
                                    httpContext.Response.Redirect(urlResolver.ResolveUrl(foundPage.RelativeUrl), permanent: baseRedirect.PageUsePermanentRedirects);
                                    await httpContext.Response.CompleteAsync();
                                    return;
                                }

                                // Use absolute URL
                                httpContext.Response.Redirect(urlResolver.ResolveUrl(foundPage.AbsoluteUrl), permanent: baseRedirect.PageUsePermanentRedirects);
                                await httpContext.Response.CompleteAsync();
                                return;
                            }
                            break;
                        case "external":
                            if (!string.IsNullOrWhiteSpace(baseRedirect.PageExternalRedirectURL)) {
                                httpContext.Response.Redirect(urlResolver.ResolveUrl(baseRedirect.PageExternalRedirectURL), permanent: baseRedirect.PageUsePermanentRedirects);
                                await httpContext.Response.CompleteAsync();
                                return;
                            }
                            break;
                        case "firstchild":
                            // get first page under the path
                            var builder = cacheDependencyBuilderFactory.Create(false).WebPagePath(redirectPage.Path, PathTypeEnum.Children);

                            var results = await progressiveCache.LoadAsync(async cs => {
                                if (cs.Cached) {
                                    cs.CacheDependency = builder.GetCMSCacheDependency();
                                }
                                var childQuery = new ContentItemQueryBuilder();
                                if (!string.IsNullOrWhiteSpace(baseRedirect.PageFirstChildClassName)) {
                                    childQuery.ForContentType(baseRedirect.PageFirstChildClassName, query => query
                                        .ForWebsite(siteRepository.ChannelNameById(redirectPage.ChannelID))
                                        .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemParentID), redirectPage.PageID))
                                        .OrderByWebpageItemOrder()
                                        .TopN(1)
                                        )
                                        .InLanguage(redirectPage.Culture);
                                } else {
                                    childQuery.ForContentTypes(query => query
                                        .ForWebsite(true)
                                        ).Parameters(parameters => parameters
                                            .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemParentID), redirectPage.PageID))
                                            .OrderBy(nameof(WebPageFields.WebPageItemOrder))
                                            .TopN(1)
                                        )
                                        .InLanguage(redirectPage.Culture);
                                }

                                return (await contentQueryExecutor.GetMappedWebPageResult<IWebPageFieldsSource>(childQuery, new ContentQueryExecutionOptions().WithPreviewModeContext(cacheRepositoryContext))).FirstOrMaybe();
                            }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "FindFirstChildForRedirect", baseRedirect.PageFirstChildClassName, redirectPage.PageID, redirectPage.ChannelID, redirectPage.Culture));

                            if (results.TryGetValue(out var foundChildPage)
                                // convert to PageIdentity so we have both relative and absolute
                                && (await pageContextRepository.GetPageAsync(foundChildPage.SystemFields.WebPageItemID.ToTreeIdentity())).TryGetValue(out var foundPageIdentity)
                                ) {
                                if (websiteChannelContext.WebsiteChannelID.Equals(foundPageIdentity.ChannelID)) {
                                    httpContext.Response.Redirect(urlResolver.ResolveUrl(foundPageIdentity.RelativeUrl), permanent: baseRedirect.PageUsePermanentRedirects);
                                    await httpContext.Response.CompleteAsync();
                                    return;
                                }

                                // Use absolute URL
                                httpContext.Response.Redirect(urlResolver.ResolveUrl(foundPageIdentity.AbsoluteUrl), permanent: baseRedirect.PageUsePermanentRedirects);
                                await httpContext.Response.CompleteAsync();
                                return;
                            }

                            break;
                    }
                }
            } catch(Exception ex) {
                eventLogService.LogException("BaseRedirectMiddleware", "ERROR", ex);
            }
            await _next(httpContext);
        }
    }
}
