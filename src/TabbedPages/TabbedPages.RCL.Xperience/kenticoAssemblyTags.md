# Assembly Tags
Below are the various assembly tags that you will want to include in your project in order to leverage the Page Templates, Widgets, etc.

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