# XperienceModels
This class should contain all of your Kentico Xperience Page Types / Custom Tables / Form Info/Info Provider classes.

This can also contain any SHARED (Admin + MVC) items such as Global Event Hooks.

This package should primarily be leveraged by the Site.Library project to properly implement interfaces from the Site.Models, but will also be referenced by the main MVC site for things such as Page Type and Routing declarations.

This Library should *ONLY* reference:
* `Kentico.Xperience.Libraries`
* `XperienceCommunity.Baseline.Core.XperienceModels.KX13`  (along with any other `XperienceCommmunity.Baseline._____.XperienceModels.KX13`)
* `Site.UniversalModels` project.

It *SHOULD NOT* reference any admin /LIB drivers, nor the Kentico.Xperience.AspNet packages