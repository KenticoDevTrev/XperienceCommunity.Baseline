
# XperienceCommunity.Baseline

Core Systems, Tools, and Structure to ensure a superior Kentico Website that's easy to migrate, for Kentico Xperience 13 and eventually Xperience by Kentico

Special thanks to my previous employer [Heartland Business Systems](https://www.hbs.net) who supported me as I created this baseline, adding things we all learned on projects into this (and other repositories) and allowing me to give them away to the Kentico Community. If you need a kentico partner to implement a Baseline project, I would definitely get in touch with them. If you need some small consulting assistance with it, please [reach out to me - tfayas@gmail.com](mailto:tfayas@gmail.com).

Also special thanks to my current employer [Movement Mortgage](https://www.movement.com) who shares my desire to serve and bless others, and is excited to use this new baseline and grow it as our team works on their site. If you need a mortgage or insurance, they put their profits towards building schools, churches, and supporting ministries and organizations to help marginalized communities in the name of Jesus.

## Goal

This Baseline's goal is to provide a framework and structure to build Kentico Xperience 13 websites in such a way that they will be easily migrated to Xperience by Kentico, as well as leveraging SOLID coding principles, Internet/SEO/Accessibility best practices, and other site optimizing principles. It also serves as a "Recipe Bin" for widgets, code samples, and other things people may want to contribute to help others as a community.


## Current Version: 2

Version 2 (nuget packages 2.X.X) is built on .net 8.0, and support Kentico Xperience 13.0.131+ (.net 8 support). The starting site (starting-site/kx13) will exclude jQuery by default, and has been refactored to be more aligned with Xperience by Kentico.
  
An Xperience by Kentico starting site, and implementations should be available over the next coming year, however you can easily look at the KX13 code and implement the logic pieces yourself and still use it.

If you are using a current version of the Baseline and wish to migrate to Version 2, please see the [Version 2 Changes Log and Migration readme](https://github.com/KenticoDevTrev/XperienceCommunity.Baseline/blob/master/Version2ChangeLogAndMigration.md)

## Version 1 (Retired)

Version 1 (nuget packages 1.X.X) is built on .net 6.0, and supports Kentico Xperience 13.0.5 (.net 6 support) and onwards. The starting site (starting-site/kx13) should only be used for Kentico Xperience 13.0.5 through Kentico Xperience 13.0.130, as in hotfix 131 new features were implemented and form javascript files changed.  You may continue to use version 1 at and 13.0.130 if you re-copy the [updated javascript files](https://github.com/KenticoDevTrev/XperienceCommunity.Baseline/tree/master/starting-site/kx13/MVC/FrontEndDev/js/bundles/form-bundle).

If you wish to access the Version 1 starting site, or any code for version 1, or submit a bug fix for version 1, please use the [master-v1 branch](https://github.com/KenticoDevTrev/XperienceCommunity.Baseline/tree/master-v1)

## Status

January 1, 2024, the Baseline v2 is ready for general usage. There is still a couple things left to do as listed below, but it's being used on my current project

1. Add Installation steps for the Core, Navigation, Search, and Localization Modules
2. Add Customization points for Navigation, Search, and Localization Modules
6. Add Installation steps for Account, Ecommerce, and Tabbed Pages Modules
7. Add Customization points for Account, Ecommerce, and Tabbed Pages Modules
  
I also plan on working on the Xperience by Kentico implementations of each module in the near future.

## Questions

If you have questions, please reach out to me at tfayas(at)gmail
