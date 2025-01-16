# Identity System

In an effort to keep things Kentico Agnostic, lookup values that are often passed to and from methods are converted to various `Identity` objects.  This also stems from during upgrading, the link between one object and another in Kentico Xperience 13 may be different than in Xperience by Kentico, so your method can remain the same but what value you take from the identity may differ in each implementation.

## Identity Types

Here are the Identity Types:

- `ObjectIdentity`: Represents an Object (Id, Guid, CodeName)
- `ContentIdentity`: Represents a Content Item (Id, Guid, CodeName)
- `ContentCultureIdentity`: Language Specific version of `ContentIdentity` (Id, Guid, and ContentID+CultureName lookup)
- `TreeIdentity`: Represents an Page (Id, Guid, Name, Path+ChannelID lookup)
- `TreeCultureIdentity`: Language specific version of `TreeIdentity` (adds a Culture property)
- `PageIdentity`: Combination of `TreeCultureIdentity`, `TreeIdentity`, `ContentCultureIdentity`, `ContentIdentity`, represents a routed Page and all it's data.

## Converting to Identity

To create an Identity, you often are doing one of two things:

1. Converting a static value (int, Guid, string) to an Identity
    - Use the [ToObjectIdentityHelper Extensions](../../src/Core/Core.Models/Extensions/ToObjectIdentityHelper.cs) or [ToContentAndContentCultureIdentityHelper Extensions](../../src/Core/Core.Models/Extensions/ToContentAndContentCultureIdentityHelper.cs)
2. Converting an existing Identity of one type to another
    - Use the Extension Methods on the various Identity classes that allow you to convert from one to another.
3. Converting an existing object to one type or another
    - Use the Constructor Methods on the Identity fields.  You can have your property inherit from [IObjectIdentity](../../src/Core/Core.Models/Interfaces/IObjectIdentity.cs) an implement the `ToObjectIdentity` method to help.

## IIdentityService

The properties in the Identity types are all `Maybe<>` typed, becuase you may only have one property or the other when you are generating a value.  Your service that receives the identity may need the `id` in one case, or the `guid` in another.

The `IIdentityService` uses what values are provided (ex `id`) to retrieve the other values for the given object, and returns a Result<T> of the Identity with all values filled in.  The implementation is optimized and cached, so if you have 100 `Identity` and you try to get one value that isn't in them, it still should be one or two cached calls total to retrieve all 100 items.

To streamline this processes of making sure you have the property you need, each Identity has extenstions to `GetOrRetrieve___` methods, which allow you to pass the IIdentityService and retrieve that specific value.  If the Identity has the value you want, it simply returns it, if not it uses the `IIdentityService` you pass to find the value based on the other given values available on the object.

Here's a code sample where we recived a `ContentIdentity` and we need the GUID field from it:

```csharp
if (!(await contentItem.GetOrRetrieveContentGuid(_identityService)).TryGetValue(out var contentItemGuid, out var error)) {
    return Result.Failure(error);
}
// I now have contentItemGuid value
```

## Comparison

There is an [ObjectIdentityComparer](../../src/Core/Core.Library/Comparers/ObjectIdentityComparer.cs) which can be used to compare two ObjectIdentities

