namespace Localization.Models
{
    // Represented a version of the CategoryItem that has had it's Display Name and possible description localized
    public record LocalizedCategoryItem : CategoryItem
    {
        public LocalizedCategoryItem(int categoryID, Guid categoryGuid, string categoryName, int categoryParentID, string categoryDisplayName) : base(categoryID, categoryGuid, categoryName, categoryParentID, categoryDisplayName)
        {
        }
    }
}
