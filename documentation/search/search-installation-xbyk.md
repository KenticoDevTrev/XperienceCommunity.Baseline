# Installation for Xperience by Kentico

The Search Pages system is contained in the nuget packages and is simple to hook up. This system uses the [Xperience by Kentico Lucene](https://github.com/Kentico/xperience-by-kentico-lucene) System to allow you to create indexes.

## 1. Add Nuget Packages

This package, because the implementation is specific to Lucene, is in 3 separate Nuget Packages:

On the MVC Site
```
npm install XperienceCommunity.Baseline.Search.RCL.Xperience
npm install XperienceCommunity.Baseline.Search.Library.Xperience.Lucene
```

On the Admin Site
```
npm install XperienceCommunity.Baseline.Search.Admin.Xperience.Lucene
```

[See the Modules Architecture Overview](../general/modules-architecture-overview.md) if you wish to install individual packages on your libraries based on dependencies.

## 2. CI/CD Setup

On your IServiceCollection call the `AddBaselineSearch`, `AddBaselineSearchRCL`, and `AddBaselineSearchLucene` along with configurations.

You may also opt to add your own `IBaselineSearchLuceneCustomizations` to take a Search Text and index and generate your own Query.  An exaple is given in the default implementation [BaselineSearchLuceneCustomizations](../../src/Search/Search.Library.Xperience.Lucene/Services/Implementations/BaselineSearchLuceneCustomizations.cs).

```csharp
// BASELINE CUSTOMIZATION - Search - Add your search implementation here and inject ISearchRepository implementation
// Note that Lucene has been done and is available through the XperienceCommunity.Baseline.Search.Admin.Xperience.Lucene and XperienceCommunity.Baseline.Search.Library.Xperience.Lucene
// https://github.com/Kentico/xperience-by-kentico-lucene
// https://github.com/Kentico/xperience-by-kentico-algolia
// https://github.com/Kentico/xperience-by-kentico-azure-ai-search
builder.Services.AddBaselineSearch(options => {
    options.DefaultSearchIndexes = ["TestIndex"];
});

// If you wish to use the page type and Page Templates
builder.Services.AddBaselineSearchRCL(options => {
    options.AddSearchPageType = true;
});

// Lucene Setup
builder.Services.AddKenticoLucene(builder =>
        builder
            .RegisterStrategy<BaselineBaseMetadataIndexingStrategy>("BaselinePagesStrategy")
    )
    .AddBaselineSearchLucene();
// Implement and add your own IBaselineSearchLuceneCustomizations which controls how Queries are parsed.
// builder.Services.AddScoped<IBaselineSearchLuceneCustomizations, MySearchCustomizations>();
```

## 3. Page Templates and enable Page Builder

For Tab and Tab Parent Items, you will want to register the Page Templates and those types as being enabled for Page Builder.  Again, Tab Parent is only if you wish to use it, normally you would probably add Tabs to other page types in their own template rendering.

```csharp
[assembly: RegisterPageTemplate(
    "Generic.Search_Default",
    "Search",
    typeof(SearchPageTemplateProperties),
    "~/Features/Search/SearchPageTemplate.cshtml",
    ContentTypeNames = [Search.CONTENT_TYPE_NAME])]
```

```csharp
// Enable desired Kentico Xperience features
builder.Services.AddKentico(features => {
    features.UsePageBuilder(new PageBuilderOptions {
        ContentTypeNames =
        [
            // Enables Page Builder for content types using their generated classes
            
            ...
           // BASELINE CUSTOMIZATION - TabbedPages - If using TabbedPages, MUST add it here
           Generic.Search.CONTENT_TYPE_NAME
        ]
    })
});
```
