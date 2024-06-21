namespace Core.Models
{
    public record PageIdentity : ICacheKey
    {
        /// <summary>
        /// Primary Constructor
        /// </summary>
        public PageIdentity(string name, string alias, int pageID, Guid pageGuid, int contentID, string contentName, Guid contentGuid, int contentCultureID, Guid contentCultureGuid, string path, string culture, string relativeUrl, string absoluteUrl, int level, int channelID, string pageType)
        {
            Name = name;
            Alias = alias;
            PageID = pageID;
            ContentID = contentID;
            PageGuid = pageGuid;
            ContentGuid = contentGuid;
            ContentName = contentName;
            ContentCultureID = contentCultureID;
            ContentCultureGuid = contentCultureGuid;
            Path = path;
            RelativeUrl = relativeUrl;
            AbsoluteUrl = absoluteUrl;
            PageLevel = level;
            ChannelID = channelID;
            PageType = pageType;
            Culture = culture;
        }

        [Obsolete("Use primary constructor, passing NodeID as both PageID and ContentID, and NodeGuid as both PageGuid and ContentGuid, NodeAlias as both Alias and ContentName, DocumentID/Guid as ContentCultureID/Guid, and SiteID as ChannelID")]
        public PageIdentity(string name, string alias, int nodeID, Guid nodeGUID, int documentID, Guid documentGUID, string path, string culture, string relativeUrl, string absoluteUrl, int nodeLevel, int nodeSiteID, string pageType)
        {
            Name = name;
            Alias = alias;
            PageID = nodeID;
            ContentID = nodeID;
            PageGuid = nodeGUID;
            ContentGuid = nodeGUID;
            ContentName = alias;
            ContentCultureID = documentID;
            ContentCultureGuid = documentGUID;
            Path = path;
            RelativeUrl = relativeUrl;
            AbsoluteUrl = absoluteUrl;
            PageLevel = nodeLevel;
            ChannelID = nodeSiteID;
            PageType = pageType;
            Culture = culture;
        }

        
        /// <summary>
        /// The Name of the page (tree)
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// The Code Name of the page (tree)
        /// </summary>
        public string Alias { get; init; }

        /// <summary>
        /// The path of the page (tree)
        /// </summary>
        public string Path { get; init; }

        /// <summary>
        /// Relative URL of the page, no tilde at beginning
        /// </summary>
        public string RelativeUrl { get; init; }

        /// <summary>
        /// Absolute URL of the page
        /// </summary>
        public string AbsoluteUrl { get; init; }

        /// <summary>
        /// The Channel this page identity belongs to.
        /// </summary>
        public int ChannelID { get; init; }

        /// <summary>
        /// The code name of the type the page is
        /// </summary>
        public string PageType { get; init;  }

        /// <summary>
        /// Nesting level of the page (tree)
        /// </summary>
        public int PageLevel { get; init; }

        /// <summary>
        /// Page's identity (tree)
        /// </summary>
        public int PageID { get; init; }

        /// <summary>
        /// The Page's guid identity (tree)
        /// </summary>
        public Guid PageGuid { get; init; }

        /// <summary>
        /// The Content's identity (data, culture agnostic)
        /// </summary>
        public int ContentID { get; init; }

        /// <summary>
        /// The Content's Code Name (data, culture agnostic)
        /// </summary>
        public string ContentName { get; init; }

        /// <summary>
        /// The Content's guid identity (data, culture agnostic)
        /// </summary>
        public Guid ContentGuid { get; init; }

        /// <summary>
        /// The Content's identity (data, culture specific)
        /// </summary>
        public int ContentCultureID { get; init; }

        /// <summary>
        /// The Content's guid identity (data, culture specific)
        /// </summary>
        public Guid ContentCultureGuid { get; init; }


        [Obsolete("Use PageLevel")]
        public int NodeLevel => PageLevel;

        [Obsolete("Use ChannelID")]
        public int NodeSiteID => ChannelID;

        [Obsolete("Use ContentID for data lookups, PageID for tree structure related lookups")]
        public int NodeID => ContentID;

        [Obsolete("Use ContentGuid for data lookups, PageGuid for tree structure related lookups")]
        public Guid NodeGUID => ContentGuid;

        [Obsolete("Use ContentCultureID")]
        public int DocumentID => ContentCultureID;

        [Obsolete("Use ContentCultureGuid")]
        public Guid DocumentGUID => ContentCultureGuid;

        public string Culture { get; init; }

        public ContentIdentity ContentIdentity
        {
            get
            {
                return new ContentIdentity()
                {
                    ContentID = ContentID,
                    ContentGuid = ContentGuid,
                    PathChannelLookup = new PathChannel(Path: Path, ChannelId: ChannelID)
                };
            }
        }

        public ContentCultureIdentity ContentCultureIdentity
        {
            get
            {
                return new ContentCultureIdentity()
                {
                    ContentCultureID = ContentCultureID,
                    ContentCultureGuid = ContentCultureGuid,
                    PathCultureChannelLookup = new PathCultureChannel(Path: Path, Culture: Culture, ChannelId: ChannelID)
                };
            }
        }

        public TreeIdentity TreeIdentity
        {
            get
            {
                return new TreeIdentity()
                {
                    PageID = PageID,
                    PageGuid = PageGuid,
                    PathChannelLookup = new PathChannel(Path: Path, ChannelId: ChannelID)
                };
            }
        }

        public TreeCultureIdentity TreeCultureIdentity
        {
            get
            {
                return new TreeCultureIdentity(Culture)
                {
                    PageID = PageID,
                    PageGuid = PageGuid,
                    PathChannelLookup = new PathChannel(Path: Path, ChannelId: ChannelID)
                };
            }
        }

        [Obsolete("Use ContentIdentity or TreeIdentity")]
        public NodeIdentity NodeIdentity
        {
            get
            {
                return new NodeIdentity()
                {
                    NodeId = ContentID,
                    NodeGuid = ContentGuid,
                    NodeAliasPathAndSiteId = new Tuple<string, Maybe<int>>(Path, ChannelID)
                };
            }
        }

        [Obsolete("Use ContentCultureIdentity")]
        public DocumentIdentity DocumentIdentity
        {
            get
            {
                return new DocumentIdentity()
                {
                    DocumentId = ContentCultureID,
                    DocumentGuid = ContentCultureGuid,
                    NodeAliasPathAndMaybeCultureAndSiteId = new Tuple<string, Maybe<string>, Maybe<int>>(Path, Culture, ChannelID)
                };
            }
        }


        public string GetCacheKey()
        {
            return $"contentculture-{ContentCultureGuid}";
        }

        /// <summary>
        /// Returns an empty Page Identity with empty Guids and 0 valued ids.
        /// </summary>
        /// <returns></returns>
        public static PageIdentity Empty()
        {
            return new PageIdentity("", "", 0, Guid.Empty, 0, "", Guid.Empty, 0, Guid.Empty, "/", "en-US", "/", "/", 0, 0, string.Empty);
        }

        /// <summary>
        /// Returns a page Identity with random Content/Content Culture/Web Page Guids
        /// </summary>
        /// <returns></returns>
        public static PageIdentity Random()
        {
            return new PageIdentity("", "", 0, Guid.NewGuid(), 0, "", Guid.NewGuid(), 0, Guid.NewGuid(), "/", "en-US", "/", "/", 0, 0, string.Empty);
        }
    }

    public record PageIdentity<T> : PageIdentity
    {
        public PageIdentity(string name, string alias, int pageID, Guid pageGuid, int contentID, string contentName, Guid contentGuid, int contentCultureID, Guid contentCultureGuid, string path, string culture, string relativeUrl, string absoluteUrl, int level, int channelID, string pageType, T data) : base(name, alias, pageID, pageGuid, contentID, contentName, contentGuid, contentCultureID, contentCultureGuid, path, culture, relativeUrl, absoluteUrl, level, channelID, pageType)
        {
            Data = data;
        }

        [Obsolete("Use primary constructor, passing NodeID as both PageID and ContentID, and NodeGuid as both PageGuid and ContentGuid, NodeAlias as both Alias and ContentName, DocumentID/Guid as ContentCultureID/Guid, and SiteID as ChannelID")]
        public PageIdentity(string name, string alias, int nodeID, Guid nodeGUID, int documentID, Guid documentGUID, string path, string culture, string relativeUrl, string absoluteUrl, int nodeLevel, int nodeSiteID, string pageType, T data) : base(name, alias, nodeID, nodeGUID, documentID, documentGUID, path, culture, relativeUrl, absoluteUrl, nodeLevel, nodeSiteID, pageType)
        {
            Data = data;
        }

        public PageIdentity(T data, PageIdentity pageIdentity) : base(pageIdentity)
        {
            Data = data;
        }

        /// <summary>
        /// Typed page data
        /// </summary>
        public T Data { get; init; }
    }
}
