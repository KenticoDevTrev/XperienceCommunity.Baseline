
# XperienceCommunity.Baseline (v1)
Core Systems, Tools, and Structure to ensure a superior Kentico Website that's easy to migrate, for Kentico Xperience 13 and eventually Xperience by Kentico

Special thanks to my previous employer [Heartland Business Systems](https://www.hbs.net) who supported me as I created this baseline, adding things we all learned on projects into this (and other repositories) and allowing me to give them away to the Kentico Community.  If you need a kentico partner to implement a Baseline project, I would definitely get in touch with them.  If you need some small consulting assistance with it, please [reach out to me - tfayas@gmail.com](mailto:tfayas@gmail.com).

Also special thanks to my current employer [Movement Mortgage](https://www.movement.com) who shares my desire to serve and bless others, and is excited to use this new baseline and grow it as our team works on their site.  If you need a mortgage or insurance, they put their profits towards building schools, churches, and supporting ministries and organizations to help marginalized communities in the name of Jesus.

## Goal
This Baseline's goal is to provide a framework and structure to build Kentico Xperience 13 websites in such a way that they will be easily migrated to Xperience by Kentico, as well as leveraging SOLID coding principles, Internet/SEO/Accessability best practices, and other site optimizing principles.  It also serves as a "Recipe Bin" for widgets, code samples, and other things people may want to contribute to help others as a community.  

## Version 1
This branch is for Version 1 (nuget packages 1.X.X), and is built on .net 6.0, and supports Kentico Xperience 13.0.5 (.net 6 support) and onwards.  The starting site (starting-site/kx13) should only be used for Kentico Xperience 13.0.5 through Kentico Xperience 13.0.130, as in hotfix 131 new features were implemented and form javascript files changed.

**If you are going to go directly to Version 13.0.130 or above, please use Version 2.**

## Status
Version 1 was rebuilt and ready for usage from August 3rd, 2023 onwards.  While there were still some areas of customization i wanted to get into v1, the majority of the functionality was working well.  

This branch (master-v1) is largely an **archive branch**, as V2 is now available and should be used going forward.

Version 2 supports Kentico Xperience 13.0.130 solutions that are on .net 8, and in 2024 will eventually have an Xperience by Kentico implementation.  If you are below hotfix 13.0.130, or on .net 6 or below, you can still use Version 1 and upgrade to Version 2 at a later time, functionality wise they are identical, the only differences are that version 2 has:

1. Property names geared towards Xperience by Kentico (with older methods obsoleted)
2. Classes refactored to records where appropriate
3. Uses .net 8 vs. .net 6
4. Has a starting site that removes jQuery dependency as this is now possible in KX 13.0.130

## Questions
If you have questions, please reach out to me at tfayas(at)gmail
