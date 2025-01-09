# What is the Baseline?
I was once asked "can you give me what the baseline is in 10 seconds?" and to be honest, I don't think I could and give it justice.  

The Baseline is many things.  It's a Boilerplate, it's a turn-key solution, it's a toolbox of tools, it's also a schematic for your own customizations, and overall it's my (Trevor Fayas)'s gift to the Kentico Community.

## Origins - The Story of the Baseline
The baseline started way back when Kentico 12 MVC came out.  Many developers at the time that were spoiled with Kentico's Portal Engine "the All in one CMS Kentico" where everything was already out of the box.  Kentico had baked in systems for everything, and handled everything (not always well or speedily), so the entire platform was a 'baseline' so to say.

However, when Kentico 12 MVC came out, we had to start over.  Basic things we took for granted like Sitemaps, Navigation, Ecommerce, User sign in and management all now had to be recreated.  It gave great flexibility, but that also came with:

**Every kentico partner, big and small, needing to rebuild the same core features again.**

Some large Kentico partners were fine because they could afford to invest the 100-200 hours to build their own starting sites and toolboxes they would use for their clients, but many partners (and simple end users of Kentico) struggled with this.

So, thanks to [Heartland Business Systems](https://www.hbs.net), my employer at the time, they green-lit me not only creating the boilerplate for our own usage, but then making it open sourced to help the community.

### Version 0 - Kentico 12 MVC 5
The first version was, to be honest, very poorly coded (to be fair, I was brand new to MVC coding which most were).  It did contain many of the features that sites needed, but wasn't very flexible.  It had hard-coded object types and no separation of concerns, etc.  It wasn't long though until the next version came.

### Version 0.5 - Kentico Xperience 13 MVC 5
This version was largely no different than the Version 0, except it used Kentico Xperience 13.  I upgraded to support those on the version 0 Baseline so they could easily move to Kentico Xperience 13 for support.  Not many people used this one, as .Net Core was going to be the future.

### Version 1 - Kentico Xperience 13 MVC .Net Core
Once 13.0.5 came out and .Net Core was supported, the task began to upgrade the Baseline into something better, leveraging .Net core features.  

As a Kentico MVP, I also was aware that Kentico was planning on a complete rebuild of their admin structure, which meant a whole new Xperience (pun intended) for customers in the future.

Xperience by Kentico was still in it's infancy at the time, and people were faced with the dreadful decision of "do we build in Kentico Xperience 13, knowing that our project may be a complete wash and we have to rebuild again next version?" or "do we wait on Portal Engine until Xperience by Kentico is ready?"

The Baseline Version 1.0 was built to solve these problems.  It was created with a "Kentico Agnostic" mindset, where all the models, interfaces, and systems were built (to the highest degree) without Kentico references.  Then, an implementation layer was built ***using*** Kentico Xperience 13's library and tools to implement this.  The idea would be then that as long as you followed the same separation mindset, going to Xperience by Kentico would mean just re-implementing those generic interfaces using Xperience By Kentico's libraries.

### Version 2.0 - Kentico Xperience 13 MVC .Net Core with Xperience by Kentico Focus
As Xperience by Kentico finally started to mature, and the database structures started to solidify, I found that many of the Kentico agnostic models were largely still geared towards Kentico Xperience 13.  Concepts such as `Node`, `Document`, etc were all throughout the generic types.  Xperience by Kentico had new concepts such as the Content Hub, `Content Item`, `Web Page Item`, etc.  So Version 2.0 was a Version 1.0 compatible intermediate step that obsoleted old properties and instructed what new properties and methods to use.  It maintained backwards compatibility, but started getting users on the Kentico Xperience 13 Baseline thinking about what the upgrade path would look like, and where things would be stored.

### Version 2.3+ - Xperience by Kentico with Kentico Xperience 13 MVC .Net Core Support
Finally, the time came for me to implement the Baseline in Xperience by Kentico.  At this time I had become the owner of [Physics Classroom](https://www.physicsclassroom.com) and had the freedom to spend the 100-200 hours to rebuild all the core systems.  **This was not easy** was there were systems that Xperience by Kentico didn't have that I had to create myself (such as Member Roles).  But, after a final push on December 21st (ending at 3am on Dec 22nd), I finally finished and published the NuGet packages.