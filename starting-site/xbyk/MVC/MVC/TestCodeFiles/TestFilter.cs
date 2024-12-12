using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace MVC.NewFolder
{
    public class PictureContentItemsFilter : IReusableFieldSchemasFilter
    {
        public IEnumerable<string> AllowedSchemaNames => [""];
    }
}
