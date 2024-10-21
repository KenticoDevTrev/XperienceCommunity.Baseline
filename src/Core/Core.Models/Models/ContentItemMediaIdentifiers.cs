namespace Core.Models
{
    /// <summary>
    /// Source information for Content Asset Items since they have multiple identifiers used.
    /// </summary>
    /// <param name="ContentItemGuid">The Content Item Guid</param>
    /// <param name="FieldGuid">The Field Guid the Content Asset Item is bound to</param>
    /// <param name="FieldName">The Field Name the Content Asset Item is bound to</param>
    /// <param name="Identifier">The Asset Identifier (in the Asset Metadata)</param>
    /// <param name="Language">The Language</param>
    public record ContentItemMediaIdentifiers(Guid ContentItemGuid, Guid FieldGuid, string FieldName, Guid Identifier, string Language);
}
