namespace Core.Repositories.Implementation
{
    public class MappedContentItemLinkedItemDepthRetriever : IMappedContentItemLinkedItemDepthRetriever
    {
        public int GetLinkedItemDepth(string className)
        {
            // Default, override in your own custom implementation
            return 100;
        }
    }
}
