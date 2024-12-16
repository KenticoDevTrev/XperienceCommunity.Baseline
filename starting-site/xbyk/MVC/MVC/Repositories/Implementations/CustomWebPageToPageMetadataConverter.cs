using CMS.ContentEngine;
using CMS.Websites;
using Core.Repositories;
using Core.Services;

namespace Site.Repositories.Implementations
{
    public class CustomWebPageToPageMetadataConverter(
        /*
        IContentQueryResultMapper contentQueryResultMapper,
        IUrlResolver urlResolver,
        ILanguageRepository languageRepository
        */
        ) : IWebPageToPageMetadataConverter
    {
        /*
        private readonly IContentQueryResultMapper _contentQueryResultMapper = contentQueryResultMapper;
        private readonly IUrlResolver _urlResolver = urlResolver;
        private readonly ILanguageRepository _languageRepository = languageRepository;
        */
        // BASELINE CUSTOMIZATION: Start Site - Any Web Pages should have logic here to parse the metadata.
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
    }
}
