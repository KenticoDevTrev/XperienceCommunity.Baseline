# Site.UniversalModels
This can contain universal nuget packages, and items that are shared on admin and the MVC site, such as:
* Enumerators used on Admin and MVC
* Extension Methods
* Nuget Packages (such as CSharpFunctionalExtensions)
* Interfaces that are used on both but implemented differently on Admin vs. MVC

This Library should *NOT* reference Kentico in any fashion, nor any other projects, and should be .Net 2.0 standard