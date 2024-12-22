
namespace Core.Repositories
{
    public interface ILanguageRepository
    {
        /// <summary>
        /// Given a list of avaialable Content Language Names, and the one that is being requested, it will return the language code (from the availableLanguages) that is the proper one.
        /// 
        /// This will match first on an exact match, followed by the "fallback" language chain, then the default language if it's present, and if none of those work, just the first language in the list.
        /// </summary>
        /// <param name="availableLanguages">What Languages are available to select from (ex: what ones a Content Item is translated in)</param>
        /// <param name="requestedLanguage">The Language you hoping to find</param>
        /// <param name="firstIfNoMatch">If true, will return the first available language if no good match found, false it will return a Result Failure in that case.</param>
        /// <param name="includeDefaultAsMatch">If true, will check for the site default language as part of it's matching, false will not.</param>
        /// <returns>The best language fit, or failure if no available languages (or no good match found)</returns>
        Task<Result<string>> GetLanguagueToSelect(IEnumerable<string> availableLanguages, string requestedLanguage, bool firstIfNoMatch = false, bool includeDefaultAsMatch = true);

        int LanguageNameToId(string languageName);

        string LanguageIdToName(int languageId);

        /// <summary>
        /// Gets the system's default language (used as a fallback often times)
        /// </summary>
        /// <returns></returns>
        ObjectIdentity GetInstanceDefaultLanguage();

        ObjectIdentity DefaultLanguageForWebsiteChannel(int? websiteChannelID = null);

        /// <summary>
        /// Returns an empty string if it's the site's default language, or /[ContentLanguageName] if it's not.
        /// </summary>
        /// <param name="websiteChannelID">the website channel id</param>
        /// <param name="contenLanguageID">Your item's language ID</param>
        /// <returns></returns>
        string GetLanguageUrlPrefix(int websiteChannelID, int contenLanguageID);
    }
}
