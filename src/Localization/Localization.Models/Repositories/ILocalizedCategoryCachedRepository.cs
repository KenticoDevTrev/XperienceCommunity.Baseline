namespace Localization.Repositories
{
    public interface ILocalizedCategoryCachedRepository
    {
        /// <summary>
        /// Original implementation, will not handle language fallback chain for Xperience by Kentico, fine for KX13
        /// </summary>
        /// <param name="categoryItem"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        [Obsolete("Use LocalizeCategoryItemAsync")]
        LocalizedCategoryItem LocalizeCategoryItem(CategoryItem categoryItem, string cultureCode);

        /// <summary>
        /// Original implementation, will not handle language fallback chain for Xperience by Kentico, fine for KX13
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        [Obsolete("Use LocalizeCategoryItemAsync")]
        IEnumerable<LocalizedCategoryItem> LocalizeCategoryItems(IEnumerable<CategoryItem> categories, string cultureCode);

        /// <summary>
        /// Converts the Category Item to a LocalizedCategoryItem with the proper title and description localized.
        /// </summary>
        /// <param name="categoryItem">The Category Item to translate</param>
        /// <param name="cultureCode">The Culture Code (will use System.Globalization.CultureInfo.CurrentCulture)</param>
        /// <returns></returns>
        Task<LocalizedCategoryItem> LocalizeCategoryItemAsync(CategoryItem categoryItem, string? cultureCode = null);

        /// <summary>
        /// Converts the Category Item to a LocalizedCategoryItem with the proper title and description localized.
        /// </summary>
        /// <param name="categories">The Categories to translate</param>
        /// <param name="cultureCode">The Culture Code (will use System.Globalization.CultureInfo.CurrentCulture)</param>
        /// <returns></returns>
        Task<IEnumerable<LocalizedCategoryItem>> LocalizeCategoryItemsAsync(IEnumerable<CategoryItem> categories, string? cultureCode = null);
    }
}
