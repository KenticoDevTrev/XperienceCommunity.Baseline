namespace Core.Repositories
{
    public interface ILanguageIdentifierRepository
    {
        int LanguageNameToId(string languageName);

        string LanguageIdToName(int languageId);
    }
}
