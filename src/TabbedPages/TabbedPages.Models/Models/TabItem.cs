namespace TabbedPages.Models
{
    public record TabItem
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageCultureID">The Page Culture ID, in KX13 it's the DocumentID, in Xperience by Kentico it's the ContentItemCommonDataID</param>
        public TabItem(string name, int pageID)
        {
            Name = name;
#pragma warning disable CS0618 // Type or member is obsolete
            PageCultureID = pageID;
#pragma warning restore CS0618 // Type or member is obsolete
            PageID = pageID;
        }

        public string Name { get; init; }
        [Obsolete("Use PageID (in KX13 it's the DocumentID, XperienceByKentico it's the WebPageItemID")]
        public int PageCultureID { get; init; }

        /// <summary>
        /// The Page Identifier. DocumentID in KX13, WebPageItemID in XbyK
        /// </summary>
        public int PageID { get; init; }
    }
}
