# Page Identity and Context

Most of your coding logic will be based around the current requested page.  In Kentico Xperience 13, this was a `TreeNode` object with all the values, in Xperience by Kentico it's only a `WebPageDataContext` object which doesn't have much for values.

The Baseline provides the `IPageContextRepository` which fills in many of the gaps, and can be called to get whatever the current page is.

## IPageContextRepository

Here's a breakdown of the methods and their uses.

**IsEditModeAsync**: Returns if it's Edit Mode or not

**IsPreviewMOdeAsync**: Returns if it's Preview mode

**GetCurrentPageAsync** / **GetPageAsync**: Gets the current / requested page (as a `PageIdentity`).

## PageIdentity

The Page Identity contains a lot of data, hopefully everything you would ever need to perform additional actions.

Since this only exists in the context of a page request, it contains things Urls, data for each of it's identities (`ContentIdentity`, `ContentCultureIdentity`, `TreeIdentity`, `TreeCultureIdentity`, all available in properties on the PageIdentity model), and the Page Type.

You can use the `PageType` property to retrieve additional information, in the near future I may have a Storage/Retrieval mechanism for the full item as it will probably be common you'll want this.

### PageIdentity T Data
The PageIdentity also has a Generic typed version, which revelas a Data property of the given generic type.

You can leverage this to include additional data with your PageIdentity (such as your actual Content Type model, or a DTO of it).

If you retrieve a PageIdentity of one type, and wish to convert it to a PageIdentity of a different type, there is the [IPageIdentityFactory.Convert](../src/Core/Core.Models/Services/IPageIdentityFactory.cs) which allows you to convert the Data from one type to another easily.

