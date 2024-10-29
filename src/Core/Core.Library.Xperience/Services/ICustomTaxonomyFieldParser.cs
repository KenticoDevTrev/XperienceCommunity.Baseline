using static Core.Models.ContentItemTaxonomyOptions;

namespace Core.Services
{
    public interface ICustomTaxonomyFieldParser
    {
        Task<IEnumerable<ObjectIdentity>> GetTagIdentities(IContentQueryDataContainer dataContainer, TaxonomyFieldIdentifier fieldIdentifier);
    }
}
