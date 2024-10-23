using CMS.ContentEngine.Internal;
using CMS.Websites;
using CMS.Websites.Routing;
using System.Data;

namespace Core.Repositories.Implementation
{
    public class LanguageFallbackRepository(
        IProgressiveCache progressiveCache,
        IContentLanguageFallbackChainProvider contentLanguageFallbackChainProvider,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IWebsiteChannelContext websiteChannelContext) : ILanguageFallbackRepository
    {
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IContentLanguageFallbackChainProvider _contentLanguageFallbackChainProvider = contentLanguageFallbackChainProvider;
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider = contentLanguageInfoProvider;
        private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider = websiteChannelInfoProvider;
        private readonly IWebsiteChannelContext _websiteChannelContext = websiteChannelContext;

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
            if (availableLanguageIds.Contains(fallbackLookup.defaultLanguageId)) {
                return fallbackLookup.IdToName[fallbackLookup.defaultLanguageId];
            }

            // If should return first on no other match
            if(firstIfNoMatch) {
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

        private record CultureMappingDictionaries(Dictionary<string, int> NameToId, Dictionary<int, string> IdToName, int defaultLanguageId, Dictionary<int, int> SiteToDefaultLanguageID);
    }
}
