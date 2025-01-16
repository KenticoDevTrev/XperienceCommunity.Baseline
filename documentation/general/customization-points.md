# General Customization Points

While each module in the Baseline has different points you can customize at, all of them share these basic principles of customization.

## Implementation Override
Any Interface that the Baseline Implements can be overwritten using normal `IServiceCollection` Dependency Injection model.

While overriding these is often unnecessary (as any point that I believe would most likely have customization i've added additional Interfaces specifically to be implemented and customized by developers), if you find yourself needing custom logic or code, simply copy the source implementation, modify to your liking, and ***AFTER*** you call the hookup extension method for the module, inject your own implementation.

For example, if you wanted to override the default `IBreadcrumbRepository`, you would do this:

```csharp
// Add the default Baseline Navigation
builder.Services.AddBaselineNavigation(new Navigation.Models.BaselineNavigationOptions() {
    ShowPagesNotTranslatedInSitemapUrlSet = false
});

// Add custom one
builder.Services.AddScoped<IBreadcrumbRepository, MyCustomBreadcrumbRepository>();

```

## Options, Actions, Generics and Optional Properties

Many of the `IServiceCollection.AddBaseline____` methods contain options and Action parameters to configure the options that control the baseline functionality.  Leverage these to adjust the configuration points the Baseline exposes.  These are primarily available in Xperience by Kentico more so than Kentico Xperience 13 since Kentico Xperience 13 often had built-in locations and ways of performing actions (ex: Categories, Media Files, Meta Data, etc), where as Xperience by Kentico is much more open to your own way of doing things.

Additionally, some classes leverage [C# Generics](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics) in order for you to use your own type or include a property of your own type.  

The `Account` module hookup requires you to set the `TUser` and `TRole` types, allowing you to extend the base types with your own.

Some models have an optional property which is intended for you to store any customization models you wish.  

The [PageIdentity](../../src/Core/Core.Models/Models/PageIdentity.cs) has a Generic version which exposes a `T Data` property, so you can append any data you wish to the PageIdentity that you pass along (such as data from the Page's model itself).  

The [User](../../src/Core/Core.Models/Models/User.cs) class has an `public Maybe<IUserMetadata> MetaData {get; init;} = Maybe.None;` which you can fill with any class that inherits the `IUserMetadata` (an empty interface).  This allows you to extend the model without creating your own.

## View Overrides

The Baseline is designed to be usable on any site, that being said we often had to have views existing for the view components and other items.

In .Net Core, if you place a View at the same location and name as one found in a Library you are referencing, your site's version of the .cshtml file will be used.

For example, the **Navigation** module's views almost always will need to be customized to your theme.  If i wanted to override the `Navigation.RCL.Xperience`'s Main Navigation views, i would create my own views at:

- `/Components/Navigation/MainNavigation/MainNavigation.cshtml`
- `/Components/Navigation/MainNavigation/MainNavigationDropdownItem.cshtml`
- `/Components/Navigation/MainNavigation/MainNavigationItem.cshtml`

Just copy the existing .cshtml from the Baseline repository and add your own markup.

## View Component Overrides

Sadly there is no way to "Override" a View Component like you would with a View, however you can Override the View that is calling the view component and instead call your OWN view component from it.

For example, the Account module has a `ConfirmationPageTemplate.cshtml` that calls the `<vc:confirmation>` view component:

```html
@model TemplateViewModel<Account.Features.Account.Confirmation.ConfirmationPageTemplateProperties>
<vc:confirmation />
```

 If I wanted to have my own Confirmation View Component, I would add my own `ConfirmationPageTemplate.cshtml` at the same location as the one in the Baseline, and call my own View Component:

`/Features/Account/Confirmation/ConfirmationPageTemplate.cshtml`

```html
@model TemplateViewModel<Account.Features.Account.Confirmation.ConfirmationPageTemplateProperties>
<vc:my-custom-confirmation />
```


# Xperience by Kentico Starting Site Customization Helpers

In the Xperience by Kentico Starting Site, I've added a helper tag to help identify Customization points for each module.  The Starting site has all the modules by default (you can simply remove the ones you don't need).

If you search the entire solution for `BASELINE CUSTOMIZATION`, you'll see comments like the example below:

```csharp
// BASELINE CUSTOMIZATION - Core - Override Baseline customization points if wanted
/*
builder.Services.AddScoped<IUserMetadataProvider, CustomUserMetadataProvider>();
builder.Services.AddScoped<IMediaFileMediaMetadataProvider, CustomMediaFileMediaMetadataProvider>();
builder.Services.AddScoped<IContentItemMediaCustomizer, CustomContentItemMediaCustomizer>();
builder.Services.AddScoped<IContentItemMediaMetadataQueryEditor, CustomContentItemMediaMetadataQueryEditor>();
builder.Services.AddScoped<ICustomTaxonomyFieldParser, CustomCustomTaxonomyFieldParser>();
*/
```

or

```csharp
// BASELINE CUSTOMIZATION - Navigation - Add navigation and configure here
builder.Services.AddBaselineNavigation(new Navigation.Models.BaselineNavigationOptions() {
    ShowPagesNotTranslatedInSitemapUrlSet = false
});
```

These will show you and explain all the customization points that you should focus as part of implementing the Baseline on your own site.