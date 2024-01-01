namespace TabbedPages.Features.Tab
{
    public class TabPageTemplateFilter : PageTypePageTemplateFilter
    {
        public override string PageTypeClassName => "Generic.Tab";
    }

    public class TabPageTemplateProperties : IPageTemplateProperties
    {
        // Custom Properties here
    }
}