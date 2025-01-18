# Permission Enabled Search (Xperience by Kentico)

Since the Baseline uses the [Member Roles](https://github.com/KenticoDevTrev/MembershipRoles_Temp) system, each page and content item can checked against the current user's roles to see if they should or should not have accesses.

The Lucene Baseline Implementation's [BaselineBaseMetadataIndexingStrategy](../../src/Search/Search.Library.Xperience.Lucene/IndexStrategies/BaselineBaseMetadataIndexingStrategy.cs) Retrieves and stores this information in the Lucene document.

Then, the [LuceneSearchRepository](../../src/Search/Search.Library.Xperience.Lucene/Repositories/Implementations/LuceneSearchRepository.cs)'s implementation of the `ISearchRepository` is able to search across the multiple indexes, retrieve and filter out any items not allowed, order by the Score and retrieve the appropriate items.

Feel free to agian, leverage this and possibly implement your own (using [standard CI/CD customization](../general/customization-points.md)) to replace the default implementation.