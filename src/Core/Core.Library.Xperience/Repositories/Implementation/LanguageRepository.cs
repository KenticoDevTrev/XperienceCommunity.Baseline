using CMS.ContentEngine.Internal;
using CMS.Websites;
using CMS.Websites.Routing;
using Microsoft.CodeAnalysis;
using System.Data;

namespace Core.Repositories.Implementation
{
    public class LanguageRepository(
        IProgressiveCache progressiveCache,
        IContentLanguageFallbackChainProvider contentLanguageFallbackChainProvider,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IWebsiteChannelContext websiteChannelContext,
        IInfoProvider<SettingsKeyInfo> settingsKeyInfoProvider) : ILanguageRepository
    {
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IContentLanguageFallbackChainProvider _contentLanguageFallbackChainProvider = contentLanguageFallbackChainProvider;
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider = contentLanguageInfoProvider;
        private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider = websiteChannelInfoProvider;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;
        private readonly IInfoProvider<SettingsKeyInfo> _settingsKeyInfoProvider = settingsKeyInfoProvider;

        public async Task<Result<string>> GetLanguagueToSelect(IEnumerable<string> availableLanguages, string requestedLanguage, bool firstIfNoMatch = false)
        {
            // No available languages
            if (!availableLanguages.Any()) {
                Result.Failure<string>("Available Languages is empty");
            }

            var availableLanguagesSafe = availableLanguages.Select(x => x.ToLowerInvariant().Trim());
            var requestedLanguageSafe = requestedLanguage.ToLowerInvariant().Trim();

            if (availableLanguagesSafe.Contains(requestedLanguageSafe)) {
                return requestedLanguageSafe;
            }

            var fallbackLookup = await GetMappingDictionaries();
            var availableLanguageIds = fallbackLookup.NameToId.Where(x => availableLanguagesSafe.Contains(x.Key)).Select(x => x.Value);
            var requestedLanguageId = fallbackLookup.NameToId.TryGetValue(requestedLanguageSafe, out var requestedLangId) ? requestedLangId : 0;

            // Check fallbacks
            var requestedLanguageFallbackIdChain = await _contentLanguageFallbackChainProvider.GetLiveSiteChain(requestedLangId);
            foreach (var requestLangId in requestedLanguageFallbackIdChain) {
                if (availableLanguageIds.Contains(requestLangId)) {
                    return fallbackLookup.IdToName[requestedLangId];
                }
            }

            // Site Default with fallbacks
            if (fallbackLookup.SiteToDefaultLanguageID.TryGetValue(_websiteChannelContext.WebsiteChannelID, out var siteDefaultLangId)) {
                requestedLanguageFallbackIdChain = await _contentLanguageFallbackChainProvider.GetLiveSiteChain(requestedLangId);
                if (availableLanguageIds.Contains(siteDefaultLangId)) {
                    return fallbackLookup.IdToName[siteDefaultLangId];
                }
            }

            // Next Content Default, there is no fallback chain for the default
            if (availableLanguageIds.Contains(fallbackLookup.DefaultLanguageId)) {
                return fallbackLookup.IdToName[fallbackLookup.DefaultLanguageId];
            }

            // If should return first on no other match
            if(firstIfNoMatch) {
                return availableLanguagesSafe.First();
            }

            return Result.Failure<string>("Could not find any good fallback language");
        }

        public string LanguageIdToName(int languageId) => GetLookups().IntToString.TryGetValue(languageId, out var languageName) ? languageName : string.Empty;

        public int LanguageNameToId(string languageName) => GetLookups().StringToInt.TryGetValue(languageName.ToLowerInvariant(), out var languageId) ? languageId : 0;

        /// <summary>
        /// Not caching because this is such a quick operation, will be referenced a lot, and should rarely ever change
        /// </summary>
        /// <returns></returns>
        private LangDictionaryLookups GetLookups()
        {
            return _progressiveCache.Load(cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([
                        $"{ContentLanguageInfo.OBJECT_TYPE}|all"
                    ]);
                }
                var languages = _contentLanguageInfoProvider.Get()
                .GetEnumerableTypedResult();

                return new LangDictionaryLookups(languages.ToDictionary(key => key.ContentLanguageID, value => value.ContentLanguageName), languages.ToDictionary(key => key.ContentLanguageName.ToLower(), value => value.ContentLanguageID));
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetLanguageLookups"));
        }

        

        private record LangDictionaryLookups(Dictionary<int, string> IntToString, Dictionary<string, int> StringToInt);

        private async Task<CultureMappingDictionaries> GetMappingDictionaries()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([
                        $"{ContentLanguageInfo.OBJECT_TYPE}|all",
                        $"{WebsiteChannelInfo.OBJECT_TYPE}|all",
                    ]);
                }

                var languages = (await _contentLanguageInfoProvider.Get()
                .Columns(nameof(ContentLanguageInfo.ContentLanguageID), nameof(ContentLanguageInfo.ContentLanguageName), nameof(ContentLanguageInfo.ContentLanguageIsDefault))
                .GetEnumerableTypedResultAsync());
                var nameToId = languages.ToDictionary(key => key.ContentLanguageName.ToLowerInvariant(), value => value.ContentLanguageID);
                var idToName = languages.ToDictionary(key => key.ContentLanguageID, value => value.ContentLanguageName.ToLowerInvariant());
                var defaultLangId = languages.FirstOrMaybe(x => x.ContentLanguageIsDefault).TryGetValue(out var defaultLang) ? defaultLang.ContentLanguageID : idToName.Keys.First();

                var sitesToLang = (await _websiteChannelInfoProvider.Get()
                .Columns(nameof(WebsiteChannelInfo.WebsiteChannelID), nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID))
                .GetEnumerableTypedResultAsync())
                .ToDictionary(key => key.WebsiteChannelID, value => value.WebsiteChannelPrimaryContentLanguageID);
                return new CultureMappingDictionaries(nameToId, idToName, defaultLangId, sitesToLang);
            }, new CacheSettings(1440, "GetMappingDictionaries"));
        }

        public ObjectIdentity GetInstanceDefaultLanguage()
        {
            return _progressiveCache.Load(cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([$"{SettingsKeyInfo.OBJECT_TYPE}|byname|CMSDefaultCultureCode", $"{ContentLanguageInfo.OBJECT_TYPE}|all"]);
                }

                var defaultCultureCode = _settingsKeyInfoProvider.Get("CMSDefaultCultureCode")?.KeyValue ?? "en";
                var firstAttempt = _contentLanguageInfoProvider.Get()
                    .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageCultureFormat), defaultCultureCode)
                    .Or()
                    .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageName), defaultCultureCode)
                    .GetEnumerableTypedResult();
                if (firstAttempt.Any()) {
                    var item = firstAttempt.First();
                    return new ObjectIdentity() {
                        Id = item.ContentLanguageID,
                        CodeName = item.ContentLanguageName,
                        Guid = item.ContentLanguageGUID
                    };
                }
                if (defaultCultureCode.Contains('-')) {
                    var secondAttempt = _contentLanguageInfoProvider.Get()
                        .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageCultureFormat), defaultCultureCode.Split('-')[0])
                        .Or()
                        .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageName), defaultCultureCode.Split('-')[0])
                        .GetEnumerableTypedResult();
                    if (secondAttempt.Any()) {
                        var item = secondAttempt.First();
                        return new ObjectIdentity() {
                            Id = item.ContentLanguageID,
                            CodeName = item.ContentLanguageName,
                            Guid = item.ContentLanguageGUID
                        };
                    }
                }
                return new ObjectIdentity() {
                    Id = 0,
                    CodeName = defaultCultureCode
                };
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetDefaultLanguageFallback"));
            
        }

        public ObjectIdentity DefaultLanguageForWebsiteChannel(int? websiteChannelID = null)
        {
            if (GetWebsiteChannelById().TryGetValue(websiteChannelID.GetValueOrDefault(_websiteChannelContext.WebsiteChannelID), out var websiteChannel)) {
                return new ObjectIdentity() {
                    Id = websiteChannel.WebsiteChannelPrimaryContentLanguageID,
                    CodeName = LanguageIdToName(websiteChannel.WebsiteChannelPrimaryContentLanguageID)
                };
            }

            return GetInstanceDefaultLanguage();
        }

        private Dictionary<int, WebsiteChannelInfo> GetWebsiteChannelById()
        {
            return _progressiveCache.Load(cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{WebsiteChannelInfo.OBJECT_TYPE}|all");
                }

                return _websiteChannelInfoProvider.Get()
                        .GetEnumerableTypedResult()
                        .ToDictionary(key => key.WebsiteChannelID, value => value);
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetWebsiteChannelById_LanguageRepository"));
        }


        private record CultureMappingDictionaries(Dictionary<string, int> NameToId, Dictionary<int, string> IdToName, int DefaultLanguageId, Dictionary<int, int> SiteToDefaultLanguageID);
    }
}
