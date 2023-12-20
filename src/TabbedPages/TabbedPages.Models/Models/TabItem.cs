namespace TabbedPages.Models
{
    public record TabItem
    {
        public TabItem(string name, int documentID)
        {
            Name = name;
            DocumentID = documentID;
        }

        public string Name { get; init; }
        public int DocumentID { get; init; }
    }
}
