# Installation for Kentico Xperience 13

The Localization system is mostly all contained in the nuget packages and is simple to hook up.  Keep in mind Localization is actually part of the Core module, this just adds a couple extra features.

## 1. Add Nuget Packages

Install the `XperienceCommunity.Baseline.Localization.Library.KX13` nuget package on your main MVC Site project.

[See the Modules Architecture Overview](../general/modules-architecture-overview.md) if you wish to install individual packages on your libraries based on dependencies.

## 2. CI/CD Setup

On your IServiceCollection call the `UseLocalization` (under `Localization` namespace).

This has an optional configuration where you can set the default Culture for your site.