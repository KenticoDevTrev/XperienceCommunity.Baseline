using CMS.ContentEngine;
using CMS.Websites;
using Core.Repositories;
using Core.Repositories.Implementation;
using Core.Services;
using Core.Services.Implementations;

namespace Site.Repositories.Implementations
{
    public class CustomWebPageToPageMetadataConverter(
        /*
        IContentQueryResultMapper contentQueryResultMapper,
        IUrlResolver urlResolver,
        ILanguageRepository languageRepository
        */
        ) : IContentItemToPageMetadataConverter
    {
        /*
        private readonly IContentQueryResultMapper _contentQueryResultMapper = contentQueryResultMapper;
        private readonly IUrlResolver _urlResolver = urlResolver;
        private readonly ILanguageRepository _languageRepository = languageRepository;
        */
        // BASELINE CUSTOMIZATION - Start Site - Any Web Pages should have logic here to parse the metadata.
        public Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageContentQueryDataContainer webPageContentQueryDataContainer, PageMetaData baseMetaData)
        {
            // Just a sample, any normal WebPageItem that inherits the IBaseMetadata and uses standard routing from Kentico is handled by default.
            /*
            if (webPageContentQueryDataContainer.ContentTypeName.Equals("MySite.BlogArticle")) {
                var article = _contentQueryResultMapper.Map<BlogArticle>(webPageContentQueryDataContainer);
                return baseMetaData with {
                    CanonicalUrl = _urlResolver.GetAbsoluteUrl($"/{_languageRepository.GetLanguageUrlPrefix(article.WebsiteChannelID, article.SystemFields.ContentItemCommonDataContentLanguageID)}/blog/{article.Year}/{article.Month}/{article.BlogTitle.Replace(" ", "-")}"),
                    Title = article.BlogTitle,
                    Description = article.BlogSummary,
                    Thumbnail = _urlResolver.GetAbsoluteUrl(article.BlogThumbnail.Url)
                };
            }
            */
            // A result failure will kick in the default 'logic' in the IMetaDataRepository, trying to get the title and URL only.
            return Task.FromResult(Result.Failure<PageMetaData>("No customization needed"));
        }

        public Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageFieldsSource webPageFieldSource, PageMetaData baseMetaData)
        {
            // This is the actual parsed object, so should be able to do type casting
            /*
            if(webPageFieldSource is BlogArticle article) {
                return baseMetaData with {
                    CanonicalUrl = _urlResolver.GetAbsoluteUrl($"/{_languageRepository.GetLanguageUrlPrefix(article.WebsiteChannelID, article.SystemFields.ContentItemCommonDataContentLanguageID)}/blog/{article.Year}/{article.Month}/{article.BlogTitle.Replace(" ", "-")}"),
                    Title = article.BlogTitle,
                    Description = article.BlogSummary,
                    Thumbnail = _urlResolver.GetAbsoluteUrl(article.BlogThumbnail.Url)
                };
            }*/
            return Task.FromResult(Result.Failure<PageMetaData>("No customization needed"));
        }

        public Task<Result<PageMetaData>> MapAndGetPageMetadataReusableContent(IContentQueryDataContainer contentQueryDataContainer, PageMetaData baseMetaData, string canonicalUrl)
        {
            // Just a sample, any normal WebPageItem that inherits the IBaseMetadata and uses standard routing from Kentico is handled by default.
            /*
            if (webPageContentQueryDataContainer.ContentTypeName.Equals("MySite.BlogArticle")) {
                var article = _contentQueryResultMapper.Map<BlogArticle>(webPageContentQueryDataContainer);
                return baseMetaData with {
                    CanonicalUrl = _urlResolver.GetAbsoluteUrl($"/{_languageRepository.GetLanguageUrlPrefix(article.WebsiteChannelID, article.SystemFields.ContentItemCommonDataContentLanguageID)}/blog/{article.Year}/{article.Month}/{article.BlogTitle.Replace(" ", "-")}"),
                    Title = article.BlogTitle,
                    Description = article.BlogSummary,
                    Thumbnail = _urlResolver.GetAbsoluteUrl(article.BlogThumbnail.Url)
                };
            }
            */
            // A result failure will kick in the default 'logic' in the IMetaDataRepository, trying to get the title and URL only.
            return Task.FromResult(Result.Failure<PageMetaData>("No customization needed"));
        }

        public Task<Result<PageMetaData>> MapAndGetPageMetadataReusableContent(IContentItemFieldsSource contentItemFieldSource, PageMetaData baseMetaData, string canonicalUrl)
        {
            // This is the actual parsed object, so should be able to do type casting
            /*
            if(webPageFieldSource is BlogArticle article) {
                return baseMetaData with {
                    CanonicalUrl = _urlResolver.GetAbsoluteUrl($"/{_languageRepository.GetLanguageUrlPrefix(article.WebsiteChannelID, article.SystemFields.ContentItemCommonDataContentLanguageID)}/blog/{article.Year}/{article.Month}/{article.BlogTitle.Replace(" ", "-")}"),
                    Title = article.BlogTitle,
                    Description = article.BlogSummary,
                    Thumbnail = _urlResolver.GetAbsoluteUrl(article.BlogThumbnail.Url)
                };
            }*/
            return Task.FromResult(Result.Failure<PageMetaData>("No customization needed"));
        }
    }
}
