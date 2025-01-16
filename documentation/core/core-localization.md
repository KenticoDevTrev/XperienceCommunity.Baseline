# Localization

The baseline leverages the [XperienceCommunity.Localization](https://www.nuget.org/packages/XperienceCommunity.Localization) (Xperience by Kentico) which restores the Localization Key + Translation functionality that was common in the Kentico Xperience 13 admin.

Kentico Xperience 13 uses the [XperienceCommunity.Localizer](https://www.nuget.org/packages/XperienceCommunity.Localizer) (Kentico Xperience 13) package, which operates the same.

This integrates with the normal `IStringLocalizer` and `IHtmlLocalizer` so you can use both `.resx` files as well as Key + Translations defined in the Kentico Admin.

There is also extension methods for both the [IHtmlLocalizer](../../src/Core/Core.Library.Xperience/Extensions/IHtmlLocalizerExtensions.cs) (`GetHtmlStringOrDefault`) and [IStringLocalizer](../../src/Core/Core.Library.Xperience/Extensions/IStringLocalizerExtensions.cs) (`GetStringOrDefault`) which allow you to put a default value if the given localization key is neither found in the resx nor in Kentico.

Language fallbacks are honored, and Kentico translation takes priority over resx translations.