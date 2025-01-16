# Caching Systems

Although .Net 8+ is already blazingly fast, caching (both data and rendering) is a vital practice to make sure your solution is scalable, uses minimal resources (saves $$$), and has blazing fast TTFB (time to first byte).

## IProgressive Cache and Cache Dependencies

Kentico provides a Caching service [IProgressiveCache](https://docs.kentico.com/developers-and-admins/development/caching/data-caching) which is the primary mechanism for Data Caching.  This system uses fast Memory Caching, and has a `CacheDependency` properties that allows you to pass an array of [Cache Dependency keys](https://docs.kentico.com/developers-and-admins/development/caching/cache-dependencies) that automatically get cleared when corresponding elements are touched in the system.

However, there are a couple of issues:

1. The Cache Dependency Keys can be difficult to build
2. If you use `<cache>` tags, you often don't have visibility or an easy way to collect all the dependency keys so your Content Caching clears as well when the data clears.

The Baseline uses the [MVCCaching](https://github.com/KenticoDevTrev/MVCCaching) System, which i designed to solve these problems.  I won't re-invent the documentation wheel, please see the [MVC Caching Usage](https://github.com/KenticoDevTrev/MVCCaching/blob/master/README.md#usage) for how to use it. 

This system solves the problems of Cache Context by:

1. Allows for easy Dependency Key generation through ICacheDependencyBuilder extentions
2. Has the `ICacheDependencyScope` service which automatically collects added dependencies (as long as added **outside your IProgressiveCache method**)
3. Has an easy way to convert your `ICacheDependencyBuilder` to the `CMSCacheDependency` required on the `CacheDependency` parameter
4. Has a `DTOWithDependencies` class that allows you to return your results **with an array of dependencies** so you can pass any dependencies found within your `IProgressiveCache` outside, and add it to the `ICacheDependencyScope` through the `ICacheDependencyBuilder.AppendDTOWithDependencies` extension
5. Allows for easy begin and end context of the dependency keys through `ICacheDependencyScope.Begin()` and `string[] ICacheDependencyScope.End()` to pass to your `<cache-dependency>` or `<cache-dependency-mvc>` tag helper within your `<cache>` tag.

## Custom Vary By (Site and Culture)

The two main areas you'll want to vary your caches by are the current Language (culture), and if you do multi-site, the current site.  The Baseline has the [CustomVaryByMiddleware](../src/Core/Core.RCL/Middleware/CustomVaryByMiddleware.cs) which adds two headers (x-culture and x-site) with the proper values.

The Culture is derived from the `System.Threading.Thread.CurrentThread.CurrentCulture.Name.Split('-')[0]` which should set itself properly in either Xperience by Kentico or Kentico Xperience 13.

The site is just from the normal Site Context.

You can leverage these then by using the vary-by-headers using the CustomVaryByHeaders.SiteVaryBy(), CustomVaryByHeaders.CultureSiteVaryBy(), and CustomVaryByHeaders.CultureVaryBy()

```html
<cache vary-by-header=@($"{CustomVaryByHeaders._CULTURE_},MyCustomHeaderVaryBy")></cache>
<cache vary-by-header=@CustomVaryByHeaders.CultureSiteVaryBy()></cache>
```

You can also use mine as a template to create your own.