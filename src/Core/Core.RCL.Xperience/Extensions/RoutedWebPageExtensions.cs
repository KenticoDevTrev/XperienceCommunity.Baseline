using Core.Models;
using Kentico.Content.Web.Mvc;

namespace Core.Extensions
{
    public static class RoutedWebPageExtensions
    {
        [Obsolete("Not Enough information in XbyK to do a direct cast, please use IPageContextRepository.GetPageAsync(RoutedWebPage.ToTreeCultureIdentity())")]
        public static PageIdentity<TPage> ToPageIdentity<TPage>(this RoutedWebPage page)
        {
            throw new NotImplementedException("Not Enough information in XbyK to do a direct cast, please use IPageContextRepository.GetPageAsync(RoutedWebPage.ToTreeCultureIdentity())");
        }

        public static TreeCultureIdentity ToTreeCultureIdentity(this RoutedWebPage page)
        {
            return new TreeCultureIdentity(page.LanguageName) {
                PageID = page.WebPageItemID,
                PageGuid = page.WebPageItemGUID,
                // Sad day, they aren't passing this so if used, will need hydrator always.
                //PathChannelLookup = new PathChannel(string.Empty, page.WebsiteChannelID),
            };
        }

        public static TreeIdentity ToTreeIdentity(this RoutedWebPage page)
        {
            return new TreeIdentity() {
                PageID = page.WebPageItemID,
                PageGuid = page.WebPageItemGUID,
                //PathChannelLookup = new PathChannel(string.Empty, page.WebsiteChannelID),
            };
        }
    }
}
