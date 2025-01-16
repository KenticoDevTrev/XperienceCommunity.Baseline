# Categories

Taxonomy is a big part of content.  The Baseline provides some helper methods and systems to help you retrieve and use that taxonomy in your site.

Categories are handled very different in Xperience by Kentico vs. Kentico Xperience 13, so i'll try to outline how each works.

## Category Storage

**Kentico Xperience 13**

In Kentico Xperience 13, categories are stored in the `CMS_Category` table, and document tagging is done through the `CMS_DocumentCategory` or `CMS_TreeCategory` (part of the [RelationshipsExtended](https://github.com/KenticoDevTrev/RelationshipsExtended) module). 

Retrieval of these categories is simply a lookup on these tables.

**Xperience by Kentico**

In Xperience by Kentico, categories are stored in the `CMS_Tag` table (with `CMS_Taxonomy` as a parent), and tagging storage is a JSON array on any field you specify on your Content Model.

Retrieval of these categories...gets complicated.  It could be quite literally *anywhere*.

So, as part of the hook up for Xperience by Kentico (`AddCoreBaseline`) you are presented with an action to configure the [ContentItemTaxonomyOptions](../../src/Core/Core.Library.Xperience/Models/ContentItemTaxonomyOptions.cs), where you define a list of `ContentItemWithTaxonomyConfigurations` (Class name to fields, and if you want to precache) so you can tell the Baseline where Categorie are stored per content type.

The system will then be able to retrieve categories based on the content item's type and this configuration.

## CategoryItem

The Baseline uses a generic CategoryItem property in it's handling.  It should have all the information from the normal TagInfo class (or CategoryInfo for Kentico Xperience 13).

## Category Retrieval 

Since categories are often used in sites, and they don't change often, the Baseline contains a [ICategoryCachedRepository](../../src/Core/Core.Models/Repositories/ICategoryCachedRepository.cs) which is used to retrieve categories quickly and in a cached fashion, given an `ObjectIdentity` or an array of them (could be the category name, id, or guid).

## Page's Category Retrieval

Retrieving a page's category is handled by the [IContentCategoryRepository](../../src/Core/Core.Models/Repositories/IContentCategoryRepository.cs) and is reponsible for getting all the Categories found on a given Page or Content Item.  It has helper methods to also take an array of Taxonomy Types if you wish to only return ones of a certain type.

Remember though, this depends on your `ContentItemTaxonomyOptions` that you configure in the `AddCoreBaseline` extension method for Xperience by Kentico.

## Comparison

There is an [ICategoryItemEqualityComparer](../../src/Core/Core.Library/Comparers/CategoryItemEqualityComparer.cs) that can be used to compare categories.