using BaselineSiteElements.Features.BasicPage;

[assembly: RegisterPageTemplate(
    "Generic.BasicPage_Default",
    "Basic Page",
    typeof(BasicPagePageTemplateProperties),
    "~/Features/BasicPage/BasicPagePageTemplate.cshtml")]

namespace BaselineSiteElements.Features.BasicPage
{
    public class BasicPagePageTemplateFilter : PageTypePageTemplateFilter
    {
        public override string PageTypeClassName => "Generic.BasicPage";
    }

    public class BasicPagePageTemplateProperties : IPageTemplateProperties
    {
        // Custom Properties here
    }
}