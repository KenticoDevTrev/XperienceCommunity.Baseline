using CMS.Websites;
using CSharpFunctionalExtensions;
using MVCCaching;
using NavigationPageType = Generic.Navigation;

namespace Navigation.XbyK.Models
{
    /// <summary>
    /// Helper to build out Hierarchy Parent-child relationships without the limit of the CMS.Document only "Children" property of the TreeNode Class
    /// 
    /// This should not be cached as the List and Items are reference typed
    /// </summary>
    public class HierarchyWebPage : ICacheKey
    {

        public HierarchyWebPage(NavigationPageType navigationPageType)
        {
            NavPage = navigationPageType;
        }

        public HierarchyWebPage(IWebPageContentQueryDataContainer otherPage)
        {
            OtherPage = otherPage.AsMaybe();
        }
        public Maybe<NavigationPageType> NavPage { get; set; }
        public Maybe<IWebPageContentQueryDataContainer> OtherPage { get; set; }
        public List<HierarchyWebPage> Children { get; set; } = [];

        public string GetCacheKey()
        {
            return $"ContentItemID_{NavPage.AsNullableValue()?.SystemFields.ContentItemID ?? OtherPage.AsNullableValue()?.ContentItemID ?? 0}";
        }
    }

    
}
