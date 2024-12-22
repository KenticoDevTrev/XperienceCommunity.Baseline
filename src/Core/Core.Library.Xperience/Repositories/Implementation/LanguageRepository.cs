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
        IInfoProvider<SettingsKeyInfo> settingsKeyInfoProvider,
        ILanguageIdentifierRepository languageIdentifierRepository) : ILanguageRepository
    {
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IContentLanguageFallbackChainProvider _contentLanguageFallbackChainProvider = contentLanguageFallbackChainProvider;
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider = contentLanguageInfoProvider;
        private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider = websiteChannelInfoProvider;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;
        private readonly IInfoProvider<SettingsKeyInfo> _settingsKeyInfoProvider = settingsKeyInfoProvider;
        private readonly ILanguageIdentifierRepository _languageIdentifierRepository = languageIdentifierRepository;

        public async Task<Result<string>> GetLanguagueToSelect(IEnumerable<string> availableLanguages, string requestedLanguage, bool firstIfNoMatch = false, bool includeDefaultAsMatch = true)
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
            if (requestedLanguageId > 0) {
                var requestedLanguageFallbackIdChain = await _contentLanguageFallbackChainProvider.GetLiveSiteChain(requestedLanguageId);
                foreach (var requestLangId in requestedLanguageFallbackIdChain) {
                    if (availableLanguageIds.Contains(requestLangId)) {
                        return fallbackLookup.IdToName[requestedLangId];
                    }
                }


                // Site Default with fallbacks
                if (fallbackLookup.SiteToDefaultLanguageID.TryGetValue(_websiteChannelContext.WebsiteChannelID, out var siteDefaultLangId)) {
                    requestedLanguageFallbackIdChain = await _contentLanguageFallbackChainProvider.GetLiveSiteChain(requestedLanguageId);
                    if (availableLanguageIds.Contains(siteDefaultLangId)) {
                        return fallbackLookup.IdToName[siteDefaultLangId];
                    }
                }
            }

            // Next Content Default, there is no fallback chain for the default
            if (includeDefaultAsMatch && availableLanguageIds.Contains(fallbackLookup.DefaultLanguageId)) {
                return fallbackLookup.IdToName[fallbackLookup.DefaultLanguageId];
            }

            // If should return first on no other match
            if (firstIfNoMatch) {
                return availableLanguagesSafe.First();
            }

            return Result.Failure<string>("Could not find any good fallback language");
        }


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

        public string GetLanguageUrlPrefix(int websiteChannelID, int contenLanguageID)
        {
            var defaultLanguageID = DefaultLanguageForWebsiteChannel(websiteChannelID);
            if (defaultLanguageID.Id.Value.Equals(contenLanguageID)) {
                return string.Empty;
            }
            return $"/{LanguageIdToName(contenLanguageID)}";
        }

        public int LanguageNameToId(string languageName) => _languageIdentifierRepository.LanguageNameToId(languageName);

        public string LanguageIdToName(int languageId) => _languageIdentifierRepository.LanguageIdToName(languageId);

        private record CultureMappingDictionaries(Dictionary<string, int> NameToId, Dictionary<int, string> IdToName, int DefaultLanguageId, Dictionary<int, int> SiteToDefaultLanguageID);
    }
}
