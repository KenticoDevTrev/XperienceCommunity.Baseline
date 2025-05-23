﻿using CMS.Websites;

namespace Core.Repositories
{
    public interface IContentItemToPageMetadataConverter
    {
        /// <summary>
        /// This allows you to do custom mapping and adjustments from the base metadata.  This is useful if you want to add custom URL routing, titles, thumbnails, etc based on the type.
        /// </summary>
        /// <param name="webPageContentQueryDataContainer">The raw item</param>
        /// <param name="baseMetaData">This is the base metadata (what will be used if you do not modify and return a result). Leverages fields from the IBaseMetaData if available.</param>
        /// <returns></returns>
        Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageContentQueryDataContainer webPageContentQueryDataContainer, PageMetaData baseMetaData);

        /// <summary>
        /// This allows you to do custom mapping and adjustments from the base metadata.  This is useful if you want to add custom URL routing, titles, thumbnails, etc based on the type.
        /// </summary>
        /// <param name="webPageFieldSource">The converted web page (it is a class so can check for type casting)</param>
        /// <param name="baseMetaData">This is the base metadata (what will be used if you do not modify and return a result). Leverages fields from the IBaseMetaData if available.</param>
        /// <returns></returns>
        Task<Result<PageMetaData>> MapAndGetPageMetadata(IWebPageFieldsSource webPageFieldSource, PageMetaData baseMetaData);


        /// <summary>
        /// This allows you to do custom mapping and adjustments from the base metadata for non-webpage items.  This is useful if you want to add custom URL routing, titles, thumbnails, etc based on the type.
        /// </summary>
        /// <param name="contentQueryDataContainer">The raw item</param>
        /// <param name="baseMetaData">This is the base metadata (what will be used if you do not modify and return a result). Leverages fields from the IBaseMetaData if available.</param>
        /// <param name="canonicalUrl">The Canonical Url that was passed to generate the original PageMetaData.</param>
        /// <returns></returns>
        Task<Result<PageMetaData>> MapAndGetPageMetadataReusableContent(IContentQueryDataContainer contentQueryDataContainer, PageMetaData baseMetaData, string? canonicalUrl);

        /// <summary>
        /// This allows you to do custom mapping and adjustments from the base metadata for non-webpage items.  This is useful if you want to add custom URL routing, titles, thumbnails, etc based on the type.
        /// </summary>
        /// <param name="contentItemFieldSource">The converted content item (it is a class so can check for type casting)</param>
        /// <param name="baseMetaData">This is the base metadata (what will be used if you do not modify and return a result). Leverages fields from the IBaseMetaData if available.</param>
        /// <param name="canonicalUrl">The Canonical Url that was passed to generate the original PageMetaData.</param>
        /// <returns></returns>
        Task<Result<PageMetaData>> MapAndGetPageMetadataReusableContent(IContentItemFieldsSource contentItemFieldSource, PageMetaData baseMetaData, string? canonicalUrl);
    }
}
