namespace Core.Enums
{
    public enum TaxonomyFieldIdentifierType
    {
        /// <summary>
        /// Taxonomy typed field, stored as an array of {"Identifier":"TagGuid"}
        /// </summary>
        Taxonomy,
        /// <summary>
        /// Object Code Name typed field, stored as an array of the string code name ["TagName1", "TagName2"]
        /// </summary>
        ObjectCodeNames,
        /// <summary>
        /// Object Global Identifiers typed field, stored as an array of Guid values ["887156fb-f54d-4737-a5dc-8ee59fa7150c","70a69181-1149-4402-b618-71b2c7ed2e73"]
        /// </summary>
        ObjectGlobalIdentities,
        /// <summary>
        /// For Custom types, you will need to implement the ICustomTaxonomyFieldParser.GetTags method and declare
        /// </summary>
        Custom
    }
}
