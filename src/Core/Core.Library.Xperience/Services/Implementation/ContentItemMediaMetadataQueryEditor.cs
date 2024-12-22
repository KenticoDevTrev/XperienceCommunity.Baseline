namespace Core.Services.Implementation
{
    public class ContentItemMediaMetadataQueryEditor : IContentItemMediaMetadataQueryEditor
    {
        public Task<Result<ContentItemQueryBuilder>> CustomizeMediaQuery(string className, IEnumerable<ContentIdentity> contentItems, IEnumerable<AssetFieldIdentifier> assetFields, string language)
        {
            /* Here's en example of customization */
            /*
            if(className.Equals("custom.pdffile", StringComparison.OrdinalIgnoreCase)) {
                return Task.FromResult(
                    Result.Success(
                        new ContentItemQueryBuilder()
                            .ForContentType(className, query => 
                                // Grab the required fields plus the PdfOwner which will be used in the custom ContentItemMediaMetadataProvider
                                query
                                    .Columns(
                                        assetFields.SelectMany(x => new string[] { x.AssetFieldName, x.TitleFieldName, x.DescriptionFieldName.GetValueOrDefault(string.Empty)})
                                        .Except([string.Empty])
                                        .Union([nameof(ContentItemFields.ContentItemGUID), nameof(ContentItemFields.ContentItemCommonDataContentLanguageID)])
                                        .Union(["PdfOwner"]) // Custom Field we may want for metadata
                                        .Distinct().ToArray())
                                    .InContentIdentities(contentItems) // Only the content items given
                        )
                        .InLanguage(language, useLanguageFallbacks: true);
                     )
                );
            }
            */

            return Task.FromResult(Result.Failure<ContentItemQueryBuilder>("Not customized"));
        }

        public Task<Result<ContentItemQueryBuilder>> CustomizeMediaQueryAllCached(string className, IEnumerable<AssetFieldIdentifier> allAssetFields)
        {
            /* Here's en example of customization */
            /*
            if(className.Equals("custom.pdffile", StringComparison.OrdinalIgnoreCase)) {
                return Task.FromResult(
                    Result.Success(
                        new ContentItemQueryBuilder()
                            .ForContentType(className, query => 
                                // Grab the required fields plus the PdfOwner which will be used in the custom ContentItemMediaMetadataProvider
                                query
                                    .Columns(
                                        assetFields.SelectMany(x => new string[] { x.AssetFieldName, x.TitleFieldName, x.DescriptionFieldName.GetValueOrDefault(string.Empty)})
                                        .Except([string.Empty])
                                        .Union([nameof(ContentItemFields.ContentItemGUID), nameof(ContentItemFields.ContentItemCommonDataContentLanguageID)])
                                        .Union(["PdfOwner"]) // Custom Field we may want for metadata
                                        .Distinct().ToArray())
                        )
                     )
                );
            }
            */

            return Task.FromResult(Result.Failure<ContentItemQueryBuilder>("Not customized"));
        }
    }
}
