namespace Core.Repositories
{
    public interface IContentTranslationInformationRepository
    {

        public Task<IEnumerable<WebpageTranslationSummary>> GetWebpageTranslationSummaries(int webPageItemID, int websiteChannelID);

        public Task<IEnumerable<ContentTranslationSummary>> GetContentItemTranslationSummaries(int contentItemID, int? channelID = null);
    }
}
