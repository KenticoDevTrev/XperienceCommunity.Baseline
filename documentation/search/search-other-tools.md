# Other Tools (Xperience by Kentico)

Below are the tools the Baseline's Search.Lucene implementation provides:

# Lucene Classes

- [BaselineBaseMetadataIndexingStrategy](../../src/Search/Search.Library.Xperience.Lucene/IndexStrategies/BaselineBaseMetadataIndexingStrategy.cs): See [search indexing strategy](search-indexing-strategy.md)
- [BaselineSearchLuceneWebCralwerService](../../src/Search/Search.Library.Xperience.Lucene/WebCrawler/BaselineSearchLuceneWebCrawlerService.cs): Service to crawl the web with, note if you implement your own, you have to CI/CD Hookup with `.AddHttpClient<YourOwnLuceneWebCrawlerService>()`
- [BaselineSearchLuceneWebScraperSanitzier](../../src/Search/Search.Library.Xperience.Lucene/WebCrawler/BaselineSearchLuceneWebScraperSanitizer.cs): Strips out HTML and other content from crawling, including the previous KX13 `data-ktc-search-exclude` exclusion attribute

## Interfaces

- [IBaselineSearchLuceneCustomizations](../../src/Search/Search.Library.Xperience.Lucene/Services/IBaselineSearchLuceneCustomizations.cs): Used to Generate the Query give the search text and index, you'll probably want to customize this.
- [ISearchRepository](../../src/Search/Search.Models/Repositories/ISearchRepository.cs) (implemented by [LuceneSearchRepository](../../src/Search/Search.Library.Xperience.Lucene/Repositories/Implementations/LuceneSearchRepository.cs)): Allows you to query against indexes and return the results, given the page and page size.
