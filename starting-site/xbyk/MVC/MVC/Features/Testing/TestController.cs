using CMS.ContentEngine;
using CMS.Websites;
using Generic;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Data;
using Testing;
using XperienceCommunity.Authorization;
using XperienceCommunity.MemberRoles;
using XperienceCommunity.MemberRoles.Models;
using XperienceCommunity.MemberRoles.Repositories;
namespace MVC.Features.Testing
{
    public class TestController(
        IContentQueryExecutor contentQueryExecutor,
        IMemberAuthorizationFilter memberAuthorizationFilter,
        IContentQueryResultMapper contentQueryResultMapper,
        IMemberAuthenticationContext memberAuthenticationContext,
        IRoleStore<TagApplicationUserRole> roleStore,
        IUserRoleStore<ApplicationUserBaseline> userRoleStore,
        IUserStore<ApplicationUserBaseline> userStore,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUserBaseline> userManager
        ) : Controller
    {
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly IMemberAuthorizationFilter _memberAuthorizationFilter = memberAuthorizationFilter;
        private readonly IContentQueryResultMapper _contentQueryResultMapper = contentQueryResultMapper;
        private readonly IMemberAuthenticationContext _memberAuthenticationContext = memberAuthenticationContext;
        private readonly IRoleStore<TagApplicationUserRole> _roleStore = roleStore;
        private readonly IUserRoleStore<ApplicationUserBaseline> _userRoleStore = userRoleStore;
        private readonly IUserStore<ApplicationUserBaseline> _userStore = userStore;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        
        private readonly UserManager<ApplicationUserBaseline> _userManager = userManager;

        
        public async Task<string> Index()
        {

            
            var username = "public";
            var authenticated = false;
            var roles = new List<string>();

            var context = _httpContextAccessor.HttpContext;
            if (context is not null) {
                var identity = context.User.Identities.FirstOrDefault();
                if (identity is not null && identity.Name is not null) {
                    username = identity.Name;
                    authenticated = identity.IsAuthenticated;
                }
                var user = (await _userManager.GetUserAsync(context.User));
                if (user != null) {
                    roles.AddRange((await _userRoleStore.GetRolesAsync(user, CancellationToken.None)).Select(x => x.ToLowerInvariant()));
                }
            }

            // Just double checking for public to set not authenticated, roles may still apply if there is customization to set roles on the public user i suppose
            if (username.Equals("public", StringComparison.OrdinalIgnoreCase)) {
                authenticated = false;
            }
            return string.Empty;
        }


        [ControllerActionAuthorization(AuthorizationType.ByAuthenticated)]
        public async Task<string> MemberRoleTest()
        {

            var testMember = await _userStore.FindByNameAsync("TestMember", CancellationToken.None);
            var roles = await _userRoleStore.GetRolesAsync(testMember, CancellationToken.None);
            var studentRoleStatus = await _roleStore.CreateAsync(new TagApplicationUserRole() {
                Name = "TestNewRole",
                NormalizedName = "testnewrole"
            }, CancellationToken.None);

            if (studentRoleStatus.Succeeded) {
                var newRole = await _roleStore.FindByNameAsync("TestNewRole", CancellationToken.None);
                if (newRole != null) {
                    await _userRoleStore.AddToRoleAsync(testMember, "students", CancellationToken.None);
                }
            } else {
                var newRole = await _roleStore.FindByNameAsync("TestNewRole", CancellationToken.None);
                if (newRole != null) {
                    await _userRoleStore.AddToRoleAsync(testMember, newRole.Name, CancellationToken.None);
                }
            }

            var currentUser = await _memberAuthenticationContext.GetAuthenticationContext();

            // For single type query, use the .IncludeMemberAuthorization() to ensure the columns are returned that are needed for parsing.
            var singleTypeQuery = new ContentItemQueryBuilder().ForContentType(BasicPage.CONTENT_TYPE_NAME, query => query.Columns(nameof(BasicPage.PageName))
            .IncludeMemberAuthorization()
            );
            var itemsSingle = await _contentQueryExecutor.GetMappedWebPageResult<BasicPage>(singleTypeQuery);


            // For Multi Type Querys, the Reusable Field Schema is usually returned in the data anyway
            var multiTypeQuery = new ContentItemQueryBuilder().ForContentTypes(parameters =>
                parameters
                    .OfContentType(BasicPage.CONTENT_TYPE_NAME, WebPage.CONTENT_TYPE_NAME, Navigation.CONTENT_TYPE_NAME)
                    .WithContentTypeFields()
                );
            var itemsMultiType = await _contentQueryExecutor.GetResult<object>(multiTypeQuery, (selector) => {
                // Important to use the _ContentQueryResultMapper to map your items so the IMemberAuthorizationFilter.RemoveUnauthorizedItems can do type checking
                if (selector.ContentTypeName.Equals(BasicPage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
                    return _contentQueryResultMapper.Map<BasicPage>(selector);
                } else if (selector.ContentTypeName.Equals(WebPage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
                    return _contentQueryResultMapper.Map<WebPage>(selector);
                }
                return selector as IContentItemFieldsSource;
            });

            var result = $"Before Filter: \n\r\n\r   -{string.Join("\n\r   -", itemsSingle.Select(x => x.PageName))}\n\r";
            var filteredItemsSingle = await _memberAuthorizationFilter.RemoveUnauthorizedItems(itemsSingle, true, ["teachers"]);
            result += $"\n\r\n\rAfter Filter: \n\r\n\r   -{string.Join("\n\r   -", filteredItemsSingle.Select(x => x.PageName))}\n\r";

            result += $"\n\r\n\rBefore Filter Multi: \n\r\n\r   -{string.Join("\n\r   -", itemsMultiType.Select(x => ((IContentItemFieldsSource)x).SystemFields.ContentItemName))}\n\r";
            var filteredItemsMulti = await _memberAuthorizationFilter.RemoveUnauthorizedItems(itemsMultiType, true, ["teachers"]);
            result += $"\n\r\n\rAfter Filter Multi: \n\r\n\r   -{string.Join("\n\r   -", filteredItemsMulti.Select(x => ((IContentItemFieldsSource)x).SystemFields.ContentItemName))}\n\r";


            return result;

        }

    }
}
