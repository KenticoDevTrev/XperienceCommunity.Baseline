# Design Patterns

Below is a list of Design Patterns used in the Baseline Project.

## Feature Folders and Components

The Baseline uses the [Features Folder](https://learn.microsoft.com/en-us/archive/msdn-magazine/2016/september/asp-net-core-feature-slices-for-asp-net-core-mvc) methodology when creating new features.  

As part of Feature Folders, some like the system to automatically divine where a view is based on the class name conventions, being in a feature folder, etc.  This can be done partially through the `services.AddMvc(o => o.Conventions.Add(new FeatureConvention()))`, and you can additionally configure it if you wish.

Or, If you wish you can implement and leverage your own [IViewLocationExpander](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.razor.iviewlocationexpander?view=aspnetcore-8.0) and configure as you wish. (The Baseline has an optional [CustomLocationExpander](../../src/Core/Core.RCL/Middleware/CustomLocationExpander.cs) that you can call on your `IServiceCollection.UseFeatureFoldersAndLocationExpansions` to do this).

***HOWEVER***, you can simply do what I do and put in the full path, and not have to worry about any of that.  ex:

```csharp
return View("/Features/Home/Home.cshtml");
```

This is what I would personally recommend.

## Page Template View Component (PTVC) and Widget View Component (WVC)

In light of the **Be Kentico Agnostic as much as possible** mindset the Baseline was built on, we adopted the PTVC and WVC model.

Page Templates and Widgets ultimately must resolve to a `View` or `ViewComponent` which resolves a View.  These are passed Kentico Specific Models and wrappers (not Kentico Agnostic).

In the Baseline, any Page Template (PT) or Widget (W) `View (.cshtml)` only is responsible for parsing it's model into a Kentico Agnostic Model and passing it to a Kentico Agnostic `View Component` (VC).  Thus our core logic and rendering remain Agnostic of Kentico and easy to test and upgrade.

**LIMITATION**: Keep in mind, that any view that requires Kentico Tag Helpers (such as `<editable-area area-identifier="areaSimple" />`) cannot be called from within a Kentico Agnostic realm.  

The other common taghelper, `<cache-dependency>` does have an Agnostic version in the MVCCaching module that is part of the Core Baseline, so you can use `<cache-dependency-mvc>` in it's place if needed.

## Kentico Agnostic DTOs and Identity Models

The Baseline's shared Interfaces all use Generic DTOs, and receive Generic Models in their operations. You can keep this model as it helps make faking these interfaces very easy, as well as limits the data returned to only what you wish (and allows usage of Immutable Records).

When passing data to and from Repositories and Services, the Baseline also leverages the following Identity Models:

- `ObjectIdentity`
- `ContentIdentity`
- `ContentCultureIdentity` (Language Specific)
- `TreeIdentity`
- `TreeCultureIdentity` (Language Specific)
- `PageIdentity` (Combination of `ContentCultureIdentity` and `TreeCultureIdentity` with other properties for the current requested page)

You can leverage these along with the `IIdentityService` to retrieve the properties you need in your implementation layer.

## Post Redirect Get (Legacy)
The Baseline's Core has systems in place to leverage a [Post-Redirect-Get](https://en.wikipedia.org/wiki/Post/Redirect/Get) model when handling requests that are sourced from a Kentico Page.

The idea is that on a Page (such as a Log In Page), you would POST to your controller, once the controller does it's logic (since you can't "return" a Kentico view at this point), it stores the View Model Data then Redirects back to the original requesting page (which Kentico would handle).  Then on your page, you would Get the model data and populate it.  

Keep in mind that I would consider this approach now "Legacy" and instead would highly recommend using a lightweight library called [htmx](https://htmx.org/) which allows you to use data-attributes in the markup to designate a form to POST and then replace (and hookup validation logic) itself with the partial returned by the controller.  This is MUCH easier.