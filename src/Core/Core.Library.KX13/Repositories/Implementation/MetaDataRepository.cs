﻿using CMS.Base;
using CMS.DocumentEngine.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kentico.Web.Mvc;

namespace Core.Repositories.Implementation
{
    [AutoDependencyInjection]
    public class MetaDataRepository : IMetaDataRepository
    {
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory;
        public readonly IPageRetriever _pageRetriever;
        public readonly IPageDataContextRetriever _pageDataContextRetriever;
        public readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlResolver _urlResolver;
        private readonly IMediaRepository _mediaRepository;
        private readonly ISiteService _siteService;
        private readonly IPageUrlRetriever _pageUrlRetriever;
        private readonly IUrlHelper _urlHelper;

        public MetaDataRepository(IPageRetriever pageRetriever,
            IPageDataContextRetriever pageDataContextRetriever,
            ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
            IHttpContextAccessor httpContextAccessor,
            IUrlResolver urlResolver,
            IMediaRepository mediaRepository,
            ISiteService siteService,
            IPageUrlRetriever pageUrlRetriever,
            IUrlHelper urlHelper)
        {
            _pageRetriever = pageRetriever;
            _pageDataContextRetriever = pageDataContextRetriever;
            _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
            _httpContextAccessor = httpContextAccessor;
            _urlResolver = urlResolver;
            _mediaRepository = mediaRepository;
            _siteService = siteService;
            _pageUrlRetriever = pageUrlRetriever;
            _urlHelper = urlHelper;
        }


        public async Task<Result<PageMetaData>> GetMetaDataAsync(int documentId, string? thumbnail = null)
        {
            var builder = _cacheDependencyBuilderFactory.Create()
                .Page(documentId);

            var page = await _pageRetriever.RetrieveAsync<TreeNode>(
                query => query
                    .WhereEquals(nameof(TreeNode.DocumentID), documentId)
                    .Columns(nameof(TreeNode.DocumentCustomData), nameof(TreeNode.DocumentPageTitle), nameof(TreeNode.DocumentPageDescription), nameof(TreeNode.DocumentPageKeyWords))
                    .TopN(1),
                cacheSettings => cacheSettings
                .Configure(builder, CacheMinuteTypes.Medium.ToDouble(), "GetMetaDataAsync", documentId)
            ) ;
            if (page.Any())
            {
                return await GetMetaDataInternalAsync(page.First(), thumbnail);
            }
            else
            {
                return Result.Failure<PageMetaData>("No page found by that documentID");
            }
        }

        public async Task<Result<PageMetaData>> GetMetaDataAsync(Guid documentGuid, string? thumbnail = null)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.Page(documentGuid);

            var page = await _pageRetriever.RetrieveAsync<TreeNode>(
                query => query
                    .WhereEquals(nameof(TreeNode.DocumentGUID), documentGuid)
                    .Columns(nameof(TreeNode.DocumentCustomData), nameof(TreeNode.DocumentPageTitle), nameof(TreeNode.DocumentPageDescription), nameof(TreeNode.DocumentPageKeyWords))
                    .TopN(1),
                cacheSettings => cacheSettings
                    .Configure(builder, CacheMinuteTypes.VeryLong.ToDouble(), "GetMetaDataAsync", documentGuid)
            );
            if (page.Any())
            {
                return await GetMetaDataInternalAsync(page.First(), thumbnail);
            }
            else
            {
                return Result.Failure<PageMetaData>("No page found by that documentGuid");
            }
        }

        public async Task<Result<PageMetaData>> GetMetaDataAsync(string? thumbnail = null)
        {
            var builder = _cacheDependencyBuilderFactory.Create();

            if (_pageDataContextRetriever.TryRetrieve<TreeNode>(out var currentPage))
            {
                builder.Page(currentPage.Page.DocumentID);
                return await GetMetaDataInternalAsync(currentPage.Page, thumbnail);
            }
            else
            {
                return Result.Failure<PageMetaData>("No page in the current context");
            }
        }

        private Task<PageMetaData> GetMetaDataInternalAsync(TreeNode node, string? thumbnail = null, string? thumbnailLarge = null)
        {
            string keywords = string.Empty;
            string description = string.Empty;
            string title = string.Empty;

            // Try to get these values first
            /*if (node is SomePageType menuPage)
            {
                if (menuPage.MenuItemTeaserImage.WithDefaultAsNone().TryGetValue(out var teaserGuid))
                {
                    var thumbResult = await _mediaRepository.GetAttachmentItemAsync(teaserGuid);
                    if (thumbResult.TryGetValue(out var thumb))
                    {
                        thumbnail = thumb.AttachmentUrl;
                    }
                }
                if (menuPage.Keywords.AsNullOrWhitespaceMaybe().TryGetValue(out var keywordsVal))
                {
                    keywords = keywordsVal;
                }
                if (menuPage.MenuItemSeoTitleOverride.AsNullOrWhitespaceMaybe().TryGetValue(out var seoTitleOverride))
                {
                    title = seoTitleOverride;
                }
            }
            */
            
            // Document custom data overrides, then site default
            if (thumbnail.AsNullOrWhitespaceMaybe().HasNoValue)
            {
                var customDataVal = node.DocumentCustomData.GetValue("MetaData_ThumbnailSmall");
                thumbnail = customDataVal != null && customDataVal is string thumbSmallVal ? thumbSmallVal : string.Empty;
            }
            if (thumbnailLarge.AsNullOrWhitespaceMaybe().HasNoValue)
            {
                var customDataVal = node.DocumentCustomData.GetValue("MetaData_ThumbnailLarge");
                thumbnailLarge = customDataVal != null && customDataVal is string thumbLargVal ? thumbLargVal : string.Empty;
            }
            if (keywords.AsNullOrWhitespaceMaybe().HasNoValue)
            {
                var customDataVal = node.DocumentCustomData.GetValue("MetaData_Keywords");
                keywords = customDataVal != null && customDataVal is string keywordVal ? keywordVal : node.DocumentPageKeyWords;
            }
            if (description.AsNullOrWhitespaceMaybe().HasNoValue)
            {
                var customDataVal = node.DocumentCustomData.GetValue("MetaData_Description");
                description = customDataVal != null && customDataVal is string descriptionVal ? descriptionVal : node.DocumentPageDescription;
            }
            if (title.AsNullOrWhitespaceMaybe().HasNoValue)
            {
                var customDataVal = node.DocumentCustomData.GetValue("MetaData_Title");
                title = customDataVal != null && customDataVal is string titleVal ? titleVal : node.DocumentPageTitle.AsNullOrWhitespaceMaybe().GetValueOrDefault(node.DocumentName);
            }

            PageMetaData metaData = new PageMetaData()
            {
                Title = title,
                Keywords = keywords.AsNullOrWhitespaceMaybe().GetValueOrDefault(string.Empty),
                Description = description.AsNullOrWhitespaceMaybe().GetValueOrDefault(string.Empty),
                Thumbnail = thumbnail.AsNullOrWhitespaceMaybe().TryGetValue(out var thumbUrl) ? _urlResolver.GetAbsoluteUrl(thumbUrl) : string.Empty,
                ThumbnailLarge = thumbnailLarge.AsNullOrWhitespaceMaybe().TryGetValue(out var thumbLargeUrl) ? _urlResolver.GetAbsoluteUrl(thumbLargeUrl) : string.Empty,
            };

            // Handle canonical url
            if (GetCanonicalUrl(node).TryGetValue(out var canonicalUrl))
            {
                metaData.CanonicalUrl = canonicalUrl;
            }
            else if (_urlHelper.Kentico().PageCanonicalUrl().AsNullOrWhitespaceMaybe().TryGetValue(out var canonicalUrlFromUrl))
            {
                // Try to get from url
                metaData.CanonicalUrl = canonicalUrlFromUrl;
            }

            return Task.FromResult(metaData);
        }

        private Result<string> GetCanonicalUrl(TreeNode page)
        {
            // Get original page URL
            if (page.IsLink)
            {
                var originalNodeData = DocumentNodeDataInfo.Provider.Get(page.OriginalNodeID);
                var canonicalUrl = _pageUrlRetriever.Retrieve(originalNodeData.NodeAliasPath, false).RelativePath;
                return Result.SuccessIf(!string.IsNullOrEmpty(canonicalUrl), canonicalUrl, "Could not retrieve relative path");
            }
            else
            {
                var canonicalUrl = _pageUrlRetriever.Retrieve(page, false).RelativePath;
                return Result.SuccessIf(!string.IsNullOrEmpty(canonicalUrl), canonicalUrl, "Could not retrieve relative path");
            }
        }

    }
}
