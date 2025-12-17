
# XperienceCommunity.Baseline

Core Systems, Tools, and Structure to ensure a superior Kentico Website that's easy to migrate, for Kentico Xperience 13 and eventually Xperience by Kentico

**NOTE** If you are not developing in Kentico Xperience 13, I would honestly recommend you do NOT use the Baseline v2, or if you use it only pull in the necessary pieces.  See the Goal and History section below for more.

**[View Installation Instructions and Starting Site Setup for Xperience by Kentico](documentation/site/site-xbyk-setup.md)**

**[View Installation Instructions and Starting Site Setup for KX13](documentation/site/site-kx13-setup.md)**

Special thanks to my previous employer [Heartland Business Systems](https://www.hbs.net) who supported me as I created this baseline, adding things we all learned on projects into this (and other repositories) and allowing me to give them away to the Kentico Community. If you need a kentico partner to implement a Baseline project, I would definitely get in touch with them.

Also thanks to my other previous employer [Movement Mortgage](https://www.movement.com) who shares my desire to serve and bless others, and allowed me to get the Version 2 up that got things prepared for the Xperience by Kentico implementation.

Lastly thanks to my wife for bearing with me as I stressed and had near-nervous breakdowns creating the Xperience by Kentico Implementation by my self-imposed Christmas Deadline (12/22/2024 at 3:00 am as i write this).

## Please Star / Send a Linked In Message

If you use this repo, please send me a message or star the project, I put hundreds of hours into this project over the years, and sometimes hard to know if anyone actually uses it...

## Goal and History

The Baseline's goal is to provide a framework and tools that all sites need in order to succeed.

It started with Kentico Xperience 12 MVC and built out a lot of functionality that was missing.

It then evolved to support Kentico Xperience 13 MVC5, and then upgraded to MVC .net Core (v1.0.0)

Later it morphed into a structure that would make it easy for Kentico Xperience 13 sites to be able to upgrade to Xperience by Kentico (Kentico agnostic core functionality, then implemented in Kentico Xperience 13), and also got things ready for Xperience by Kentico (v2.0.0)

Finally, I was able to implement it in Xperience by Kentico (v2.3.0), which now provides a great turn-key solution for people looking to use Xperience by Kentico.

Xperience by Kentico was still missing many key features at the time which the Baseline saught to fill in, however now the platform is robust and many of these work-arounds are no longer needed, thus the Baseline v2 is not a good choice for starting out with a new Xperience by Kentico Project, wait for v3 (mid 2026) or simply look and grab what functionality / logic you want from this baseline for your own usage until then.

### Final Baseline v2 - Future v3

Version 2.10 is the final version of the Baseline that features "Dual Support" for Kentico Xperience 13 and Xperience by Kentico.  In 2026, we plan on creating a brand new Baseline that will be Xperience Only, and will focus on leveraging all the new Xperience Features without all the complex Project Structure / Hacks / "Kentico Agnostic" projects since people will no longer are developing on Kentico Xperience 13 (or at least shouldn't be).  Version 3 will also be built in .net 10.

### Documentation

Documentation is my next goal, I'll be focusing on the Xperience by Kentico at this point as you should be building on that platform vs. KX13.

**See more about the design principle in my talk ['Patterns for an easy-to-migrate and easy-to-maintain website'](https://www.kentico.com/presentation/patterns-for-an-easy-to-migrate-and-easy-to-mainta)**

## Current Version: 2.x

As each refresh is launched, the baseline is being updated to the latest Xperience Refresh.  Below is a table of the Baseline version to the Refresh

Baseline => Xperience
- v2.10 => 31.0.0
- v2.9 => 30.6.3
- v2.8 => 30.5.4
- v2.7 => 30.4.0
- v2.6.1 => 30.3.0
- v2.5 => 30.2.0
- v2.4 => 30.1.0
- v2.3 => 30.0.0

## Version: 2.10 (December 17, 2025)

Xperience released it's version 31.0.0 Refresh (with .net 10 support) as well as the final refresh for Kentico Xperience 13.0.197 with .net 10 support.  This will be the final version of the Baseline v2 (except bug fixes)

## Version: 2.9 (September 9, 2025)

There was a bug in the Membership roles that caused Database connections to not close.  This update brings it up to Hotfix 30.6.3 as well as fixes the reference to that.

## Version: 2.3 (December 22, 2024)

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
