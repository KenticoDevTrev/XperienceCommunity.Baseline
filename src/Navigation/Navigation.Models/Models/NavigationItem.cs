namespace Navigation.Models
{
    /// <summary>
    /// Used to build a NavigationItem, should convert to the NavigationItem once built.
    /// </summary>
    public class NavigationItemBuilder
    {
        public NavigationItemBuilder(string linkText)
        {
            LinkText = linkText;
        }

        public string LinkText { get; set; }
        public int NavLevel { get; set; } = 0;
        public List<NavigationItemBuilder> Children { get; set; } = new List<NavigationItemBuilder>();
        public Maybe<string> LinkCSSClass { get; set; }
        public Maybe<string> LinkHref { get; set; }
        public Maybe<string> LinkTarget { get; set; }
        public Maybe<string> LinkOnClick { get; set; }
        public Maybe<string> LinkAlt { get; set; }
        public Maybe<string> LinkPagePath { get; set; }
        public Maybe<Guid> LinkPageGUID { get; set; }
        public Maybe<Guid> LinkDocumentGUID { get; set; }
        public Maybe<int> LinkPageID { get; set; }
        public Maybe<int> LinkDocumentID { get; set; }
        public bool IsMegaMenu { get; set; } = false;

        /// <summary>
        /// Call this on the top level navigation items to set the levels
        /// </summary>
        public void InitializeNavLevels()
        {
            Children.ForEach(child =>
            {
                child.NavLevel = NavLevel + 1;
                child.InitializeNavLevels();
            });
        }

        public NavigationItem ToNavigationItem()
        {
            return new NavigationItem(LinkText)
            {
                LinkText = LinkText,
                NavLevel = NavLevel,
                Children = Children.Select(x => x.ToNavigationItem()),
                LinkCSSClass = LinkCSSClass,
                LinkHref = LinkHref,
                LinkTarget = LinkTarget,
                LinkOnClick = LinkOnClick,
                LinkAlt = LinkAlt,
                LinkPagePath = LinkPagePath,
                LinkPageGUID = LinkPageGUID,
                LinkDocumentGUID = LinkDocumentGUID,
                LinkPageID = LinkPageID,
                LinkDocumentID = LinkDocumentID,
                IsMegaMenu = IsMegaMenu
            };
        }
    }

    public record NavigationItem
    {
        public NavigationItem(string linkText)
        {
            LinkText = linkText;
        }

        public string LinkText { get; init; }
        public int NavLevel { get; init; } = 0;
        public IEnumerable<NavigationItem> Children { get; init; } = Array.Empty<NavigationItem>();

        public Maybe<string> LinkCSSClass { get; init; }

        public Maybe<string> LinkHref { get; init; }

        public Maybe<string> LinkTarget { get; init; }
        public Maybe<string> LinkOnClick { get; init; }
        public Maybe<string> LinkAlt { get; init; }
        public Maybe<string> LinkPagePath { get; init; }
        public Maybe<Guid> LinkPageGUID { get; init; }
        public Maybe<Guid> LinkDocumentGUID { get; init; }
        public Maybe<int> LinkPageID { get; init; }
        public Maybe<int> LinkDocumentID { get; init; }
        public bool IsMegaMenu { get; init; } = false;



        /// <summary>
        /// Checks if the current page or descendent is current page
        /// </summary>
        /// <param name="PageIdentifier">can pass a string (LinkPagePath/LinkHref match), an Int (LinkPageID match), or a Guid (LinkPageGUID)</param>
        /// <returns></returns>
        public bool IsDescendentCurrentPage(object PageIdentifier)
        {
            return IsCurrentPage(PageIdentifier) || Children.Any(x => x.IsCurrentPage(PageIdentifier) || x.IsDescendentCurrentPage(PageIdentifier));
        }

        /// <summary>
        /// Returns true if the NavItem represents the current page
        /// </summary>
        /// <param name="PageIdentifier">can pass a string (LinkPagePath/LinkHref match), an Int (LinkPageID match), or a Guid (LinkPageGUID)</param>
        public bool IsCurrentPage(object PageIdentifier)
        {
            if (PageIdentifier == null)
            {
                return false;
            }

            switch (PageIdentifier.GetType().FullName)
            {
                case "System.String":
                default:
                    if ((LinkPagePath.HasValue && LinkPagePath.Value.Equals(Convert.ToString(PageIdentifier), StringComparison.InvariantCultureIgnoreCase)) || (LinkHref.HasValue && LinkHref.Value.Equals(Convert.ToString(PageIdentifier), StringComparison.InvariantCultureIgnoreCase)))
                    {
                        return true;
                    }
                    break;
                case "System.Int32":
                case "System.Int16":
                case "System.Int":
                    if ((LinkPageID.HasValue && LinkPageID == Convert.ToInt32(PageIdentifier)) || (LinkDocumentID.HasValue && LinkDocumentID.Equals(Convert.ToInt32(PageIdentifier))))
                    {
                        return true;
                    }
                    break;
                case "System.Guid":
                    if (LinkPageGUID.HasValue && LinkPageGUID.Value.Equals((Guid)PageIdentifier))
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

    }

}