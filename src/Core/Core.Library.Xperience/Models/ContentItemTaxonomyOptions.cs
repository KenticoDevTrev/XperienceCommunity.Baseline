namespace Core.Models
{
    public class ContentItemTaxonomyOptions
    {
        public ContentItemTaxonomyOptions() { 
            
        }

        /// <summary>
        /// Provides configurations of your content types and their Content Item Taxonomy fields, used when generating Category Items for the ContentCategoryRepository
        /// </summary>
        public List<ContentItemWithTaxonomysConfiguration> ContentItemConfigurations { get; set; } = new List<ContentItemWithTaxonomysConfiguration>();

        /// <summary>
        /// Configuration for a specific Content Type, since it may contain multiple Content Taxonomy Fields, has a list of them.
        /// </summary>
        /// <param name="ClassName">The Content Type CodeName</param>
        /// <param name="TaxonomyFieldIdentifier">The list of Taxonomy Field Configurations</param>
        /// <param name="PreCache">If true, will generate and cache all the Taxonomy Identifiers for this Class and use that instead of individual lookups.  Has logic to switch to individual lookup if the content items are being updated frequently.</param>
        public record ContentItemWithTaxonomysConfiguration(string ClassName, List<TaxonomyFieldIdentifier> TaxonomyFieldIdentifier, bool PreCache = false);

        /// <summary>
        /// Used by editors to configure the taxonomy identifiers.  Title field and Field Guid will be filled in if not provided.
        /// </summary>
        public record TaxonomyFieldIdentifier
        {
            /// <summary>
            /// User defined Taxonomy Field Identifiers
            /// </summary>
            /// <param name="taxonomyFieldName">The Field Name (Column Name) for the Content Item Taxonomy field</param>
            /// <param name="taxonomyFieldType">The type the field is, determines how it's content will be parsed.</param>
            public TaxonomyFieldIdentifier(string taxonomyFieldName, TaxonomyFieldIdentifierType taxonomyFieldType)
            {
                TaxonomyFieldName = taxonomyFieldName;
                TaxonomyFieldType = taxonomyFieldType;
            }

            public string TaxonomyFieldName { get; }
            public TaxonomyFieldIdentifierType TaxonomyFieldType { get; }
        }
    }
}
