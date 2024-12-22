using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using Microsoft.Extensions.DependencyInjection;
using Navigation.Library;
using RelationshipsExtended;
using XperienceCommunity.QueryExtensions.ContentItems;
using NavigationPageType = Generic.Navigation;

[assembly: CMS.RegisterModule(typeof(NavigationLoaderModule))]

namespace Navigation.Library
{
    public class NavigationLoaderModule : Module
    {
        private IContentQueryExecutor? _contentQueryExecutor;

        // Module class constructor, the system registers the module under the name "CustomInit"
        public NavigationLoaderModule()
            : base("NavigationLoaderModule")
        {
        }

        // Contains initialization code that is executed when the application starts
        protected override void OnInit(ModuleInitParameters parameters)
        {
            base.OnInit();

            _contentQueryExecutor = parameters.Services.GetRequiredService<IContentQueryExecutor>();
            // Clear navigation either if Navigation Category touched
            ContentItemCategoryInfo.TYPEINFO.Events.Insert.After += CategoryNavigationCacheClear;
            ContentItemCategoryInfo.TYPEINFO.Events.Update.After += CategoryNavigationCacheClear;
            ContentItemCategoryInfo.TYPEINFO.Events.Delete.Before += CategoryNavigationCacheClear;

            // Or clear navigation if page is updated that is attached to a navigation item
            ContentItemEvents.UpdateLanguageMetadata.After += ContentItemLanguageNavigationCacheClear;
        }


        private void ContentItemLanguageNavigationCacheClear(object? sender, UpdateContentItemLanguageMetadataEventArgs e)
        {
            try
            {
                var query = $@"SELECT NavigationWebPageItemGuid FROM [Baseline_v2_XbyK].[dbo].[Generic_Navigation]
  inner join CMS_WebPageItem on WebPageItemGUID = NavigationWebPageItemGuid
  where WebPageItemContentItemID = {e.ID}";
                
                if (ConnectionHelper.ExecuteQuery(query, [], QueryTypeEnum.SQLQuery).Tables[0].Rows.Count > 0)
                {
                    CacheHelper.EnsureDummyKey("CustomNavigationClearKey");
                    CacheHelper.TouchKey("CustomNavigationClearKey");
                }
            }
            catch (Exception) { }
        }

        private void CategoryNavigationCacheClear(object? sender, ObjectEventArgs e)
        {
            try
            {
                var category = (ContentItemCategoryInfo)e.Object;

                var queryBuilder = new ContentItemQueryBuilder().ForContentType(NavigationPageType.CONTENT_TYPE_NAME, query => query.Columns([nameof(ContentItemFields.ContentItemID)]).WhereContentItemIDEquals(category.ContentItemCategoryContentItemID));

                if(_contentQueryExecutor != null) {
                    var results = _contentQueryExecutor.GetMappedWebPageResult<NavigationPageType>(queryBuilder).GetAwaiter().GetResult();
                    if(results.Any()) {
                        CacheHelper.EnsureDummyKey("CustomNavigationClearKey");
                        CacheHelper.TouchKey("CustomNavigationClearKey");
                    }
                }
            }
            catch (Exception) { }
        }
    }
}