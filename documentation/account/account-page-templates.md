# Account Page Templates

In the `RCL._____` package, there contains the Page Templates for the `Account` Page Type.

The goal of the `Account` Page type was to allow you to create all your Account page types easily (similar to way back in the day of Kentico 12 Portal Engine where it had all the account page templates).

The Account Page Templates all post to the Controllers found in the [Account.RCL Features](../../src/Account/Account.RCL/Features/Account/) for their Post Logic, and then those controllers handle the logic, store the Model State, and redirect back to your page.

Please note that creating the `Account` pages is **OPTIONAL**, the Controllers have their own default routes and will operate using those routes if no pages and URLs are configured.

## Adding Pages

Once you have added the Account page (this is created automatically for Xperience by Kentico, or for Kentico Xperience 13 it's in the `Baseline_Generics.1.0.0.zip`), and you have added it to your site/channel, you can add the pages.

Assuming you properly added the template assembly tags, you'll be asked to select from the various templates, each one being a different part of User Registration.  Create your page, then mark the URL and configure it.  After configuration, the Controllers (upon performing their action) will redirect to that URL (if none is set, it will redirect to the Controller's own action as a fall back).

## Configuring Urls 

Configuring the Urls so the Controllers know where to redirect back to is configured different in Xperience by Kentico than Kentico Xperience 13.

### Xperience by Kentico

There are [Channel Settings](https://github.com/KenticoDevTrev/XperienceCommunity.ChannelSettings) for the [AccountChannelSettings](../../src/Account/Account.Library.Xperience/Models/AccountChannelSettings.cs) (Configured in the [AccountChannelSettingsFormAnnotated](../../src/Account/Account.Admin.Xperience/Models/AccountChannelSettingsFormAnnotated.cs)).  These are found in the Admin -> Channel Management -> (Select Channel) -> Account URL Settings.

### Kentico Xperience 13

In Kentico Xperience 13, the settings are installed in the `Baseline_Generics.1.0.0.zip` import file.  Then they are located in the Settings Module under Settings -> URLs and SEO (near the bottom).

