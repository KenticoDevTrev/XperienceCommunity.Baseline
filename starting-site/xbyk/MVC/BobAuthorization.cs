using CMS.Websites;
using XperienceCommunity.Authorization;

namespace MVC
{
    public class BobAuthorization : IAuthorization
    {
        public Task<bool> IsAuthorizedAsync(UserContext user, AuthorizationConfiguration authConfig, IWebPageFieldsSource currentPage = null, string pageTemplateIdentifier = null)
        {
            // Only Bobs...
            return Task.FromResult(user.UserName.Contains("Bob", StringComparison.OrdinalIgnoreCase));
        }
    }
}
