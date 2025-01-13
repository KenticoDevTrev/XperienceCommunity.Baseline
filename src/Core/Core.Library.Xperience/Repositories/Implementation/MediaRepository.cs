using CMS.ContentEngine.Internal;
using CMS.MediaLibrary;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using System.Data;
using System.Text.Json;
using System.Web;
using System.Xml;

namespace Core.Repositories.Implementation
{
    public class MediaRepository(IProgressiveCache progressiveCache,
        ICacheDependencyBuilderFactory cacheDependencyBuilderFactory,
        IInfoProvider<MediaFileInfo> mediaFileInfoProvider,
        IMediaFileUrlRetriever mediaFileUrlRetriever,
        IMediaFileMediaMetadataProvider mediaFileMediaMetadataProvider,
        IContentItemMediaCustomizer contentItemMediaCustomizer,
        IIdentityService identityService,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        IContentItemMediaMetadataQueryEditor contentItemMediaMetadataQueryEditor,
        ContentItemAssetOptions contentItemAssetOptions,
        MediaFileOptions mediaFileOptions,
        IContentQueryExecutor contentQueryExecutor,
        ICacheRepositoryContext cacheRepositoryContext,
        ICacheDependenciesScope cacheDependenciesScope,
        ILanguageRepository languageFallbackRepository,
        IPageContextRepository pageContextRepository,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
        ILanguageRepository languageRepository,
        IClassContentTypeAssetConfigurationRepository classContentTypeAssetConfigurationRepository) : IMediaRepository
    {

        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ICacheDependencyBuilderFactory _cacheDependencyBuilderFactory = cacheDependencyBuilderFactory;
        private readonly IInfoProvider<MediaFileInfo> _mediaFileInfoProvider = mediaFileInfoProvider;
        private readonly IMediaFileUrlRetriever _mediaFileUrlRetriever = mediaFileUrlRetriever;
        private readonly IMediaFileMediaMetadataProvider _mediaFileMediaMetadataProvider = mediaFileMediaMetadataProvider;
        private readonly IContentItemMediaCustomizer _contentItemMediaCustomizer = contentItemMediaCustomizer;
        private readonly IIdentityService _identityService = identityService;
        private readonly IPreferredLanguageRetriever _preferredLanguageRetriever = preferredLanguageRetriever;
        private readonly IContentItemMediaMetadataQueryEditor _contentItemMediaMetadataQueryEditor = contentItemMediaMetadataQueryEditor;
        private readonly ContentItemAssetOptions _contentItemAssetOptions = contentItemAssetOptions;
        private readonly MediaFileOptions _mediaFileOptions = mediaFileOptions;
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly ICacheRepositoryContext _cacheRepositoryContext = cacheRepositoryContext;
        private readonly ICacheDependenciesScope _cacheDependenciesScope = cacheDependenciesScope;
        private readonly ILanguageRepository _languageFallbackRepository = languageFallbackRepository;
        private readonly IPageContextRepository _pageContextRepository = pageContextRepository;
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider = contentLanguageInfoProvider;
        private readonly ILanguageRepository _languageRepository = languageRepository;
        private readonly IClassContentTypeAssetConfigurationRepository _classContentTypeAssetConfigurationRepository = classContentTypeAssetConfigurationRepository;

        #region "Content Items"

        public async Task<Result<MediaItem>> GetContentItemAsset(ContentIdentity contentItem, Guid assetFieldGuid, string? language = null) => (await GetContentItemAssetsInternal([contentItem], Maybe.None, assetFieldGuid, language)).FirstOrMaybe().TryGetValue(out var firstResult) ? firstResult : Result.Failure<MediaItem>("Could not find asset given the information");

        public async Task<Result<MediaItem>> GetContentItemAsset(ContentIdentity contentItem, string assetFieldName, string? language = null) => (await GetContentItemAssetsInternal([contentItem], assetFieldName, Maybe.None, language)).FirstOrMaybe().TryGetValue(out var firstResult) ? firstResult : Result.Failure<MediaItem>("Could not find asset given the information");

        public Task<IEnumerable<MediaItem>> GetContentItemAssets(IEnumerable<ContentIdentity> contentItems, string? language = null) => GetContentItemAssetsInternal(contentItems, Maybe.None, Maybe.None, language);

        public Task<IEnumerable<MediaItem>> GetContentItemAssets(IEnumerable<ContentIdentity> contentItems, Guid assetFieldGuid, string? language = null) => GetContentItemAssetsInternal(contentItems, Maybe.None, assetFieldGuid, language);

        public Task<IEnumerable<MediaItem>> GetContentItemAssets(IEnumerable<ContentIdentity> contentItems, string assetFieldName, string? language = null) => GetContentItemAssetsInternal(contentItems, assetFieldName, Maybe.None, language);

        public Task<IEnumerable<MediaItem>> GetContentItemAssets(ContentIdentity contentItem, string? language = null) => GetContentItemAssets([contentItem], language);

        private async Task<IEnumerable<MediaItem>> GetContentItemAssetsInternal(IEnumerable<ContentIdentity> contentItems, Maybe<string> assetFieldName, Maybe<Guid> assetFieldGuid, string? language = null)
        {
            var contentIdentitiesByType = await SortByContentType(contentItems);
            var mediaItems = new List<MediaItem>();

            // Retrieve assets for all fields, separated by type
            var classNameToAssetConfiguration = await _classContentTypeAssetConfigurationRepository.GetClassNameToContentTypeAssetConfigurationDictionary();
            foreach (var type in contentIdentitiesByType.Keys) {
                // Retrieve configuration
                if (!classNameToAssetConfiguration.TryGetValue(type, out var contentAssetConfiguration)) {
                    continue;
                }

                // Get applicable Asset Field(s)
                var assetFields = contentAssetConfiguration.AssetFields.Where(x =>
                    (assetFieldName.HasNoValue && assetFieldGuid.HasNoValue) ||
                    x.AssetFieldName.Equals(assetFieldName.GetValueOrDefault(string.Empty), StringComparison.OrdinalIgnoreCase) ||
                    x.FieldGuid.Equals(assetFieldGuid.GetValueOrDefault(Guid.Empty))
                ).ToList();

                // Retrieve and convert to Media Item
                if (assetFields.Count != 0) {
                    mediaItems.AddRange(await GetContentItemsOfSameTypeToMediaItem(type, contentIdentitiesByType[type], assetFields, language ?? _preferredLanguageRetriever.Get(), contentAssetConfiguration.PreCache));
                }
            }

            return mediaItems;
        }

        #endregion

        #region "Content Item Language Metadata"

        public async Task<Result<MediaItem>> GetContentItemLanguageMetadataAsset(int? contentItemMetadataId, Guid assetFieldGuid)
        {
            if ((await GetContentIdentityAndLanguage(contentItemMetadataId)).TryGetValue(out var identityAndLanguage)) {
                return (await GetContentItemAssetsInternal([identityAndLanguage.ContentIdentity], Maybe.None, assetFieldGuid, identityAndLanguage.Language)).FirstOrMaybe().TryGetValue(out var mediaItem) ? mediaItem : Result.Failure<MediaItem>("No media item found");
            }
            return Result.Failure<MediaItem>("Content Item Metadata ID not provided and there is no current page context");
        }

        public async Task<Result<MediaItem>> GetContentItemLanguageMetadataAsset(int? contentItemMetadataId, string assetFieldName)
        {
            if ((await GetContentIdentityAndLanguage(contentItemMetadataId)).TryGetValue(out var identityAndLanguage)) {
                return (await GetContentItemAssetsInternal([identityAndLanguage.ContentIdentity], assetFieldName, Maybe.None, identityAndLanguage.Language)).FirstOrMaybe().TryGetValue(out var mediaItem) ? mediaItem : Result.Failure<MediaItem>("No media item found");
            }
            return Result.Failure<MediaItem>("Content Item Metadata ID not provided and there is no current page context");
        }

        public async Task<IEnumerable<MediaItem>> GetContentItemLanguageMetadataAssets(int? contentItemMetadataId)
        {
            if ((await GetContentIdentityAndLanguage(contentItemMetadataId)).TryGetValue(out var identityAndLanguage)) {
                return await GetContentItemAssetsInternal([identityAndLanguage.ContentIdentity], Maybe.None, Maybe.None, identityAndLanguage.Language);
            }
            return [];
        }

        #endregion

        #region "Content Item Language Metadata Helpers"

        private async Task<Result<ContentIdentityAndLanguage>> GetContentIdentityAndLanguage(int? contentItemMetadataId)
        {
            if (contentItemMetadataId.AsMaybe().TryGetValue(out var metadataId)
                && (await GetContentItemMetadataIdToIdentityAndLanguage()).TryGetValue(metadataId, out var identityAndLang)
                ) {
                return new ContentIdentityAndLanguage(identityAndLang.ContentIdentity, identityAndLang.Language);
            } else if ((await _pageContextRepository.GetCurrentPageAsync()).TryGetValue(out var currentPage)) {
                return new ContentIdentityAndLanguage(currentPage.ContentIdentity, _preferredLanguageRetriever.Get().ToLowerInvariant());
            }
            return Result.Failure<ContentIdentityAndLanguage>("Cannot find proper content item based on given data");
        }

        private async Task<Dictionary<int, ContentIdentityAndLanguage>> GetContentItemMetadataIdToIdentityAndLanguage()
        {
            var builder = _cacheDependencyBuilderFactory.Create(false)
                .AddKey("contentitem|all")
                .ObjectType(ContentLanguageInfo.OBJECT_TYPE);

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }
                var langById = (await _contentLanguageInfoProvider.Get()
                .Columns(nameof(ContentLanguageInfo.ContentLanguageID), nameof(ContentLanguageInfo.ContentLanguageName))
                .GetEnumerableTypedResultAsync()).ToDictionary(key => key.ContentLanguageID, value => value.ContentLanguageName.ToLowerInvariant());

                var query = @$"SELECT {nameof(ContentItemFields.ContentItemID)}, {nameof(ContentItemFields.ContentItemGUID)}, {nameof(ContentItemFields.ContentItemName)}, ContentItemLanguageMetadataID, ContentItemLanguageMetadataContentLanguageID FROM [CMS_ContentItemLanguageMetadata]
    inner join CMS_ContentItem on ContentItemID = ContentItemLanguageMetadataContentItemID";

                return ((await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, [], QueryTypeEnum.SQLQuery)).Tables[0].Rows.Cast<DataRow>())
                .ToDictionary(key => (int)key["ContentItemLanguageMetadataID"], value => new ContentIdentityAndLanguage(new ContentIdentity() {
                    ContentID = (int)value[nameof(ContentItemFields.ContentItemID)],
                    ContentGuid = (Guid)value[nameof(ContentItemFields.ContentItemGUID)],
                    ContentName = (string)value[nameof(ContentItemFields.ContentItemName)]
                }, langById[(int)value["ContentItemLanguageMetadataContentLanguageID"]]));
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetContentItemMetadataIdToIdentityAndLanguage"));
        }

        #endregion

        #region "Content Item Asset Helpers"

       

        private async Task<IEnumerable<MediaItem>> GetContentItemsOfSameTypeToMediaItem(string contentType, IEnumerable<ContentIdentity> contentItems, IEnumerable<AssetFieldIdentifier> assetFields, string language, bool useCache)
        {
            var cacheBuilder = _cacheDependencyBuilderFactory.Create(false).ContentType(contentType);

            if (useCache && cacheBuilder.DependenciesNotTouchedSince(new TimeSpan(0, 0, 30))) {
                // Use Cached Result
                var dictionary = await GetCachedContentGuidToFieldGuidToLanguageMediaItems(contentType);
                var mediaItems = new List<MediaItem>();
                foreach (var contentItem in contentItems) {
                    if ((await contentItem.GetOrRetrieveContentGuid(_identityService)).TryGetValue(out var contentItemGuid)
                        && dictionary.TryGetValue(contentItemGuid, out var fieldToLangMediaItemDictionary)) {
                        foreach (var assetField in assetFields) {
                            if (fieldToLangMediaItemDictionary.TryGetValue(assetField.FieldGuid, out var langToMediaDictionary)) {
                                // Find the best language match of the given available assets
                                if ((await _languageFallbackRepository.GetLanguagueToSelect(langToMediaDictionary.Keys, language, true)).TryGetValue(out var lang)) {
                                    mediaItems.Add(langToMediaDictionary[lang.ToLowerInvariant()]);
                                }
                            }
                        }
                    }
                }
                return mediaItems;
            } else {

                // Still cache, but do so individually and only for a shorter period of time.
                var mediaItems = await _progressiveCache.LoadAsync(async cs => {
                    // Will add dependency keys if user sets in their customizations
                    _cacheDependenciesScope.Begin();

                    var queryResults = await _contentItemMediaMetadataQueryEditor.CustomizeMediaQuery(contentType, contentItems, assetFields, language);

                    // If not handled, then do a normal query getting only the fields needed.
                    if (!queryResults.TryGetValue(out var queryBuilder)) {
                        queryBuilder = new ContentItemQueryBuilder()
                            .ForContentType(contentType, query => query
                                .Columns(
                                    assetFields.SelectMany(x => new string[] { x.AssetFieldName, x.TitleFieldName, x.DescriptionFieldName.GetValueOrDefault(string.Empty) })
                                        .Except([string.Empty])
                                        .Union([nameof(ContentItemFields.ContentItemGUID), nameof(ContentItemFields.ContentItemCommonDataContentLanguageID)])
                                        .Distinct().ToArray()
                                        )
                                .InContentIdentities(contentItems)
                            )
                            .InLanguage(language, useLanguageFallbacks: true);
                    }

                    // Execute
                    var result = await _contentQueryExecutor.GetResult(builder: queryBuilder, options: new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext), resultSelector: async (fileItem) => {
                        var mediaItems = new List<MediaItem>();
                        foreach (var assetField in assetFields) {
                            if ((await ParseMediaItem(fileItem, assetField)).TryGetValue(out var mediaItem)) {
                                mediaItems.Add(mediaItem);
                            }
                        }
                        return mediaItems;
                    });

                    // Combine media items
                    var allMediaItems = result.SelectMany(x => x);

                    // Media Guid is the Content Item Guid
                    var internalBuilder = _cacheDependencyBuilderFactory.Create(false).ContentItemsOnLanguage(allMediaItems.Select(x => x.MediaGUID), language);

                    // Get any keys from customize interface calls
                    var keys = _cacheDependenciesScope.End();

                    if(cs.Cached) {
                        cs.CacheDependency = CacheHelper.GetCacheDependency(keys);
                    }

                    return new MediaItemsAndKeys(allMediaItems, keys);

                }, new CacheSettings(CacheMinuteTypes.Medium.ToDouble(), "GetContentItemsOfSameTypeToMediaItem", contentType, contentItems.Select(x => x.GetCacheKey()), language, assetFields.Select(x => x.FieldGuid.ToString()).Join("|")));

                // Add dependencies to the cache scope
                var builder = _cacheDependencyBuilderFactory.Create()
                    .AddKeys(mediaItems.Keys);

                return mediaItems.MediaItems;
            }
        }


        private async Task<Dictionary<Guid, Dictionary<Guid, Dictionary<string, MediaItem>>>> GetCachedContentGuidToFieldGuidToLanguageMediaItems(string contentType)
        {
            var builder = _cacheDependencyBuilderFactory.Create(false)
                .ContentType(contentType);

            return await _progressiveCache.LoadAsync(async cs => {

                // Will add dependency keys if user sets in their customizations
                _cacheDependenciesScope.Begin();

                var configuration = await _classContentTypeAssetConfigurationRepository.GetClassNameToContentTypeAssetConfigurationDictionary();
                if (!configuration.TryGetValue(contentType.ToLowerInvariant(), out var classAssetConfiguration)) {
                    return [];
                }

                var queryResults = await _contentItemMediaMetadataQueryEditor.CustomizeMediaQueryAllCached(contentType, classAssetConfiguration.AssetFields);

                // If not handled, then do a normal query getting only the fields needed.
                if (!queryResults.TryGetValue(out var queryBuilder)) {
                    queryBuilder = new ContentItemQueryBuilder()
                        .ForContentType(contentType, query => query
                            .Columns(
                                classAssetConfiguration.AssetFields.SelectMany(x => new string[] { x.AssetFieldName, x.TitleFieldName, x.DescriptionFieldName.GetValueOrDefault(string.Empty) })
                                .Except([string.Empty])
                                .Union([nameof(ContentItemFields.ContentItemGUID), nameof(ContentItemFields.ContentItemCommonDataContentLanguageID)])
                                .Distinct().ToArray())
                        );
                }

                // Execute
                var allContentItemDataContainers = await _contentQueryExecutor.GetResult(builder: queryBuilder, options: new ContentQueryExecutionOptions().WithPreviewModeContext(_cacheRepositoryContext), resultSelector: (contentItemDataContainer) => {
                    return contentItemDataContainer;
                });

                var compiledDictionary = new Dictionary<Guid, Dictionary<Guid, Dictionary<string, MediaItem>>>();
                foreach (var contentItemDataContainer in allContentItemDataContainers) {
                    if (!compiledDictionary.TryGetValue(contentItemDataContainer.ContentItemGUID, out Dictionary<Guid, Dictionary<string, MediaItem>>? contentItemDictionary)) {
                        contentItemDictionary = [];
                        compiledDictionary.Add(contentItemDataContainer.ContentItemGUID, contentItemDictionary);
                    }

                    var language = _languageRepository.LanguageIdToName(contentItemDataContainer.ContentItemCommonDataContentLanguageID).ToLowerInvariant();

                    foreach (var assetField in classAssetConfiguration.AssetFields) {
                        if (!contentItemDictionary.TryGetValue(assetField.FieldGuid, out Dictionary<string, MediaItem>? languageToMediaItemDictionary)) {
                            languageToMediaItemDictionary = [];
                            contentItemDictionary.Add(assetField.FieldGuid, languageToMediaItemDictionary);
                        }

                        // get language
                        if ((await ParseMediaItem(contentItemDataContainer, assetField)).TryGetValue(out var mediaItem)) {
                            languageToMediaItemDictionary.TryAdd(language, mediaItem);
                        }
                    }
                }

                // Add Keys and set dependencies
                if (cs.Cached) {
                    cs.CacheDependency = builder.AddKeys(_cacheDependenciesScope.End()).GetCMSCacheDependency();
                }

                return compiledDictionary;
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetCachedContentGuidToFieldGuidToLanguageMediaItems", contentType, _cacheRepositoryContext.GetCacheKey()));
        }

        private async Task<Result<MediaItem>> ParseMediaItem(IContentQueryDataContainer fileItem, AssetFieldIdentifier assetField)
        {
            if (!JsonSerializer.Deserialize<ContentItemAssetMetadata>(fileItem.GetValue<string>(assetField.AssetFieldName)).AsMaybe().TryGetValue(out var metaData)) {
                return Result.Failure<MediaItem>($"Content Item Asset Metadata was not found on the field [{assetField.AssetFieldName}]");
            }
            var title = fileItem.GetValue<string>(assetField.TitleFieldName);

            var description = Maybe<string>.None;
            try {
                description = assetField.DescriptionFieldName.TryGetValue(out var descriptionFieldName) ? fileItem.GetValue<string>(descriptionFieldName).AsNullOrWhitespaceMaybe() : Maybe<string>.None;
            } catch (Exception) {
                // ignore
            }

            var language = _languageRepository.LanguageIdToName(fileItem.ContentItemCommonDataContentLanguageID);

            var permanentUrl = $"/getcontentasset/{fileItem.ContentItemGUID}/{assetField.FieldGuid}/{metaData.Name}?language={language}";
            var directUrl = $"/getcontentasset/{fileItem.ContentItemGUID}/{assetField.FieldGuid}/{metaData.Name}?language={language}";
            //var directUrl = $"/assets/contentitems/{fileItem.ContentItemGUID.ToString()[..2]}/{attachmentFieldGuid}/{metaData.Identifier}{metaData.Extension}".ToLower();

            var mediaItem = new MediaItem(new ContentItemMediaIdentifiers(fileItem.ContentItemGUID, assetField.FieldGuid, assetField.AssetFieldName, metaData.Identifier, language), metaData.Name, title, metaData.Extension, directUrl, permanentUrl) {
                MediaDescription = description
            };

            if ((await _contentItemMediaCustomizer.GetMediaMetadata(fileItem, metaData, mediaItem)).TryGetValue(out var customMediaMetadata)) {
                mediaItem = mediaItem with { MetaData = Maybe.From(customMediaMetadata) };
            }

            return mediaItem;

        }

        private async Task<Dictionary<string, List<ContentIdentity>>> SortByContentType(IEnumerable<ContentIdentity> contentItems)
        {
            var contentIdentitiesByType = new Dictionary<string, List<ContentIdentity>>();
            foreach (var contentItem in contentItems) {
                if ((await _identityService.GetContentType(contentItem)).TryGetValue(out var type)) {
                    if (!contentIdentitiesByType.TryGetValue(type.ToLowerInvariant(), out var identityList)) {
                        identityList = [];
                        contentIdentitiesByType.Add(type.ToLowerInvariant(), identityList);
                    }
                    identityList.Add(contentItem);
                }
            }
            return contentIdentitiesByType;
        }

        #endregion

        #region "Media_File Items"

        public async Task<Result<MediaItem>> GetMediaItemAsync(Guid fileGuid)
        {
            // use cached if possible, much faster, but does have some drawbacks like the DirectUrl is excluded for performance
            if ( 
                _mediaFileOptions.UseCachedMediaFiles
                &&  _cacheDependencyBuilderFactory.Create(false)
                        .ObjectType(MediaFileInfo.OBJECT_TYPE)
                        .DependenciesNotTouchedSince(new TimeSpan(0, 0, 30))
                && (await GetMediaFileGuidToMediaFile()).TryGetValue(fileGuid, out var mediaFile)) {
                var mediaItem = MediaFileInfoToMediaItemCached(mediaFile);

                if ((await _mediaFileMediaMetadataProvider.GetMediaMetadata(mediaFile, mediaItem)).TryGetValue(out var metaData)) {
                    return mediaItem with { MetaData = Maybe.From(metaData) };
                }
                return mediaItem;
            } else {
                var builder = _cacheDependencyBuilderFactory.Create()
                .Object(MediaFileInfo.OBJECT_TYPE, fileGuid);

                return await _progressiveCache.LoadAsync(async cs => {
                    if (cs.Cached) {
                        cs.CacheDependency = builder.GetCMSCacheDependency();
                    }
                    var mediaFile = await _mediaFileInfoProvider.Get()
                        .WhereEquals(nameof(MediaFileInfo.FileGUID), fileGuid)
                        .GetEnumerableTypedResultAsync();

                    if (mediaFile.FirstOrMaybe().TryGetValue(out var mediaFileVal)) {
                        var mediaItem = MediaFileInfoToMediaItem(mediaFileVal);
                        if ((await _mediaFileMediaMetadataProvider.GetMediaMetadata(mediaFileVal, mediaItem)).TryGetValue(out var metaData)) {
                            mediaItem = mediaItem with { MetaData = metaData.AsMaybe() };
                        }
                        return mediaItem;
                    }
                    return Result.Failure<MediaItem>("Could not find Media Item");
                }, new CacheSettings(CacheMinuteTypes.Short.ToDouble(), "GetMediaItemAsync", fileGuid));
            }
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
                foreach (var mediaFile in mediaFiles) {
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

        #endregion

        #region "Media_File Item Helpers"

        private async Task<Dictionary<Guid, MediaFileInfo>> GetMediaFileGuidToMediaFile()
        {
            var builder = _cacheDependencyBuilderFactory.Create(false)
                .ObjectType(MediaFileInfo.OBJECT_TYPE);

            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = builder.GetCMSCacheDependency();
                }

                var allMediaFileItems = await _mediaFileInfoProvider.Get()
                    .Columns(nameof(MediaFileInfo.FileID)
                            , nameof(MediaFileInfo.FileName)
                            , nameof(MediaFileInfo.FileTitle)
                            , nameof(MediaFileInfo.FileDescription)
                            , nameof(MediaFileInfo.FileExtension)
                            , nameof(MediaFileInfo.FileMimeType)
                            , nameof(MediaFileInfo.FilePath)
                            , nameof(MediaFileInfo.FileSize)
                            , nameof(MediaFileInfo.FileImageWidth)
                            , nameof(MediaFileInfo.FileImageHeight)
                            , nameof(MediaFileInfo.FileGUID)
                            , nameof(MediaFileInfo.FileModifiedWhen)
                            , nameof(MediaFileInfo.FileCustomData))
                    .GetEnumerableTypedResultAsync();

                return allMediaFileItems.ToDictionary(key => key.FileGUID, value => value);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetMediaFileGuidToMediaItem"));
        }

        #endregion

        #region "Obsolete Methods"

        public Task<Result<string>> GetMediaAttachmentSiteNameAsync(Guid mediaOrAttachmentGuid)
        {
            throw new NotSupportedException("There is no longer a Media File Channel or Attachment Channel, neither require the sitename parameter to operate, you can remove usage of this.");
        }

        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAsset(attachmentGuid.ToObjectIdentity(), \"EventualContentAssetFieldName\"). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        public Task<Result<MediaItem>> GetAttachmentItemAsync(Guid attachmentGuid) => GetContentItemAsset(attachmentGuid.ToContentIdentity(), string.Empty);

        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAssets(attachmentGuids.Select(x => x.ToObjectIdentity())). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        public Task<IEnumerable<MediaItem>> GetAttachmentsAsync(IEnumerable<Guid> attachmentGuids) => GetContentItemAssets(attachmentGuids.Select(x => x.ToContentIdentity()));

        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemAsset(attachmentGuid.ToObjectIdentity(), \"EventualContentAssetFieldName\"). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        public Task<Result<MediaItem>> GetPageAttachmentAsync(Guid attachmentGuid, int? documentID = null) => GetContentItemAsset(attachmentGuid.ToContentIdentity(), string.Empty);

        [Obsolete("Attachments are being replaced by Content Item Assets in Xperience by Kentico.  Please use GetContentItemLanguageMetadataAssets(int? contentItemMetadataId). You can also migrate to use the media library (using the XperienceCommunity.MediaLibraryMigrationToolkit), however this too will eventually be replaced with Content Item Assets.  A migration tool for Media File to Content Asset Item will eventually be provided.")]
        public Task<IEnumerable<MediaItem>> GetPageAttachmentsAsync(int? documentID = null) => GetContentItemLanguageMetadataAssets(documentID);

        #endregion

        #region "Other Helpers"

        public async Task<Result<MediaItem>> GetMediaItemFromUrl(string url)
        {
            if (url.IsMediaUrl() && url.ParseGuidFromMediaUrl().TryGetValue(out var mediaMediaGuid)
                && (await GetMediaItemAsync(mediaMediaGuid)).TryGetValue(out var mediaMediaItem)) {
                return mediaMediaItem;
            }
            if (url.IsContentAssetUrl() && url.ParseGuidFromAssetUrl().TryGetValue(out var mediaContentAssetGuid)
                && (await GetContentItemAsset(mediaContentAssetGuid.ContentItemGuid.ToContentIdentity(), mediaContentAssetGuid.FieldGuid, url.GetContentAssetUrlLanguage().AsNullableValue())).TryGetValue(out var contentMediaItem)) {
                return contentMediaItem;
            }
            return Result.Failure<MediaItem>("Url is not a /getmedia or /getcontentasset url.");
        }

        #endregion

        #region "Media Helpers"

        private static MediaItem MediaFileInfoToMediaItemCached(MediaFileInfo mediaFile)
        {
            return new MediaItem(
                mediaGUID: mediaFile.FileGUID,
                mediaName: mediaFile.FileName,
                mediaTitle: mediaFile.FileTitle.AsNullOrWhitespaceMaybe().GetValueOrDefault(mediaFile.FileName),
                mediaExtension: mediaFile.FileExtension,
                mediaDirectUrl: string.Format("/getmedia/{0}/{1}", Convert.ToString(mediaFile.FileGUID), HttpUtility.UrlPathEncode(mediaFile.FileName)),
                mediaPermanentUrl: string.Format("/getmedia/{0}/{1}", Convert.ToString(mediaFile.FileGUID), HttpUtility.UrlPathEncode(mediaFile.FileName))
                ) {
                MediaDescription = mediaFile.FileDescription.AsNullOrWhitespaceMaybe()
            };
        }

        private MediaItem MediaFileInfoToMediaItem(MediaFileInfo mediaFile)
        {
            // using own logic since media file url retriever is not optimized
            var extension = mediaFile.FileExtension.Trim('.').ToLower();
            var fileUrl = _mediaFileUrlRetriever.Retrieve(mediaFile);

            return new MediaItem(
                mediaGUID: mediaFile.FileGUID,
                mediaName: mediaFile.FileName,
                mediaTitle: mediaFile.FileTitle.AsNullOrWhitespaceMaybe().GetValueOrDefault(mediaFile.FileName),
                mediaExtension: mediaFile.FileExtension,
                mediaDirectUrl: fileUrl.DirectPath,
                mediaPermanentUrl: fileUrl.RelativePath
                ) {
                MediaDescription = mediaFile.FileDescription.AsNullOrWhitespaceMaybe(),
                MetaData = _imageExtensions.Contains(extension) ? new MediaMetadataImage(mediaFile.FileImageWidth, mediaFile.FileImageHeight) : Maybe<IMediaMetadata>.None
            };
        }

        private static readonly string[] _imageExtensions = ["png", "gif", "bmp", "jpg", "jpeg", "webp"];

        #endregion

        #region "Data Holder Records"

        private record ContentIdentityAndLanguage(ContentIdentity ContentIdentity, string Language);
        private record MediaItemsAndKeys(IEnumerable<MediaItem> MediaItems, string[] Keys);

    #endregion
}
}
