namespace Core.Models
{
    public record AssetFieldIdentifierWithType : AssetFieldIdentifier
    {
        public AssetFieldIdentifierWithType(ContentItemAssetMediaType mediaType, string assetFieldName, Guid fieldGuid, string titleFieldName, string? descriptionFieldName = null) : base(assetFieldName, fieldGuid, titleFieldName, descriptionFieldName)
        {
            MediaType = mediaType;
        }

        public ContentItemAssetMediaType MediaType { get; }
    }
}
