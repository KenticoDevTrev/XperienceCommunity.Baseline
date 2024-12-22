namespace Navigation.Models
{
    public record SiteMapOptions : ICacheKey
    {
        /// <summary>
        /// Determines the path to start from
        /// </summary>
        public string Path { get; init; } = string.Empty;

        /// <summary>
        /// Determines which page types are displayed. Specify the page types as a list of code names separated by semicolons (;). If empty, only CMS.MenuItem pages are loaded by default (KX13) or only pages that inherit the IBaselineMetadata (with With Exclude from Sitemap false) and Manual moded Generic.Navigation items, all of which that are public.
        /// </summary>
        public IEnumerable<string> ClassNames { get; init; } = [];

        /// <summary>
        /// Specifies whether the default language version of pages is used as a replacement for pages that are not translated into the currently selected language. If you select the 'Use site settings' option, the web part loads the value from 'Settings -> Content -> Combine with default culture'.
        /// </summary>
        public Maybe<bool> CombineWithDefaultCulture { get; init; }

        /// <summary>
        /// Indicates which culture version of the specified pages should be used.
        /// </summary>
        public Maybe<string> CultureCode { get; init; }

        /// <summary>
        /// Specifies the maximum number of content tree sub-levels from which the content is to be loaded. This number is relative, i.e. it is counted from the beginning of the path specified for the content of the web part. Entering -1 causes all sub-levels to be included.
        /// </summary>
        public int MaxRelativeLevel { get; init; } = -1;

        /// <summary>
        /// Specifies whether the web part only loads published pages.
        /// </summary>
        public bool SelectOnlyPublished { get; init; } = true;

        /// <summary>
        /// Sets the value of the WHERE clause in the SELECT statement used to retrieve the content.
        /// </summary>
        public Maybe<string> WhereCondition { get; init; }

        /// <summary>
        /// Indicates whether the web part checks the permissions of the users viewing the content. If enabled, the web part only loads pages for which the user has the 'Read' permission.  Default is true
        /// </summary>
        public Maybe<bool> CheckDocumentPermissions { get; init; }

        /// <summary>
        /// If this is false and Check Permissions is true, will use current user, if unset or true then will check permissions with the Public User only. Default is true.
        /// </summary>
        public Maybe<bool> PublicOnly { get; init; }

        /// <summary>
        /// Sets the name of the cache key used for the content of the web part. If not specified, this name is generated automatically based on the site, page path, Web part control ID and current user. A cache key can be shared between multiple web parts with the same content on different pages in order to avoid keeping redundant data in the memory.
        /// </summary>
        public Maybe<string> CacheItemName { get; init; }

        /// <summary>
        /// The Column name to get the relative Url field from, useful for Navigation page types
        /// </summary>
        public Maybe<string> UrlColumnName { get; init; }

        public string GetCacheKey()
        {
            return CacheItemName.GetValueOrDefault($"{Path}|{string.Join(",", ClassNames)}|{CombineWithDefaultCulture.GetValueOrDefault(false)}|{CultureCode.GetValueOrDefault(string.Empty)}|{MaxRelativeLevel}|{SelectOnlyPublished}|{WhereCondition.GetValueOrDefault(string.Empty)}|{CheckDocumentPermissions.GetValueOrDefault(false)}");
        }
    }
}
