
using System.Data;

namespace Core.Repositories.Implementation
{
    public class ContentTranslationInformationRepository(IProgressiveCache progressiveCache,
        ILanguageRepository languageRepository) : IContentTranslationInformationRepository
    {
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly ILanguageRepository _languageRepository = languageRepository;

        public async Task<IEnumerable<ContentTranslationSummary>> GetContentItemTranslationSummaries(int contentItemID, int? channelID = null)
        {
            return (await GetContentTranslationSummaryDictionary(channelID)).TryGetValue(contentItemID, out var translationSummaries) ? translationSummaries : [];
        }

        private async Task<Dictionary<int, IEnumerable<ContentTranslationSummary>>> GetContentTranslationSummaryDictionary(int? channelID = null)
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([
                        $"contentitem|all",
                        $"{ContentLanguageInfo.OBJECT_TYPE}|all"
                        ]);
                }
                var query = @$"SELECT ContentItemID
	  ,ContentItemLanguageMetadataID
	  ,ContentItemCommonDataID
	  ,ContentLanguageID
      ,case when ContentItemLanguageMetadataID is not null then 1 else 0 end as [TranslationExists]
      ,ContentLanguageIsDefault as [IsDefaultLanguage]
  FROM CMS_ContentItem
  inner join CMS_ContentLanguage on 1=1
  left join CMS_ContentItemLanguageMetadata on ContentItemID = ContentItemLanguageMetadataContentItemID and ContentItemLanguageMetadataContentLanguageID = ContentLanguageID
  left join CMS_ContentItemCommonData on  ContentItemID = ContentItemCommonDataContentItemID and ContentItemCommonDataContentLanguageID = ContentLanguageID
  where COALESCE(ContentItemCommonDataVersionStatus, 2) = 2";
                var queryParams = new QueryDataParameters();
                if (channelID.AsMaybe().TryGetValue(out var channelIDVal)) {
                    queryParams.Add("@ChannelID", channelIDVal);
                    query += " and COALESCE(ContentItemChannelID, @ChannelID) = @ChannelID";
                }

                return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, queryParams, QueryTypeEnum.SQLQuery))
                .Tables[0].Rows.Cast<DataRow>().Select(x => new ContentTranslationSummary(
                 ContentItemID: (int)x["ContentItemID"],
                 ContentItemLanguageMetadataID: x.Field<int?>("ContentItemLanguageMetadataID").AsMaybeStatic(),
                 ContentItemCommonDataID: x.Field<int?>("ContentItemCommonDataID").AsMaybeStatic(),
                 LanguageName: _languageRepository.LanguageIdToName((int)x["ContentLanguageId"]),
                 TranslationExists: ((int)x["TranslationExists"]) == 1,
                 IsDefaultLanguage: ((int)x["IsDefaultLanguage"]) == 1
                ))
                .GroupBy(key => key.ContentItemID)
                .ToDictionary(key => key.Key, value => (IEnumerable<ContentTranslationSummary>) value.OrderByDescending(x => x.TranslationExists).ThenByDescending(x => x.IsDefaultLanguage));

            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "ContentTranslationSummaryDictionary", channelID ?? 0));
        }

        public async Task<IEnumerable<WebpageTranslationSummary>> GetWebpageTranslationSummaries(int webPageItemID, int websiteChannelID)
        {
            return (await GetWebpageTranslationSummaryDictionary(websiteChannelID)).TryGetValue(webPageItemID, out var summary) ? summary : [];
        }

        private async Task<Dictionary<int, IEnumerable<WebpageTranslationSummary>>> GetWebpageTranslationSummaryDictionary(int websiteChannelID)
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([
                        $"webpageitem|all",
                        $"{ContentLanguageInfo.OBJECT_TYPE}|all"
                        ]);
                }
                var query = @$"SELECT WebPageItemID
      ,ContentItemID
	  ,ContentItemLanguageMetadataID
	  ,ContentItemCommonDataID
	  ,WebPageUrlPath
	  ,WebPageItemTreePath
	  ,WebPageUrlPathContentLanguageID
      ,case when ContentItemLanguageMetadataID is not null then 1 else 0 end as [TranslationExists]
      ,case when WebsiteChannelPrimaryContentLanguageID = WebPageUrlPathContentLanguageID then 1 else 0 end as [IsDefaultLanguage]
      ,case when WebsiteChannelHomePage = WebPageItemTreePath then 1 else 0 end as [IsHome]
  FROM [CMS_WebPageUrlPath]
  inner join CMS_WebPageItem on WebPageItemID = WebPageUrlPathWebPageItemID
  inner join CMS_ContentItem on ContentItemID = WebPageItemContentItemID
  inner join CMS_WebsiteChannel on WebsiteChannelID = WebPageItemWebsiteChannelID
  left join CMS_ContentItemLanguageMetadata on ContentItemID = ContentItemLanguageMetadataContentItemID and ContentItemLanguageMetadataContentLanguageID = WebPageUrlPathContentLanguageID
  left join CMS_ContentItemCommonData on  ContentItemID = ContentItemCommonDataContentItemID and ContentItemCommonDataContentLanguageID = WebPageUrlPathContentLanguageID
  where COALESCE(ContentItemCommonDataVersionStatus, 2) = 2
  and WebPageUrlPathIsCanonical = 1  and WebPageUrlPathIsLatest = 1
  and WebPageUrlPathWebsiteChannelID = @WebsiteChannelID";
                
                return (await XperienceCommunityConnectionHelper.ExecuteQueryAsync(query, new QueryDataParameters() { { "@WebsiteChannelID", websiteChannelID } }, QueryTypeEnum.SQLQuery))
                .Tables[0].Rows.Cast<DataRow>().Select(x => {
                    var isHome = (int)x["IsHome"] == 1;
                    var isDefaultLanguage = (int)x["IsDefaultLanguage"] == 1;
                    return new WebpageTranslationSummary(
                     WebPageItemID: (int)x["WebPageItemID"],
                     ContentItemID: (int)x["ContentItemID"],
                     ContentItemLanguageMetadataID: x.Field<int?>("ContentItemLanguageMetadataID").AsMaybeStatic(),
                     ContentItemCommonDataID: x.Field<int?>("ContentItemCommonDataID").AsMaybeStatic(),
                     Url: $"/{(isHome && isDefaultLanguage ? "" : ((string)x["WebPageUrlPath"]))}",
                     TreePath: (string)x["WebPageItemTreePath"],
                     LanguageName: _languageRepository.LanguageIdToName((int)x["WebPageUrlPathContentLanguageID"]),
                     TranslationExists: ((int)x["TranslationExists"]) == 1,
                     IsDefaultLanguage: isDefaultLanguage,
                     IsHome: isHome
                    ); 
                })
                .GroupBy(key => key.WebPageItemID)
                .ToDictionary(key => key.Key, value => (IEnumerable<WebpageTranslationSummary>)value.OrderByDescending(x => x.TranslationExists).ThenByDescending(x => x.IsDefaultLanguage));

            }, new CacheSettings(CacheMinuteTypes.Long.ToDouble(), "WebpageTranslationSummaryDictionary", websiteChannelID));
        }
    }
}
