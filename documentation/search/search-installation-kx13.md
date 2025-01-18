# Installation for Kentico Xperience 13

For Kentico Xperience 13, the Search module just integrates with the standard Smart Search.  All functionality for those indexes is configured in the admin.

The Search system is mostly all contained in the nuget packages and is simple to hook up, *except* it does have two special page type `Generic.Search`, which is optional and allows you to have a Page Builder enabled Search page.

## 1. Add Page Types

Log into the Kentico Admin, and proceed to Site -> Import Sites and Objects

Grab the [Baseline_Generics.1.0.0.zip](../../starting-site/kx13/Baseline_Generics.1.0.0.zip) file from this repository, and import it.

When selecting what Page Types to import, you will want the `Search` page type, unless you are going to have your search controlled by a Controller.

## 2. Add Nuget Packages

Install the `XperienceCommunity.Baseline.Search.RCL.KX13` nuget package on your main MVC Site project.

[See the Modules Architecture Overview](../general/modules-architecture-overview.md) if you wish to install individual packages on your libraries based on dependencies.

## 3. CI/CD Setup

On your IServiceCollection call the `AddBaselineSearch`, including in the `BaselineSearchOptions` the Default Search Index Names (through Admin -> Smart Search)

## 4. Add Page Templates

If you decide to use the `Search` Page, include this page template registration

```csharp
[assembly: RegisterPageTemplate(
    "Generic.Search_Default",
    "Search",
    typeof(SearchPageTemplateProperties),
    "~/Features/Search/SearchPageTemplate.cshtml")]
```

## Customization

Make sure you add your Smart Search Indexes to the `BaselineSearchOptions`

Use normal [View Overwriting Customization Point](../general/customization-points.md) to put your own rendering on the `/Features/Search/SearchPageTemplate.cshtml`, or register your own page template for it.