namespace Core.Repositories.Implementation
{
    public class CustomTaxonomyFieldParser : ICustomTaxonomyFieldParser
    {
        public Task<IEnumerable<ObjectIdentity>> GetTagIdentities(IContentQueryDataContainer dataContainer, ContentItemTaxonomyOptions.TaxonomyFieldIdentifier fieldIdentifier)
        {
            return Task.FromResult((IEnumerable<ObjectIdentity>)[]);
        }
    }
}
