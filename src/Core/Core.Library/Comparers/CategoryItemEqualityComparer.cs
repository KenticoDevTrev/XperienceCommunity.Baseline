namespace Core
{

    public class CategoryItemEqualityComparer : IEqualityComparer<CategoryItem>
    {
        public bool Equals(CategoryItem? x, CategoryItem? y)
        {
            if (x == null && y == null) {
                return true;
            }
            if (x == null || y == null) {
                return false;
            }
            if (
                (x.CategoryID == y.CategoryID)
                ||
                (x.CategoryName.Equals(y.CategoryName, StringComparison.OrdinalIgnoreCase))
                ||
                (x.CategoryGuid.Equals(y.CategoryGuid))
                )
                {
                return true;
            }
            return false;
        }

        public int GetHashCode(CategoryItem obj)
        {
            return obj.CategoryGuid.GetHashCode();
        }
    }
}
