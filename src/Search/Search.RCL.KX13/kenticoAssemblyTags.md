# Assembly Tags
Below are the various assembly tags that you will want to include in your project in order to leverage the Page Templates, Widgets, etc.

```csharp
[assembly: RegisterPageTemplate(
    "Generic.Search_Default",
    "Search",
    typeof(SearchPageTemplateProperties),
    "~/Features/Search/SearchPageTemplate.cshtml")]
```