namespace Navigation.Features.PartialNavigation
{
    public class NavigationPageTemplateFilter : PageTypePageTemplateFilter
    {
        public override string PageTypeClassName => "Generic.Navigation";
    }

    public class NavigationPageTemplateProperties : IPageTemplateProperties
    {
        // Custom Properties here
    }
}
