namespace Core.Models
{
    public record CategoryItem : IObjectIdentifiable
    {
        [Obsolete("Use other constructor, if KX13 then set the categoryTypeID to the CategoryParentID, and optionally also set the CategoryParentID")]
        public CategoryItem(int categoryID, Guid categoryGuid, string categoryName, int categoryParentID, string categoryDisplayName)
        {
            CategoryID = categoryID;
            CategoryGuid = categoryGuid;
            CategoryName = categoryName;
            CategoryParentID = categoryParentID;
            CategoryTypeID = categoryParentID;
            CategoryDisplayName = categoryDisplayName;
        }

        public CategoryItem(int categoryID, Guid categoryGuid, string categoryName, string categoryDisplayName, int categoryTypeID, int? categoryParentID = null)
        {
            CategoryID = categoryID;
            CategoryGuid = categoryGuid;
            CategoryName = categoryName;
            CategoryParentID = categoryParentID.AsMaybe();
            CategoryTypeID = categoryTypeID;
            CategoryDisplayName = categoryDisplayName;
        }

        public int CategoryID { get; init; }

        /// <summary>
        /// The Category Parent ID, this could be NULL for XbyK
        /// </summary>
        public Maybe<int> CategoryParentID { get; init; }
        
        /// <summary>
        /// The Type of category it is, in KX13 this is the same as the CategoryParentID, in XbyK it's the TaxonomyID
        /// </summary>
        public int CategoryTypeID { get; init; }
        
        public Guid CategoryGuid { get; init; }
        
        public string CategoryName { get; init; }

        public string CategoryDisplayName { get; init; }
        
        public Maybe<string> CategoryDescription { get; init; }
      
        public static CategoryItem UnfoundCategoryItem()
        {
            return new CategoryItem(0, Guid.Empty, "NoCategoryFound", "No Category Found", 0);
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
