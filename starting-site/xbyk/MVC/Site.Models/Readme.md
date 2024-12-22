# Site.Models
This should contain:
* Kentico Agnostic Models (usually not View Models though)
* Enumerators
* Extension Methods
* Repository and Service Interfaces

This Library should *NOT* reference Kentico in any fashion.

This library should reference:
* `XperienceCommunity.Baseline.Core.Models` Package (along with any other `XperienceCommmunity.Baseline._____.Models`)
* `Site.UniversalModels` Project
