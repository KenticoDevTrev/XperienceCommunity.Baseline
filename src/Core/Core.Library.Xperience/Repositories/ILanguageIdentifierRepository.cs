namespace Core.Repositories
{
    public interface ILanguageIdentifierRepository
    {
        int LanguageNameToId(string languageName);

        string LanguageIdToName(int languageId);

        /// <summary>
        /// Converts the language (ex "en") to the culture format ("en-US")
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        string LanguageNameToCultureFormat(string languageName);
    }
}
