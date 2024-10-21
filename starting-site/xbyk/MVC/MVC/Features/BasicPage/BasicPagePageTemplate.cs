using BaselineSiteElements.Features.BasicPage;
using Generic;

[assembly: RegisterPageTemplate(
    "Generic.BasicPage_Default",
    "Basic Page",
    typeof(BasicPagePageTemplateProperties),
    "~/Features/BasicPage/BasicPagePageTemplate.cshtml",
    ContentTypeNames = [BasicPage.CONTENT_TYPE_NAME])]

namespace BaselineSiteElements.Features.BasicPage
{
    public class BasicPagePageTemplateProperties : IPageTemplateProperties
    {
        // Custom Properties here
    }
}