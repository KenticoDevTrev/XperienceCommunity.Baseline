using CMS.ContentEngine;
using CMS.Websites;
using Core.Interfaces;
using Core.Repositories;
using Core.Services;
using Generic;
using Kentico.Membership;
using Microsoft.AspNetCore.Identity;
using Testing;
using XperienceCommunity;
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
        IUserRoleStore<ApplicationUser> userRoleStore,
        IUserStore<ApplicationUser> userStore
        ) : Controller
    {
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly IMemberAuthorizationFilter _memberAuthorizationFilter = memberAuthorizationFilter;
        private readonly IContentQueryResultMapper _contentQueryResultMapper = contentQueryResultMapper;
        private readonly IMemberAuthenticationContext _memberAuthenticationContext = memberAuthenticationContext;
        private readonly IRoleStore<TagApplicationUserRole> _roleStore = roleStore;
        private readonly IUserRoleStore<ApplicationUser> _userRoleStore = userRoleStore;
        private readonly IUserStore<ApplicationUser> _userStore = userStore;


        [ControllerActionAuthorization(AuthorizationType.ByAuthenticated)]
        public async Task<string> Index()
        {
            var testMember = await _userStore.FindByNameAsync("TestMember", CancellationToken.None);
            var roles = await _userRoleStore.GetRolesAsync(testMember, CancellationToken.None);
            var studentRoleStatus = await _roleStore.CreateAsync(new TagApplicationUserRole() {
                Name = "TestNewRole",
                NormalizedName = "testnewrole"
            }, CancellationToken.None);

            if(studentRoleStatus.Succeeded) {
                var newRole = await _roleStore.FindByNameAsync("TestNewRole", CancellationToken.None);
                if(newRole != null) {
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
                } else if(selector.ContentTypeName.Equals(WebPage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase)) {
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
