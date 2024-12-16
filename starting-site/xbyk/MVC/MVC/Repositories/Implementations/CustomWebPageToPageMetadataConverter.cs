using CMS.ContentEngine;
using CMS.Websites;
using Core.Repositories;
using Generic;

namespace Site.Repositories.Implementations
{
    public class CustomWebPageToPageMetadataConverter(IContentQueryResultMapper contentQueryResultMapper, IMediaRepository mediaRepository,
        IContentItemLanguageMetadataRepository contentItemLanguageMetadataRepository) : IWebPageToPageMetadataConverter
    {
        private readonly IContentQueryResultMapper _contentQueryResultMapper = contentQueryResultMapper;
        private readonly IMediaRepository _mediaRepository = mediaRepository;
        private readonly IContentItemLanguageMetadataRepository _contentItemLanguageMetadataRepository = contentItemLanguageMetadataRepository;

        // BASELINE CUSTOMIZATION: Start Site - Any Web Pages should have logic here to parse the metadata.
        public async Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageContentQueryDataContainer webPageContentQueryDataContainer)
        {
            // I'm only doing the IBaseMetadata logic, but if you have custom content types you could adjust this method to do whatever you wish.
            Result<IBaseMetadata> metadata = webPageContentQueryDataContainer.ContentTypeName switch {
                Home.CONTENT_TYPE_NAME => Result.Success<IBaseMetadata>(_contentQueryResultMapper.Map<Home>(webPageContentQueryDataContainer)),
                BasicPage.CONTENT_TYPE_NAME => Result.Success<IBaseMetadata>(_contentQueryResultMapper.Map<BasicPage>(webPageContentQueryDataContainer)),
                Generic.Account.CONTENT_TYPE_NAME => Result.Success<IBaseMetadata>(_contentQueryResultMapper.Map<Generic.Account>(webPageContentQueryDataContainer)),
                _ => Result.Failure<IBaseMetadata>("This class doesn't inherit the base.")
            };
            
            // Using custom parsing, of course you can use your own logic with each page type if you wish
            if(metadata.TryGetValue(out var metadataObject)) {
                var thumbnailSmall = string.Empty;
                var thumbnailLarge = string.Empty;

                if (metadataObject.MetaData_ThumbnailSmall.FirstOrMaybe().TryGetValue(out var smallThumbIdentity)) {
                    var medias = await _mediaRepository.GetContentItemAssets(smallThumbIdentity.Identifier.ToContentIdentity());
                    if(medias.FirstOrMaybe().TryGetValue(out var media)) {
                        thumbnailSmall = media.MediaPermanentUrl;
                    }
                }
                if (metadataObject.MetaData_ThumbnailLarge.FirstOrMaybe().TryGetValue(out var largeThumbIdentity)) {
                    var medias = await _mediaRepository.GetContentItemAssets(largeThumbIdentity.Identifier.ToContentIdentity());
                    if (medias.FirstOrMaybe().TryGetValue(out var media)) {
                        thumbnailLarge = media.MediaPermanentUrl;
                    }
                }

                // Title should be the Title, followed by MenuName, followed by the Content Item Language Metadata Display Name
                var title = metadataObject.MetaData_Title.AsNullOrWhitespaceMaybe().GetValueOrDefault(
                        metadataObject.MetaData_MenuName.AsNullOrWhitespaceMaybe().GetValueOrDefault("")).AsNullOrWhitespaceMaybe();
                if(title.HasNoValue && (await _contentItemLanguageMetadataRepository.GetOptimizedContentItemLanguageMetadata(webPageContentQueryDataContainer, true, true)).TryGetValue(out var langMetadata)) {
                    title = langMetadata.ContentItemLanguageMetadataDisplayName;
                }

                return Result.Success(new PageMetaData() {
                    CanonicalUrl = webPageContentQueryDataContainer.WebPageUrlPath.AsNullOrWhitespaceMaybe(),
                    Title = title,
                    Description = metadataObject.MetaData_Description.AsNullOrWhitespaceMaybe(),
                    Keywords = metadataObject.MetaData_Keywords.AsNullOrWhitespaceMaybe(),
                    NoIndex = metadataObject.MetaData_NoIndex,
                    Thumbnail = thumbnailSmall.AsNullOrWhitespaceMaybe(),
                    ThumbnailLarge = thumbnailLarge.AsNullOrWhitespaceMaybe()
                });
            }

            // A result failure will kick in the default 'logic' in the IMetaDataRepository, trying to get the title and URL only.
            return Result.Failure<PageMetaData>("Not defined");
        }
    }
}
