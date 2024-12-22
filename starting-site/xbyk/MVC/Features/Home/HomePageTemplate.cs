using BaselineSiteElements.Features.Home;
using Generic;
using XperienceCommunity.Authorization;
[assembly: RegisterPageBuilderAuthorization(PageBuilderAuthorizationTypes.ByPageTemplate, "Generic.Home_", AuthorizationType.ByPageACL, templateIdentifiersArePrefix: true)]
[assembly: RegisterPageTemplate(
    "Generic.Home_Default",
    "Home Page (Default)",
    typeof(HomePageTemplateProperties),
    "/Features/Home/HomePageTemplate.cshtml", ContentTypeNames = [Home.CONTENT_TYPE_NAME] )]

namespace BaselineSiteElements.Features.Home
{
    public class HomePageTemplateProperties : IPageTemplateProperties
    {
        // Custom Properties here
    }
}