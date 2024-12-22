
# XperienceCommunity.Baseline

Core Systems, Tools, and Structure to ensure a superior Kentico Website that's easy to migrate, for Kentico Xperience 13 and eventually Xperience by Kentico

**[View Installation Instructions and Starting Site Setup for Xperience by Kentico](installationXbyK.md)**

**[View Installation Instructions and Starting Site Setup for KX13](installationKX13.md)**

Special thanks to my previous employer [Heartland Business Systems](https://www.hbs.net) who supported me as I created this baseline, adding things we all learned on projects into this (and other repositories) and allowing me to give them away to the Kentico Community. If you need a kentico partner to implement a Baseline project, I would definitely get in touch with them.

Also thanks to my current employer [Movement Mortgage](https://www.movement.com) who shares my desire to serve and bless others, and allowed me to get the Version 2 up that got things prepared for the Xperience by Kentico implementation.

Lastly thanks to my wife for bearing with me as I stressed and had near-nervous breakdowns creating the Xperience by Kentico Implementation by my self-imposed Christmas Deadline (12/22/2024 at 3:00 am as i write this).

## Please Star / Send a Line

If you use this repo, please send me a message or star the project, I put hundreds of hours into this project over the years, and sometimes hard to know if anyone actually uses it...

## Goal and History

The Baseline's goal is to provide a framework and tools that all sites need in order to succeed.

It started with Kentico Xperience 12 MVC and built out a lot of functionality that was missing.

It then evolved to support Kentico Xperience 13 MVC5, and then upgraded to MVC .net Core (v1.0.0)

Later it morphed into a structure that would make it easy for Kentico Xperience 13 sites to be able to upgrade to Xperience by Kentico (Kentico agnostic core functionality, then implemented in Kentico Xperience 13), and also got things ready for Xperience by Kentico (v2.0.0)

Finally, I was able to implement it in Xperience by Kentico (v2.3.0), which now provides a great turn-key solution for people looking to use Xperience by Kentico.

### Documentation

Documentation is my next goal, I'll be focusing on the Xperience by Kentico at this point as you should be building on that platform vs. KX13.

**See more about the design principle in my talk ['Patterns for an easy-to-migrate and easy-to-maintain website'](https://www.kentico.com/presentation/patterns-for-an-easy-to-migrate-and-easy-to-mainta)**

## Current Version: 2.3 (December 22, 2024)

Version 2.3 is built on .net 8.0 and supports both Kentico Xperience 13.0.131+ as well as Xperience by Kentico 30.0.0.  All modules except Ecommerce have been implemented.

Migration from Kentico Xperience 13 to Xperience by Kentico though still may prove a little tricky, as all the functionality is there, but some data has been moved around to align with Xperience by Kentico (ex using Content Assets vs. Media Files, using Reusable Schemas instead of NodeCustomData, etc)

Reach out to me if you need ideas and help.

## Version: 2 (Transitional, January 1, 2024)

Version 2 (nuget packages 2.X.X) is built on .net 8.0, and support Kentico Xperience 13.0.131+ (.net 8 support). The starting site (starting-site/kx13) will exclude jQuery by default, and has been refactored to be more aligned with Xperience by Kentico.

If you are using a current version of the Baseline and wish to migrate to Version 2.3, please see the [Version 2 Changes Log and Migration readme](https://github.com/KenticoDevTrev/XperienceCommunity.Baseline/blob/master/Version2ChangeLogAndMigration.md)

## Version 1 (Retired)

Version 1 (nuget packages 1.X.X) is built on .net 6.0, and supports Kentico Xperience 13.0.5 (.net 6 support) and onwards. The starting site (starting-site/kx13) should only be used for Kentico Xperience 13.0.5 through Kentico Xperience 13.0.130, as in hotfix 131 new features were implemented and form javascript files changed.  You may continue to use version 1 at and 13.0.130 if you re-copy the [updated javascript files](https://github.com/KenticoDevTrev/XperienceCommunity.Baseline/tree/master/starting-site/kx13/MVC/FrontEndDev/js/bundles/form-bundle).

If you wish to access the Version 1 starting site, or any code for version 1, or submit a bug fix for version 1, please use the [master-v1 branch](https://github.com/KenticoDevTrev/XperienceCommunity.Baseline/tree/master-v1)

## Questions

If you have questions, please reach out to me at tfayas(at)gmail
