# Assembly Tags
Below are the various assembly tags that you will want to include in your project in order to leverage the Page Templates, Widgets, etc.

```csharp
// Handling of cache touching for navigation category and NavigationItems
[assembly: RegisterModule(typeof(NavigationLoaderModule))]
```