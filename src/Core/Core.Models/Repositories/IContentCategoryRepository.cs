namespace Core.Repositories
{
    public interface IContentCategoryRepository
    {
        /// <summary>
        /// Gets the Categories associated with the given TreeIdentity.  This will combine all Culture variations and any ContentItemCategories (culture agnostic)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="taxonomyTypes">What Taxonomy types you wish to retrieve by, if empty will return all. XbyK Only</param>
        /// <returns></returns>
        Task<IEnumerable<CategoryItem>> GetCategoriesByTreeIdentity(TreeIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null);

        /// <summary>
        /// Gets the Categories associated with the given TreeIdentities.  This will combine all Culture variations and any ContentItemCategories (culture agnostic)
        /// </summary>
        /// <param name="identities"></param>
        /// <param name="taxonomyTypes">What Taxonomy types you wish to retrieve by, if empty will return all. XbyK Only</param>
        /// <returns></returns>
        Task<IEnumerable<CategoryItem>> GetCategoriesByTreeIdentities(IEnumerable<TreeIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null);

        /// <summary>
        /// Gets the Categories associated with the given TreeCultureIdentity.  This will combine the specified culture variant and any ContentItemCategories (culture agnostic)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="taxonomyTypes">What Taxonomy types you wish to retrieve by, if empty will return all. XbyK Only</param>
        /// <returns></returns>
        Task<IEnumerable<CategoryItem>> GetCategoriesByTreeCultureIdentity(TreeCultureIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null);

        /// <summary>
        /// Gets the Categories associated with the given TreeCultureIdentities.  This will combine the specified culture variants and any ContentItemCategories (culture agnostic)
        /// </summary>
        /// <param name="identities"></param>
        /// <param name="taxonomyTypes">What Taxonomy types you wish to retrieve by, if empty will return all. XbyK Only</param>
        /// <returns></returns>
        Task<IEnumerable<CategoryItem>> GetCategoriesByTreeCultureIdentities(IEnumerable<TreeCultureIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null);

        /// <summary>
        /// Gets the Categories associated with the given ContentIdentity.  This will combine all Culture variations and any ContentItemCategories (culture agnostic)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="taxonomyTypes">What Taxonomy types you wish to retrieve by, if empty will return all. XbyK Only</param>
        /// <returns></returns>
        Task<IEnumerable<CategoryItem>> GetCategoriesByContentIdentity(ContentIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null);

        /// <summary>
        /// Gets the Categories associated with the given ContentIdentities.  This will combine all Culture variations and any ContentItemCategories (culture agnostic)
        /// </summary>
        /// <param name="identities"></param>
        /// <param name="taxonomyTypes">What Taxonomy types you wish to retrieve by, if empty will return all. XbyK Only</param>
        /// <returns></returns>
        Task<IEnumerable<CategoryItem>> GetCategoriesByContentIdentities(IEnumerable<ContentIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null);

        /// <summary>
        /// Gets the Categories associated with the given ContentCultureIdentity.  This will combine the specified culture variant and any ContentItemCategories (culture agnostic)
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="taxonomyTypes">What Taxonomy types you wish to retrieve by, if empty will return all. XbyK Only</param>
        /// <returns></returns>
        Task<IEnumerable<CategoryItem>> GetCategoriesByContentCultureIdentity(ContentCultureIdentity identity, IEnumerable<ObjectIdentity>? taxonomyTypes = null);

        /// <summary>
        /// Gets the Categories associated with the given ContentCultureIdentities.  This will combine the specified culture variants and any ContentItemCategories (culture agnostic)
        /// </summary>
        /// <param name="identities"></param>
        /// <param name="taxonomyTypes">What Taxonomy types you wish to retrieve by, if empty will return all. XbyK Only</param>
        /// <returns></returns>
        Task<IEnumerable<CategoryItem>> GetCategoriesByContentCultureIdentities(IEnumerable<ContentCultureIdentity> identities, IEnumerable<ObjectIdentity>? taxonomyTypes = null);

    }

    
}
