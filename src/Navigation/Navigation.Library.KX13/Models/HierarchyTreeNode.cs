namespace Navigation.KX13.Models
{
    /// <summary>
    /// Helper to build out Hierarchy Parent-child relationships without the limit of the CMS.Document only "Children" property of the TreeNode Class
    /// 
    /// This should not be cached as the List and Items are reference typed
    /// </summary>
    public class HierarchyTreeNode(TreeNode page) : ICacheKey
    {
        public TreeNode Page { get; set; } = page;
        public List<HierarchyTreeNode> Children { get; set; } = [];

        public string GetCacheKey()
        {
            return "NodeID_" + Page.NodeID;
        }
    }
}
