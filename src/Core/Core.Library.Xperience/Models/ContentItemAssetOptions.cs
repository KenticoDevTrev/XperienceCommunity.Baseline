namespace Core.Models
{
    public class ContentItemAssetOptions
    {
        public ContentItemAssetOptions() { 
            
        }

        /// <summary>
        /// Provides configurations of your content types and their Content Item Asset fields, used when generating Media items
        /// </summary>
        public List<ContentItemWithAssetsConfiguration> ContentItemConfigurations { get; set; } = [];

        /// <summary>
        /// Configuration for a specific Content Type, since it may contain multiple Content Asset Fields, has a list of them.
        /// </summary>
        /// <param name="ClassName">The Content Type CodeName</param>
        /// <param name="AssetFieldIdentifierConfigurations">The list of Asset Field Configurations</param>
        /// <param name="PreCache">If true, will generate and cache all the Media Items for this Class and use that instead of individual lookups.  Has logic to switch to individual lookup if the content items are being updated frequently.</param>
        public record ContentItemWithAssetsConfiguration(string ClassName, List<AssetFieldIdentifierConfiguration> AssetFieldIdentifierConfigurations, bool PreCache = false);

        /// <summary>
        /// Used by editors to configure the asset identifiers.  Title field and Field Guid will be filled in if not provided.
        /// </summary>
        public record AssetFieldIdentifierConfiguration
        {
            /// <summary>
            /// User defined Asset Field Identifiers
            /// </summary>
            /// <param name="assetFieldName">The Field Name (Column Name) for the Content Item Asset field</param>
            /// <param name="titleFieldName">The Field Name (Column Name) for the title value of the Media Item, if not provided will default to the ContentItemLanguageMetadataDisplayName</param>
            /// <param name="descriptionFieldName">The Field Name (Column Name) for the description value of the Media Item, optional as description isn't required.</param>
            public AssetFieldIdentifierConfiguration(string assetFieldName, ContentItemAssetMediaType mediaType, string? titleFieldName = null, string? descriptionFieldName = null)
            {
                AssetFieldName = assetFieldName;
                MediaType = mediaType;
                TitleFieldName = titleFieldName.AsNullOrWhitespaceMaybe();
                DescriptionFieldName = descriptionFieldName.AsNullOrWhitespaceMaybe();
            }

            public string AssetFieldName { get; }
            public Maybe<string> TitleFieldName { get; }
            public Maybe<string> DescriptionFieldName { get; }
            public ContentItemAssetMediaType MediaType { get; }
        }
    }
}
