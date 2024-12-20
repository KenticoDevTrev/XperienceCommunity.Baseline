using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Navigation.Models;
using Navigation.Repositories;
using System.Data;
using Testing;
using XperienceCommunity.Authorization;
using XperienceCommunity.MemberRoles.Models;
using XperienceCommunity.MemberRoles.Repositories;
using XperienceCommunity.MemberRoles.Services;

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
        UserManager<ApplicationUserBaseline> userManager,
        INavigationRepository navigationRepository,
        IBreadcrumbRepository breadcrumbRepository,
        IInfoProvider<TagInfo> tagInfoProvider,
        ISiteMapRepository siteMapRepository,
        IStringLocalizer<SharedResources> stringLocalizer
        ) : Controller
    {
        private readonly IContentQueryExecutor _contentQueryExecutor = contentQueryExecutor;
        private readonly IMemberAuthorizationFilter _memberAuthorizationFilter = memberAuthorizationFilter;
        private readonly IContentQueryResultMapper _contentQueryResultMapper = contentQueryResultMapper;
        private readonly IMemberAuthenticationContext _memberAuthenticationContext = memberAuthenticationContext;
        private readonly IRoleStore<TagApplicationUserRole> _roleStore = roleStore;
        private readonly IUserRoleStore<ApplicationUserBaseline> _userRoleStore = userRoleStore;
        private readonly IUserStore<ApplicationUserBaseline> _userStore = userStore;
        
        private readonly UserManager<ApplicationUserBaseline> _userManager = userManager;
        private readonly INavigationRepository _navigationRepository = navigationRepository;
        private readonly IBreadcrumbRepository _breadcrumbRepository = breadcrumbRepository;
        private readonly IInfoProvider<TagInfo> _tagInfoProvider = tagInfoProvider;
        private readonly ISiteMapRepository _siteMapRepository = siteMapRepository;
        private readonly IStringLocalizer<SharedResources> _stringLocalizer = stringLocalizer;

        public async Task<string> Index()
        {
            var translated = _stringLocalizer.GetString("test.localize");

            // test secondary navigation
            var test = await _navigationRepository.GetSecondaryNavItemsAsync("/MPTest-FullAccess/MPTest-BreakInheritance", Navigation.Enums.PathSelectionEnum.ParentAndChildren);

            var test2 = await _breadcrumbRepository.GetBreadcrumbsAsync(13.ToTreeIdentity(), true);
            var jsonLd = await _breadcrumbRepository.BreadcrumbsToJsonLDAsync(test2);

            var sitemap = await _siteMapRepository.GetSiteMapUrlSetAsync();

            var sitemapText = SitemapNode.GetSitemap(sitemap);
            return sitemapText;
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
            var singleTypeQuery = new ContentItemQueryBuilder().ForContentType(BasicPage.CONTENT_TYPE_NAME, query => query.Columns(nameof(BasicPage.MetaData_PageName))
            .IncludeMemberAuthorization()
            );
            var itemsSingle = await _contentQueryExecutor.GetMappedWebPageResult<BasicPage>(singleTypeQuery);


            // For Multi Type Querys, the Reusable Field Schema is usually returned in the data anyway
            var multiTypeQuery = new ContentItemQueryBuilder().ForContentTypes(parameters =>
                parameters
                    .OfContentType(BasicPage.CONTENT_TYPE_NAME, WebPage.CONTENT_TYPE_NAME, Generic.Navigation.CONTENT_TYPE_NAME)
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

            var result = $"Before Filter: \n\r\n\r   -{string.Join("\n\r   -", itemsSingle.Select(x => x.MetaData_PageName))}\n\r";
            var filteredItemsSingle = await _memberAuthorizationFilter.RemoveUnauthorizedItems(itemsSingle, true, ["teachers"]);
            result += $"\n\r\n\rAfter Filter: \n\r\n\r   -{string.Join("\n\r   -", filteredItemsSingle.Select(x => x.MetaData_PageName))}\n\r";

            result += $"\n\r\n\rBefore Filter Multi: \n\r\n\r   -{string.Join("\n\r   -", itemsMultiType.Select(x => ((IContentItemFieldsSource)x).SystemFields.ContentItemName))}\n\r";
            var filteredItemsMulti = await _memberAuthorizationFilter.RemoveUnauthorizedItems(itemsMultiType, true, ["teachers"]);
            result += $"\n\r\n\rAfter Filter Multi: \n\r\n\r   -{string.Join("\n\r   -", filteredItemsMulti.Select(x => ((IContentItemFieldsSource)x).SystemFields.ContentItemName))}\n\r";


            return result;

        }

    }
}
