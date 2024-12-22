namespace Core.Repositories
{
    public interface IClassContentTypeAssetConfigurationRepository
    {
        Task<Dictionary<string, ContentTypeAssetConfigurations>> GetClassNameToContentTypeAssetConfigurationDictionary();
    }
}
