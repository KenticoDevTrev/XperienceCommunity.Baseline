namespace Core.Models
{
    public record CategoryItem : IObjectIdentifiable
    {
        public CategoryItem(int categoryID, Guid categoryGuid, string categoryName, int categoryParentID, string categoryDisplayName)
        {
            CategoryID = categoryID;
            CategoryGuid = categoryGuid;
            CategoryName = categoryName;
            CategoryParentID = categoryParentID;
            CategoryDisplayName = categoryDisplayName;
        }

        public int CategoryID { get; init; }
        public int CategoryParentID { get; init; }
        public Guid CategoryGuid { get; init; }
        public string CategoryName { get; init; }

        public string CategoryDisplayName { get; init; }
        public Maybe<string> CategoryDescription { get; init; }
      
        public static CategoryItem UnfoundCategoryItem()
        {
            return new CategoryItem(0, Guid.Empty, "NoCategoryFound", 0, "No Category Found");
        }

        public ObjectIdentity ToObjectIdentity()
        {
            return new ObjectIdentity()
            {
                Id = CategoryID,
                Guid = CategoryGuid,
                CodeName = CategoryName
            };
        }
    }
}
