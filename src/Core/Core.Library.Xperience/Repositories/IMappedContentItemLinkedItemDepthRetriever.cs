namespace Core.Repositories
{
    /// <summary>
    /// Customizable to control the Linked Item depth when the IMappedContentRepository retrieves data (used in the IPageContextRepository retrieval logic)
    /// </summary>
    public interface IMappedContentItemLinkedItemDepthRetriever
    {
        public int GetLinkedItemDepth(string className);
    }
}
