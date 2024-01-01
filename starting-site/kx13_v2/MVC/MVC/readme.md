# MVC
This should contain:
* Main MVC Site Initalization / Service and App Configuration
* Kentico Page Template, Widgets, or any component/feature/controller that leverages Kentico technology (such as Widget Zones)
* Other Modules / packages that you wish to leverage (See `XperienceCommunity` prefixed nuget packages, also [Github.com/KenticoDevTrev](https://www.github.com/kenticodevtrev))

This library should reference:
* `XperienceCommunity.Baseline.Core.RCL.KX13` Package (along with any other `XperienceCommmunity.Baseline._____.RCL.KX13`)
* `Site.Components` Project (Agnostic items)
* `Site.Library.KX13` Project (Kentico items)


# Main Project
This is the main .Net Core project.

In the revised baseline, there's some systems and explenations on how this was put together.

# Inheritance
This project should inherit all your libraries (primary through Site.Components and Site.Library.KX13), however the references should almost exclusively come from the MVC.Models class library.

# Workflow
This is the general flow:

1. Kentico Xperience maps pages through the "Page Tree Routing" using Page Templates
1. Page Templates then call the associated Component(s) which does the actual building logic
1. Component's should be Kentico Xperience Agnostic* as much as possible, calling the MVC.Models interfaces to retrieve DTO models, and build their ViewModel from it.

* There are areas where usage of Kentico's API and systems are unavoidable, such as Page Builder elements (widget zones, widgets, sections, inline editors, etc)

This way all Views, Components, and Interface / logic can be tested, faked, very easily, and during upgrades you should only really need to update the Interface implementations along with any Page Builder specific logic.

## Page Templates
While Kentico offers documentation on how to build page filtering, this tool uses Sean's [Xperience-page-template-utilities](https://github.com/wiredviews/xperience-page-template-utilities) package which allows for very easy One to One Page Template Filtering (Each Template has only 1 page type it applies to).  It works automatically by looking at the given PageTypeClassName and returning all page templates that are prefixed with that.

An example, for "Generic.Home" page type, any templtae starting with "Generic.Home" would show (Generic.Home_Default, Generic.Home_Featured)

If you have existing templates or want some other filtering logic, you can override the PageTemplateFilterBy (see )
This seems to cover most cases, as 'shared' page templates across page types is pretty rare, but still doable through normal filtering methods.  See his github repo for more details.

# Samples
Be sure to check out the `samples-and-examples` Section of this repository for copy and paste-able widgets, code examples, etc.
