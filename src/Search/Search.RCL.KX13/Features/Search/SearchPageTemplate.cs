namespace Search.Features.Search
{
    public class SearchPageTemplateFilter : PageTypePageTemplateFilter
    {
        public override string PageTypeClassName => "Generic.Search";
    }

    public class SearchPageTemplateProperties : IPageTemplateProperties
    {
        // Custom Properties here
    }
}