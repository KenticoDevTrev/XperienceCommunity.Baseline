# Installation for Xperience by Kentico

The Tabbed Pages system is contained in the nuget packages and is simple to hook up. This system uses the [PartialWidgetPage](https://github.com/KenticoDevTrev/PartialWidgetPage) System to allow you to render the widgets of one page type (Tab) in another page (Tab Parent).

## 1. Add Nuget Packages

Install the `XperienceCommunity.Baseline.TabbedPages.RCL.Xperience` nuget package on your main MVC Site project.

```
npm install XperienceCommunity.Baseline.TabbedPages.RCL.Xperience
```

[See the Modules Architecture Overview](../general/modules-architecture-overview.md) if you wish to install individual packages on your libraries based on dependencies.

## 2. CI/CD Setup

On your IServiceCollection call the `AddBaselineNavigation`, passing in your `BaselineNavigationOptions`

You may also opt to add your own `IDynamicNavigationRepository` and `ISiteMapCustomizationService`

```csharp
builder.Services.AddTabbedPages();
```

## 3. Page Templates and enable Page Builder

For Tab and Tab Parent Items, you will want to register the Page Templates and those types as being enabled for Page Builder.  Again, Tab Parent is only if you wish to use it, normally you would probably add Tabs to other page types in their own template rendering.

```csharp
[assembly: RegisterPageTemplate(
    "Generic.Tab_Default",
    "Tab",
    typeof(TabPageTemplateProperties),
    "/Features/Tab/TabPageTemplate.cshtml",
    ContentTypeNames = [Tab.CONTENT_TYPE_NAME])]

[assembly: RegisterPageTemplate(
    "Generic.TabParent_Default",
    "Tab Parent",
    typeof(TabParentPageTemplateProperties),
    "/Features/TabParent/TabParentPageTemplate.cshtml",
    ContentTypeNames = [TabParent.CONTENT_TYPE_NAME])]
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
           Generic.TabParent.CONTENT_TYPE_NAME,
           Generic.Tab.CONTENT_TYPE_NAME,
        ]
    })
});
```

## Customization

The Tabs render as just the editable area, so you normally wouldn't customize them.  You may want to just not use the Tab Parent and use your own Page Type and make a template [that performs the same logic](../../src/TabbedPages/TabbedPages.RCL.Xperience/Features/TabParent/TabParent.cshtml).
