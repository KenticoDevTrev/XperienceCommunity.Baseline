# User and User Context

Getting the current User (*Member* in Xperience by Kentico) is also often needed in normal website operations.

The Baseline comes with an [IUserRepository](../../src/Core/Core.Models/Repositories/IUserRepository.cs) which can be used to Get the current user, or a given user.  It also presents a fake `Public` user if there is no actual current user.

The system uses the current HttpContext's User Identity Name and does a lookup for a user found there.  It will look up based on the MemberName and MemberEmail (Xperience by Kentico) or UserName and Email (Kentico Xperience 13).

## Xperience By Kentico Only

### Customizing User (Xperience by Kentico Only)

Kentico provides documentation on how to [add fields to your Member object](https://docs.kentico.com/developers-and-admins/development/registration-and-authentication/add-fields-to-member-objects).  It is your responsibility though to then tell the Baseline about your new `ApplicationUser` inherited type, and to set or retrieve those values.

The Baseline User Model contains a `Maybe<IUserMetadata> MetaData` property which is used as a catch all for any customizations you wish to do.  This is where you should store and retreive these custom fields.

You do this in the service [IBaselineUserMapper.cs](../../src/Core/Core.Library.Xperience/Services/IBaselineUserMapper.cs) that has a default [implementation](../../src/Core/Core.Library.Xperience/Services/Implementation/BaselineUserMapper.cs), that you can [override](../customization-points.md).  In here you can use type-checking to retrieve and parse additional information to the `MetaData` property (and from the MetaData to the ApplicationUser for saving).  This is how you can add additional properties and fields for your purposes.

### ApplicationUserBaseline

The Baseline provides it's own ApplicationUser ([ApplicationUserBaseline](../../src/Core/Core.Library.Xperience/Models/ApplicationUserBaseline.cs)) since Xperience by Kentico doesn't have the First, Middle, and Last Names as part of it's core Member object.  It also has in it's Baseline Core Installer where these fields are added to the Member Object for you.

You can inherit from this model and add your own properties as you wish!

## User Management

User management functionality is contained in the Baseline Account Module.  The Core only contains the basic data of retrieval and context.