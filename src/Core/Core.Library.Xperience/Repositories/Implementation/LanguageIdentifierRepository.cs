
namespace Core.Repositories.Implementation
{
    public class LanguageIdentifierRepository(IProgressiveCache progressiveCache,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider) : ILanguageIdentifierRepository
    {
        private readonly IProgressiveCache _progressiveCache = progressiveCache;
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider = contentLanguageInfoProvider;

        public string LanguageIdToName(int languageId) => GetLookups().IntToString.TryGetValue(languageId, out var languageName) ? languageName : string.Empty;

        public string LanguageNameToCultureFormat(string languageName) => GetLookups().NameToCultureName.TryGetValue(languageName.ToLowerInvariant().Split('-')[0], out var cultureName) ? cultureName : languageName;

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

                return new LangDictionaryLookups(
                    languages.ToDictionary(key => key.ContentLanguageID, value => value.ContentLanguageName), 
                    languages.ToDictionary(key => key.ContentLanguageName.ToLower(), value => value.ContentLanguageID),
                    languages.ToDictionary(key => key.ContentLanguageName.ToLower(), value => value.ContentLanguageCultureFormat)
                    );
            }, new CacheSettings(CacheMinuteTypes.VeryLong.ToDouble(), "GetLanguageLookups"));
        }

        private record LangDictionaryLookups(Dictionary<int, string> IntToString, Dictionary<string, int> StringToInt, Dictionary<string, string> NameToCultureName);

    }
}
