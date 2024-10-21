using BaselineSiteElements.Features.Home;
using Generic;

[assembly: RegisterPageTemplate(
    "Generic.Home_Default",
    "Home Page (Default)",
    typeof(HomePageTemplateProperties),
    "/Features/Home/HomePageTemplate.cshtml", ContentTypeNames = [Home.CONTENT_TYPE_NAME] )]

namespace BaselineSiteElements.Features.Home
{
    /* TODO: Rebuild this for Sean
     * public class HomePageTemplateFilter : PageTypePageTemplateFilter
    {
        public override string PageTypeClassName => "Generic.Home";
    }*/

    public class HomePageTemplateProperties : IPageTemplateProperties
    {
        // Custom Properties here
    }
}