# Page Identity and Context

Most of your coding logic will be based around the current requested page.  In Kentico Xperience 13, this was a `TreeNode` object with all the values, in Xperience by Kentico it's only a `WebPageDataContext` object which doesn't have much for values.

The Baseline provides the `IPageContextRepository` which fills in many of the gaps, and can be called to get whatever the current page is.


## PageIdentity

The Page Identity contains a lot of data, hopefully everything you would ever need to perform additional actions.

Since this only exists in the context of a page request, it contains things Urls, data for each of it's identities (`ContentIdentity`, `ContentCultureIdentity`, `TreeIdentity`, `TreeCultureIdentity`, all available in properties on the PageIdentity model), and the Page Type.

You can use the `PageType` property to retrieve additional information, in the near future I may have a Storage/Retrieval mechanism for the full item as it will probably be common you'll want this.

### PageIdentity T Data
The PageIdentity also has a Generic typed version, which revelas a Data property of the given generic type.

You can leverage this to include additional data with your PageIdentity (such as your actual Content Type model, Reusable Schema data, or a DTO of it).

If you retrieve a PageIdentity of one type, and wish to convert it to a PageIdentity of a different type, there is the [IPageIdentityFactory.Convert](../src/Core/Core.Models/Services/IPageIdentityFactory.cs) which allows you to convert the Data from one type to another easily.


## IPageContextRepository

Here's a breakdown of the methods and their uses.

**IsEditModeAsync**: Returns if it's Edit Mode or not

**IsPreviewMOdeAsync**: Returns if it's Preview mode

**GetCurrentPageAsync** / **GetPageAsync**: Gets the current / requested page (as a `PageIdentity`).

**GetCurrentPageAsync T** / **GetPageAsync T**: Typed version of GetCurrentPageAsync() and GetPageAsync, can pass your Content Type or Reusable Schema type as the parameter, and if the current page is that type (or inherits from that reusable schema), it will return the PageIdentity WITH the T Data property of the type you requested (Result.Failure if couldn't find or parse);

### Leveraging the Typed GetPageAsync/GetCurrentPageAsync
The IPageContextRepository.GetCurrentPageAsync<T> or GetPageAsync<T> are very useful both for your initial Page Templates (to get the model without needing additional parsing each time), but also for "Current Page" related View Components.  For example, if you have an `IBanners` Reusable Schema that links to `Banner` Content Types.  Some pages have banners, some do not.

You can now have a `BannerViewComponent` that attempts to get the `_pageContextRepository.GetCurrentPageAsync<IBanner>()`.  If it returns a Result.Success, it will have the `PageIdentity<IBanner>` which will have a `Data` property that is the `IEnumerable<Banner>`s.  If the current page doesn't inherit it, it will of course return a `Result.Failure` and you can ignore or handle.

### Upgrading from Kentico Xperience 13
In Kentico Xperience 13, the `IPageDataContextRetriever` easily gave the current `TreeNode` which you could type check.  In Xperience by Kentico, you do not have this, only the `IWebPageDataContextRetriever` which has the `WebPageDataContext` (the lookup values so you can retrieve the model yourself).  Use the `GetCurrentPageAsync<T>()` to easily get the current model.

Next, if you leveraged Related Pages heavily for relationships, or even child items, these can now be represented as a `Reusable Schema` with a `Content Items` field type (where you select the content items related).  Where as before you would take the current page context to find your related items, now your content type would use the Reusable Schema to select those within the model, then use `GetCurrentPageAsync<IMyReusableSchema>` to see if the current page leverages it and if so, retrieve it.