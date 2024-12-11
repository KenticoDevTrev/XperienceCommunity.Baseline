using CMS.ContentEngine;
using CMS.Websites;
using Core.Repositories;
using Generic;

namespace Site.Repositories.Implementations
{
    public class CustomWebPageToPageMetadataConverter(IContentQueryResultMapper contentQueryResultMapper) : IWebPageToPageMetadataConverter
    {
        private readonly IContentQueryResultMapper _contentQueryResultMapper = contentQueryResultMapper;

        // BASELINE CUSTOMIZATION: Start Site - Any Web Pages should have logic here to parse the metadata.
        public Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageContentQueryDataContainer webPageContentQueryDataContainer)
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
                return Task.FromResult(Result.Success(new PageMetaData() {
                    CanonicalUrl = webPageContentQueryDataContainer.WebPageUrlPath.AsNullOrWhitespaceMaybe(),
                    Title = metadataObject.MetaData_Title.AsNullOrWhitespaceMaybe().GetValueOrDefault(
                        webPageContentQueryDataContainer.GetValue<string>("ContentItemLanguageMetadataDisplayName") ?? ""
                        ).AsNullOrWhitespaceMaybe(),
                    Description = metadataObject.MetaData_Description.AsNullOrWhitespaceMaybe(),
                    Keywords = metadataObject.MetaData_Keywords.AsNullOrWhitespaceMaybe(),
                    NoIndex = metadataObject.MetaData_NoIndex,
                    Thumbnail = metadataObject.MetaData_ThumbnailSmall.FirstOrMaybe().TryGetValue(out var thumbnail) ? $"/getmedia/{thumbnail.Identifier}/{thumbnail.Name}" : Maybe.None,
                    ThumbnailLarge = metadataObject.MetaData_ThumbnailSmall.FirstOrMaybe().TryGetValue(out var thumbnailLarge) ? $"/getmedia/{thumbnailLarge.Identifier}/{thumbnailLarge.Name}" : Maybe.None
                }));
            }

            // A result failure will kick in the default 'logic' in the IMetaDataRepository, trying to get the title and URL only.
            return Task.FromResult(Result.Failure<PageMetaData>("Not defined"));
        }
    }
}
