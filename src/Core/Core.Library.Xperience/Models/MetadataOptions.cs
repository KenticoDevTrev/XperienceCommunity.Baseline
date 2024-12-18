namespace Core.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="MaxLinkedLevelsRetrievedForMetadata">When retrieving metadata (which doesn't know which page type you are retrieving), how many levels should linked items be retrieved.  Leave at 2 (The item and related Image Assets) unless you need deeper levels to build the Metadata in your customization of the IWebPageToPageMetadataConverter</param>
    public record MetadataOptions(int MaxLinkedLevelsRetrievedForMetadata = 2);
}
