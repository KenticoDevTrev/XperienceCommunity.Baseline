namespace TabbedPages.Models
{
    public record TabItem
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageCultureID">The Page Culture ID, in KX13 it's the DocumentID, in Xperience by Kentico it's the ContentItemCommonDataID</param>
        public TabItem(string name, int pageCultureID)
        {
            Name = name;
            PageCultureID = pageCultureID;
        }

        public string Name { get; init; }
        public int PageCultureID { get; init; }
    }
}
