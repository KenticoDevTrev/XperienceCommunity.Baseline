# Assembly Tags
Below are the various assembly tags that you will want to include in your project in order to leverage the Page Templates, Widgets, etc.

```csharp
[assembly: RegisterPageTemplate(
    "Generic.Navigation_Default",
    "Navigation",
    typeof(NavigationPageTemplateProperties),
    "/Features/Navigation/PartialNavigation/NavigationPageTemplate.cshtml",
    ContentTypeNames = [Generic.Navigation.CONTENT_TYPE_NAME])]

```