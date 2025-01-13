using CMS.DataEngine;
using CMS.MediaLibrary;
using DocumentFormat.OpenXml.Office2010.Word;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Data;

namespace Core.Repositories.Implementation
{
    [AutoDependencyInjection]
    public class MediaRepository(
        ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory,
        ISiteRepository _siteRepository,
        IPageRetriever _pageRetriever,
        IProgressiveCache _progressiveCache,
        IAttachmentInfoProvider _attachmentInfoProvider,
        IMediaFileInfoProvider _mediaFileInfoProvider,
        IPageDataContextRetriever _pageDataContextRetriever,
        IMediaFileUrlRetriever _mediaFileUrlRetriever,
        IPageAttachmentUrlRetriever _pageAttachmentUrlRetriever,
        IMediaFileMediaMetadataProvider _mediaFileMediaMetadataProvider) : IMediaRepository
    {

        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAsset(attachmentGuid.ToObjectIdentity(), \"EventualContentAssetFieldName\"). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        public Task<Result<MediaItem>> GetAttachmentItemAsync(Guid attachmentGuid) => GetAttachmentInternal(attachmentGuid);

        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAsset(attachmentGuid.ToObjectIdentity(), \"EventualContentAssetFieldName\"). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        public async Task<Result<MediaItem>> GetPageAttachmentAsync(Guid attachmentGuid, int? documentID = null)
        {
            // Will keep this logic here, but in the end the lookup by attachmentGuid alone is sufficient.
            var builder = _cacheDependencyBuilderFactory.Create();

            if (_pageDataContextRetriever.TryRetrieve<TreeNode>(out var currentPage) && (!documentID.HasValue || currentPage.Page.DocumentID == documentID.Value)) {
                var attachment = currentPage.Page.Attachments?.Where(x => x.AttachmentGUID.Equals(attachmentGuid)).FirstOrDefault();
                if (attachment != null) {
                    builder.Attachment(attachmentGuid);
                    return AttachmentToMediaItem(attachment);
                }
            }

            // Look up normally
            return await GetAttachmentItemAsync(attachmentGuid);
        }

        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemLanguageMetadataAssets(int? contentItemMetadataId). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        public Task<IEnumerable<MediaItem>> GetPageAttachmentsAsync(int? documentID = null) => GetPageAttachmentsInternal(documentID ?? (_pageDataContextRetriever.TryRetrieve<TreeNode>(out var currentPage) ? currentPage.Page.DocumentID : 0));

        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAssets(attachmentGuids.Select(x => x.ToObjectIdentity())). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        public Task<IEnumerable<MediaItem>> GetAttachmentsAsync(IEnumerable<Guid> attachmentGuids) => GetAttachmentsInternal(attachmentGuids);

        public Task<IEnumerable<MediaItem>> GetContentItemLanguageMetadataAssets(int? contentItemMetadataId) => GetPageAttachmentsInternal(contentItemMetadataId ?? (_pageDataContextRetriever.TryRetrieve<TreeNode>(out var currentPage) ? currentPage.Page.DocumentID : 0));

        public Task<Result<MediaItem>> GetContentItemLanguageMetadataAsset(int? contentItemMetadataId, Guid assetFieldGuid) => GetContentItemLanguageMetadataAsset(contentItemMetadataId, string.Empty);

        public async Task<Result<MediaItem>> GetContentItemLanguageMetadataAsset(int? contentItemMetadataId, string assetFieldName) => (await GetPageAttachmentsInternal(contentItemMetadataId ?? (_pageDataContextRetriever.TryRetrieve<TreeNode>(out var currentPage) ? currentPage.Page.DocumentID : 0))).FirstOrMaybe().TryGetValue(out var attachment) ? attachment : Result.Failure<MediaItem>("Could not find any attachments on that document");

        public Task<Result<MediaItem>> GetContentItemAsset(ContentIdentity contentItem, Guid assetFieldGuid, string? language = null) => GetContentItemAsset(contentItem, string.Empty);

        public async Task<Result<MediaItem>> GetContentItemAsset(ContentIdentity contentItem, string assetFieldName, string? language = null) => contentItem.ContentGuid.TryGetValue(out var attachmentGuid) ? await GetAttachmentInternal(attachmentGuid) : Result.Failure<MediaItem>("For KX13, the ContentItem Identity must be the Attachment Guid");

        public Task<IEnumerable<MediaItem>> GetContentItemAssets(IEnumerable<ContentIdentity> contentItems, string? language = null) => GetAttachmentsInternal(contentItems.Where(x => x.ContentGuid.HasValue).Select(x => x.ContentGuid.Value));

        public Task<IEnumerable<MediaItem>> GetContentItemAssets(IEnumerable<ContentIdentity> contentItems, Guid assetFieldGuid, string? language = null) => GetContentItemAssets(contentItems);

        public Task<IEnumerable<MediaItem>> GetContentItemAssets(IEnumerable<ContentIdentity> contentItems, string assetFieldName, string? language = null) => GetContentItemAssets(contentItems);

        public Task<IEnumerable<MediaItem>> GetContentItemAssets(ContentIdentity contentItem, string? language = null) => GetContentItemAssets([contentItem]);

        public async Task<Result<MediaItem>> GetMediaItemAsync(Guid fileGuid)
        {
            var builder = _cacheDependencyBuilderFactory.Create()
                .Object(MediaFileInfo.OBJECT_TYPE, fileGuid);

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var mediaFile = await _mediaFileInfoProvider.GetAsync(fileGuid, _siteRepository.GetChannelID().GetValueOrDefault(0));
                if(mediaFile.AsMaybe().TryGetValue(out var mediaFileVal)) {
                    var mediaItem = MediaFileInfoToMediaItem(mediaFileVal);
                    if((await _mediaFileMediaMetadataProvider.GetMediaMetadata(mediaFileVal, mediaItem)).TryGetValue(out var metaData)) {
                        mediaItem = mediaItem with { MetaData = metaData.AsMaybe() };
                    }
                    return mediaItem;
                }
                return Result.Failure<MediaItem>("Could not find Media Item");
            }, new CacheSettings(CacheMinuteTypes.Short.ToDouble(), "GetMediaItemAsync", fileGuid));
        }

        public async Task<IEnumerable<MediaItem>> GetMediaItemsByLibraryAsync(string libraryName)
        {
            var builder = _cacheDependencyBuilderFactory.Create()
                .Object(MediaLibraryInfo.OBJECT_TYPE, libraryName);

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                // Gets an instance of the 'SampleMediaLibrary' media library for the current site
                var mediaFiles = await _mediaFileInfoProvider.Get()
                        .Source(x => x.Join<MediaLibraryInfo>(nameof(MediaFileInfo.FileLibraryID), nameof(MediaLibraryInfo.LibraryID)))
                        .WhereEquals(nameof(MediaLibraryInfo.LibraryName), libraryName)
                        .GetEnumerableTypedResultAsync();

                var mediaItems = new List<MediaItem>();
                foreach(var mediaFile in mediaFiles) {
                    var mediaItem = MediaFileInfoToMediaItem(mediaFile);
                    if ((await _mediaFileMediaMetadataProvider.GetMediaMetadata(mediaFile, mediaItem)).TryGetValue(out var metaData)) {
                        mediaItem = mediaItem with { MetaData = metaData.AsMaybe() };
                    }
                    mediaItems.Add(mediaItem);
                }
                return mediaItems;
            }, new CacheSettings(15, "GetMediaItemsByLibraryAsync", libraryName));
        }

        public async Task<IEnumerable<MediaItem>> GetMediaItemsByPathAsync(string path, string? libraryName = null)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            if (libraryName.AsMaybe().TryGetValue(out var libraryNameVal)) {
                builder.Object(MediaLibraryInfo.OBJECT_TYPE, libraryNameVal);
            }

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                // Gets an instance of the 'SampleMediaLibrary' media library for the current site
                var mediaFiles = await _mediaFileInfoProvider.Get()
                        .Source(x => x.Join<MediaLibraryInfo>(nameof(MediaFileInfo.FileLibraryID), nameof(MediaLibraryInfo.LibraryID)))
                        .If(libraryName.AsMaybe().TryGetValue(out var libraryNameVal), query => query.WhereEquals(nameof(MediaLibraryInfo.LibraryName), libraryNameVal))
                        .WhereLike(nameof(MediaFileInfo.FilePath), $"%{SqlHelper.EscapeLikeText(path)}%")
                        .GetEnumerableTypedResultAsync();

                var mediaItems = new List<MediaItem>();
                foreach (var mediaFile in mediaFiles) {
                    var mediaItem = MediaFileInfoToMediaItem(mediaFile);
                    if ((await _mediaFileMediaMetadataProvider.GetMediaMetadata(mediaFile, mediaItem)).TryGetValue(out var metaData)) {
                        mediaItem = mediaItem with { MetaData = metaData.AsMaybe() };
                    }
                    mediaItems.Add(mediaItem);
                }
                return mediaItems;
            }, new CacheSettings(15, "GetMediaItemsByPathAsync", path, libraryName));
        }

        public async Task<Result<string>> GetMediaAttachmentSiteNameAsync(Guid mediaOrAttachmentGuid)
        {
            // Get dictionary of Guid to SiteID
            var guidToSiteID = await _progressiveCache.LoadAsync(async cs => {
                var data = await XperienceCommunityConnectionHelper.ExecuteQueryAsync(
                    @"select FileGuid as [Guid], FileSiteID as [SiteId] from Media_File
                        union all
                    Select AttachmentGUID as [Guid], AttachmentSiteID as [SiteId] from CMS_Attachment", [], QueryTypeEnum.SQLQuery);

                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency(
                    [
                        $"{MediaFileInfo.OBJECT_TYPE}|all",
                        $"{AttachmentInfo.OBJECT_TYPE}|all",
                    ]);
                }

                return data.Tables[0].Rows.Cast<DataRow>()
                    .Select(x => new Tuple<Guid, int>(ValidationHelper.GetGuid(x["Guid"], Guid.Empty), ValidationHelper.GetInteger(x["SiteID"], 0)))
                    .GroupBy(x => x.Item1)
                    .ToDictionary(key => key.Key, value => value.First().Item2);

            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "GetAttachmentMediaGuidToSiteID"));

            if (guidToSiteID.GetValueOrMaybe(mediaOrAttachmentGuid).TryGetValue(out var siteID)) {
                string siteName = _siteRepository.ChannelNameById(siteID);
                return siteName;
            }
            return Result.Failure<string>("Could not find attachment or media item by that Guid");

        }

        private async Task<Result<MediaItem>> GetAttachmentInternal(Guid attachmentGuid)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            builder.Attachment(attachmentGuid);
            var result = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var attachment = await _attachmentInfoProvider.Get()
                .WhereEquals(nameof(AttachmentInfo.AttachmentGUID), attachmentGuid)
                .TopN(1)
                .BinaryData(false)
                .GetEnumerableTypedResultAsync();
                return attachment.FirstOrMaybe();

            }, new CacheSettings(15, "GetAttachmentItemAsnc", attachmentGuid));

            return result.HasValue ? AttachmentToMediaItem(result.Value) : Result.Failure<MediaItem>("Could not find attachment");
        }

        private async Task<IEnumerable<MediaItem>> GetPageAttachmentsInternal(int documentID)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            if (_pageDataContextRetriever.TryRetrieve<TreeNode>(out var currentPage) && (currentPage.Page.DocumentID == documentID)) {
                currentPage.Page.Attachments.ToList().ForEach(x => builder.Attachment(x.AttachmentGUID));
                builder.Page(currentPage.Page.DocumentID);
                return currentPage.Page.Attachments.ToList().Select(x => AttachmentToMediaItem(x));
            }

            // Do lookup for page
            int docId = documentID.GetValueOrDefault(0);
            builder.Page(docId);
            var results = await _pageRetriever.RetrieveAsync<TreeNode>(
                query => query
                    .WhereEquals(nameof(TreeNode.DocumentID), docId),
                cacheSettings => cacheSettings
                .Configure(builder, CacheMinuteTypes.Short.ToDouble(), "GetPageAttachmentsAsync", docId)
                );

            if (results.Any()) {
                return results.First().Attachments.ToList().Select(x => AttachmentToMediaItem(x));
            } else {
                return [];
            }
        }

        private async Task<IEnumerable<MediaItem>> GetAttachmentsInternal(IEnumerable<Guid> attachmentGuids)
        {
            var builder = _cacheDependencyBuilderFactory.Create();
            foreach (var attachment in attachmentGuids) {
                builder.Attachment(attachment);
            }

            var attachmentInfos = await _progressiveCache.LoadAsync(async cs => {
                cs.CacheDependency = builder.GetCMSCacheDependency();
                var attachments = await _attachmentInfoProvider.Get().WhereIn(nameof(AttachmentInfo.AttachmentGUID), attachmentGuids.ToArray()).GetEnumerableTypedResultAsync();
                return attachments;
            }, new CacheSettings(Convert.ToDouble((int)CacheMinuteTypes.Medium), "GetAttachments", attachmentGuids.Select(x => x.ToString()).Join(",")));

            return attachmentInfos.Select(x => AttachmentToMediaItem(x));
        }

        private MediaItem MediaFileInfoToMediaItem(MediaFileInfo mediaFile)
        {
            var fileUrl = _mediaFileUrlRetriever.Retrieve(mediaFile);
            return new MediaItem(
                mediaGUID: mediaFile.FileGUID,
                mediaName: mediaFile.FileName,
                mediaTitle: mediaFile.FileTitle.AsNullOrWhitespaceMaybe().GetValueOrDefault(mediaFile.FileName),
                mediaExtension: mediaFile.FileExtension,
                mediaDirectUrl: fileUrl.DirectPath,
                mediaPermanentUrl: fileUrl.RelativePath
                ) {
                MediaDescription = mediaFile.FileDescription.AsNullOrWhitespaceMaybe()
            };

        }

        private MediaItem AttachmentToMediaItem(AttachmentInfo attachment)
        {
            var urls = _pageAttachmentUrlRetriever.Retrieve(attachment);
            var attachmentItem = new MediaItem(
                mediaName: attachment.AttachmentName,
                mediaGUID: attachment.AttachmentGUID,
                mediaDirectUrl: urls.RelativePath,
                mediaPermanentUrl: urls.RelativePath,
                mediaTitle: attachment.AttachmentTitle.AsNullOrWhitespaceMaybe().GetValueOrDefault(attachment.AttachmentName),
                mediaExtension: attachment.AttachmentExtension
            );
            return attachmentItem;
        }

        private MediaItem AttachmentToMediaItem(DocumentAttachment attachment)
        {
            var urls = _pageAttachmentUrlRetriever.Retrieve(attachment);

            var attachmentItem = new MediaItem(
                mediaName: attachment.AttachmentName,
                mediaGUID: attachment.AttachmentGUID,
                mediaDirectUrl: urls.RelativePath,
                mediaPermanentUrl: urls.RelativePath,
                mediaTitle: attachment.AttachmentTitle.AsNullOrWhitespaceMaybe().GetValueOrDefault(attachment.AttachmentName),
                mediaExtension: attachment.AttachmentExtension
            );
            return attachmentItem;
        }

        #region "Other Helpers"

        public async Task<Result<MediaItem>> GetMediaItemFromUrl(string url)
        {
            if (url.IsMediaUrl() && url.ParseGuidFromMediaUrl().TryGetValue(out var mediaMediaGuid)
                && (await GetMediaItemAsync(mediaMediaGuid)).TryGetValue(out var mediaMediaItem)) {
                return mediaMediaItem;
            }
            if (url.IsAttachmentUrl() && url.ParseGuidFromMediaUrl().TryGetValue(out var attachmentMediaGuid)
                && (await GetAttachmentInternal(attachmentMediaGuid)).TryGetValue(out var attachmentMediaItem)) {
                return attachmentMediaItem;
            }
            return Result.Failure<MediaItem>("Url is not a /getmedia or /getattachment url.");
        }

        #endregion

    }
}