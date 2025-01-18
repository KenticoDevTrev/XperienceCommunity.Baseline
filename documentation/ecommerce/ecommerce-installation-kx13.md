# Installation for Kentico Xperience 13

The Ecommerce system leverages Kentico's built in Ecommerce, along with the [HBS.Kentico.Ecommerce](https://github.com/HBSTech/GenericEcommerce)

This module contains templates for a Generic.Ecommerce page type which is included in the `Baseline_Generics.1.0.0.zip` (steps below).

## 1. Add Navigation Page Type

Log into the Kentico Admin, and proceed to Site -> Import Sites and Objects

Grab the [Baseline_Generics.1.0.0.zip](../../starting-site/kx13/Baseline_Generics.1.0.0.zip) file from this repository, and import it.

When selecting what Page Types to import, you will need the `Ecommerce` Page Type for this module.

## 2. Register Ecommerce Assembly Tag

In your MVC Site, add this so you can use the Page Templates included in the `HBS.Kentico.Ecommerce` package

```csharp
// This is the Ecommerce Page Type Registration, if you import it
[assembly: RegisterDocumentType(Ecommerce.CLASS_NAME, typeof(Ecommerce))]
```

## 3. Follow HBS.Kentico.Ecommerce setup

The rest of the setup should outlined in the [HBS.Kentico.Ecommerce Repo](https://github.com/HBSTech/GenericEcommerce) and [It's wiki](https://github.com/HBSTech/GenericEcommerce/wiki)