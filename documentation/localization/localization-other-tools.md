# Localization Module Tools

The localization module only adds in one additional tool:

[ILocalizationCategoryCachedRepository](../../src/Localization/Localization.Models/Repositories/ILocalizedCategoryCachedRepository.cs) which has 2 methods:

**LocalizeCategoryItemAsync**: Localizes the CategoryItem given the culture (or the current culture if not specified)
**LocalizeCategoryItemsAsync**: Same as above, just for an array of CategoryItems.

This is all this module really does, but still useful if you have multi-lingual sites and use Categories for filters or in display.