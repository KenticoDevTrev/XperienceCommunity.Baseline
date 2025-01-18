# Installation for Xperience by Kentico

The Localization system is contained in the nuget packages and is simple to hook up.  Keep in mind that Localizing functions are part the Core Module, this separate module only adds a a couple small features (namely the `ILocalizedCategoryCachedRepository`)

## 1. Add Nuget Packages

Install the `XperienceCommunity.Baseline.Localization.RCL.Xperience` nuget package on your main MVC Site project.

```
npm install XperienceCommunity.Baseline.Localization.RCL.Xperience
```

[See the Modules Architecture Overview](../general/modules-architecture-overview.md) if you wish to install individual packages on your libraries based on dependencies.

## 2. CI/CD Setup

On your IServiceCollection call the `AddBaselineLocalization`


```csharp
builder.Services.AddBaselineLocalization();
```
