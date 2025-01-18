# Installation for Kentico Xperience 13

The Tabbed Pages system is mostly all contained in the nuget packages and is simple to hook up, *except* it does have two special page type `Generic.Tab` and `Generic.TabParent`.  This system uses the [PartialWidgetPage](https://github.com/KenticoDevTrev/PartialWidgetPage) System to allow you to render the widgets of one page type (Tab) in another page (Tab Parent).

## 1. Add Page Types

Log into the Kentico Admin, and proceed to Site -> Import Sites and Objects

Grab the [Baseline_Generics.1.0.0.zip](../../starting-site/kx13/Baseline_Generics.1.0.0.zip) file from this repository, and import it.

When selecting what Page Types to import, you will need the `Tab` and optionally the `Tab Parent` page types.

## 2. Add Nuget Packages

Install the `XperienceCommunity.Baseline.TabbedPages.RCL.KX13` nuget package on your main MVC Site project.

[See the Modules Architecture Overview](../general/modules-architecture-overview.md) if you wish to install individual packages on your libraries based on dependencies.

## 3. CI/CD Setup

On your IServiceCollection call the `AddTabbedPages`

## 4. Add Page Templates

To add Page Templates for your Tab and Tab Parent, add the following somewhere in a discoverable assembly:

```csharp
[assembly: RegisterPageTemplate(
    "Generic.Tab_Default",
    "Tab",
    typeof(TabPageTemplateProperties),
    "/Features/Tab/TabPageTemplate.cshtml")]

[assembly: RegisterPageTemplate(
    "Generic.TabParent_Default",
    "Tab Parent",
    typeof(TabParentPageTemplateProperties),
    "/Features/TabParent/TabParentPageTemplate.cshtml")]
```

## Customization

The Tabs render as just the editable area, so you normally wouldn't customize them.  You may want to just not use the Tab Parent and use your own Page Type and make a template [that performs the same logic](../../src/TabbedPages/TabbedPages.RCL.KX13/Features/TabParent/TabParent.cshtml).
