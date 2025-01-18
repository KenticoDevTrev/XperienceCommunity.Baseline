# Baseline Indexing Strategy (Xperience by Kentico)

The Search (Lucene) module comes with an Indexing Strategy that helps replicate a similar 'feel' as in Kentico versions past.  The [BaselineBaseMetadataIndexingStrategy](../../src/Search/Search.Library.Xperience.Lucene/IndexStrategies/BaselineBaseMetadataIndexingStrategy.cs) looks for any page that inherits the `IBaseMetadata`


1. Attempts to retrieve and web pages that have the `IBaseMetadata` and/or `IXperienceCommunityMemberPermissionConfiguration` reusable schemas
2. Retrieve the `PageMetaData` from the page
3. Store any `Member Permissions` (if available) in the lucene document (used to filter out pages the searching user doesn't have permission for)
4. Store any `PageMetaData` in the lucene document
5. Crawl the page using the `IBaselineSearchLuceneWebCrawlerService`, and sanitize the HTML using `IBaselineWebScraperSanitizer` (removes header, footer, script/style tags, and anything with data-ktc-search-exclude), includes this in the lucene document
6. Adds any NoIndex, ContentType, Id, and Created date fields to the lucene document.

Feel free to use as is, or clone and modify these services to your own needs.  Overall, this produced an acceptible site search to start out with.