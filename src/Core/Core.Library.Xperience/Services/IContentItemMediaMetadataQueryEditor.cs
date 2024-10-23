namespace Core.Services
{
    public interface IContentItemMediaMetadataQueryEditor
    {

        /// <summary>
        /// Can be overwritten if you need to retrieve additional fields for your Media IMediaMetadata handling
        /// </summary>
        /// <param name="className">The Class Name for the assets being retrieved, use this to alter behavior</param>
        /// <param name="contentItems">The Content Item that contains the assets being retrieved</param>
        /// <param name="assetFields">The Field names of where the assets are being retrieved.</param>
        /// <param name="language">The Language requested.</param>
        /// <returns>The Content Item Query Builder (if you customzied it), or Result.Failure if you are not customizing it</returns>
        Task<Result<ContentItemQueryBuilder>> CustomizeMediaQuery(string className, IEnumerable<ContentIdentity> contentItems, IEnumerable<AssetFieldIdentifier> assetFields, string language);

        /// <summary>
        /// Can be overwritten if you need to retrieve additional fields for your Media IMediaMetadata handling, this will be fore if ALL the items are to be retrieved and parsed.
        /// </summary>
        /// <param name="className">The Class Name for the assets being retrieved, use this to alter behavior</param>
        /// <param name="allAssetFields">The Field names of all the assets found for this Content Type to be retrieved.</param>
        /// <returns>The Content Item Query Builder (if you customzied it), or Result.Failure if you are not customizing it</returns>
        Task<Result<ContentItemQueryBuilder>> CustomizeMediaQueryAllCached(string className, IEnumerable<AssetFieldIdentifier> allAssetFields);
    }

    /// <summary>
    /// Represents the actual identifiers, with ones not provided in configuration filled in.
    /// </summary>
    public record AssetFieldIdentifier
    {
        public AssetFieldIdentifier(string assetFieldName, Guid fieldGuid, string titleFieldName, string? descriptionFieldName = null)
        {
            AssetFieldName = assetFieldName;
            FieldGuid = fieldGuid;
            TitleFieldName = titleFieldName;
            DescriptionFieldName = descriptionFieldName.AsNullOrWhitespaceMaybe();
        }

        public string AssetFieldName { get; }
        public Guid FieldGuid { get; }
        public string TitleFieldName { get; }
        public Maybe<string> DescriptionFieldName { get; }
        
    }
}
