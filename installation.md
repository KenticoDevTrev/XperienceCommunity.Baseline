# Installation
There are a handful of scenarios when adopting the Baseline, below are the various ways to get started.  The goal is to have a Kentico Xperience 13.0.131 instance which is the minimum requirement for the Baseline v2.

## Fresh Kentico Installation
If you are starting a brand new Kentico instance, please follow these instructions.

1. Download the [Kentico 13 Experience Refresh 11](https://download.kentico.com/Xperience_13_Refresh11.exe) Installing file and install.
2. Run the Installer to install a fresh .net core MVC Solution
   * I recommend creating a blank SQL database first, get the user and login set in it, and when installing to select **Install without database**, and once installation is finished and you run the admin, it will guide you through selecting an existing database and installing Kentico on it.
3. Once your site is installed and you can run the admin application, install the NuGet packages outlined in the section **[Adding required Admin NuGet Packages](#Adding-required-Admin-NuGet-Packages)** 
4. Lastly, follow the instructions in **[Connecting the Starting Site](#Connecting-the-Starting-Site)** section

## Upgrading from Portal KX12
If you are upgrading from 12 Portal Engine, you must first migrate your site to a 12 MVC [using the Kentico12to13Converter](https://github.com/KenticoDevTrev/KX12To13Converter).  Then proceed to **[Upgrading from MVC KX12](#Upgrading-from-MVC-KX12)**

## Upgrading from MVC KX12
If you are upgrading from 12 MVC, follow these step:
  1. Install the Kentico [12 to 13 upgrade installer](https://download.kentico.com/CMSUpgrades/Upgrade/Upgrade_12_0_13_0.exe) (and make sure to follow [instructions when upgrading](https://docs.xperience.io/x/cgmRBg))
  2. Once installed, navigate to the installation folder (usually `C:\Program Files (x86)\Kentico\13.0\Upgrade120_130\SQL`), and run the `upgrade.sql` (or the Separation sql files if you have a separate OM database) against your 12 MVC Database. **Don't forget to backup**
  3. Install the [Kentico 13 Experience installer](https://download.kentico.com/Xperience_13_0.exe) and from it, install a fresh copy of Kentico **with no database**
  4. Once installed, navigate to the CMS folder of your new instance, go to the `CMS\web.config` file and...
     * Turn Debug mode on
	 * Add a connection string to point to your newly 'upgraded' KX13 database (from Step 2) (ex:
	 *  `
<connectionStrings>
<clear  />
<add  name="CMSConnectionString"  connectionString="Data Source=localhost\SQL2022;Initial Catalog=MyUpgradedKenticoDB;Integrated Security=False;Persist Security Info=False;User ID=sa;Password=somesapassword;Connect Timeout=600;Encrypt=False;Current Language=English;"  />
</connectionStrings>
`
	 * Set the connection string to at least 600
 5. Rebuild the WebApp.sln file
 6. Run the site, this may take a while for it to run as this finishes up any upgrade procedures, make sure the event log shows a successful upgrade.
 7. Proceed to **[Hotfixing solution to KX13.0.131](#Hotfixing-solution-to-KX130131)**

## Hotfixing solution to KX13.0.131
If you already have a KX13 but it's not on Hotfix 131, then you need to hotfix it to 131 (you can go higher if you want, you'll just need to make sure to update the nuget packages on the baseline projects)
      1. Follow Kentico's [Hotfix Instructions](https://docs.xperience.io/installation/hotfix-instructions-xperience-13)
      2. Make sure to update any nuget package references, clear our any old DLL references that copied from the `Lib` Folder in other projects that may not be the highest version dll.
      3. Proceed to **[Adding required Admin NuGet Packages](##adding-required-admin-nuget-packages)**, then 

# Adding required Admin NuGet Packages
The Baseline systems does have a dependency on two custom module for the Kentico Admin (for Categories and Relationships).

1. Open you `CMSApp` solution
2. Install the [RelationshipsExtended]([NuGet Gallery | RelationshipsExtended 13.0.9](https://www.nuget.org/packages/RelationshipsExtended)) NuGet Packages on your `CMSApp` project
3. Update the `RelationshipsExtended.Base` related project to the highest version (bug fixes)
4. Install the [XperienceCommunity.PageCustomDataControlExtender](https://www.nuget.org/packages/XperienceCommunity.PageCustomDataControlExtender)
6. Rebuild the `CMSApp`
7. Run the admin, and verify in the event log that the installation was successful.

Optionally, here are some other Admin-specific NuGet packages that are recommended:
* [XperienceCommunity.UrlRedirection.Admin 13.0.9](https://www.nuget.org/packages/XperienceCommunity.UrlRedirection.Admin) (if you use, must install the [XperienceCommunity.UrlRedirection 13.0.12](https://www.nuget.org/packages/XperienceCommunity.UrlRedirection) on the `MVC` project as well)
* [XperienceCommunity.CSVImport.Admin 13.0.0](https://www.nuget.org/packages/XperienceCommunity.CSVImport.Admin)
* [XperienceCommunity.SvgMediaDimensions](https://www.nuget.org/packages/XperienceCommunity.SvgMediaDimensions)

There are many other `XperienceCommunity` prefixed NuGet packages, but most of the rest are for the `MVC` application only.

# Connecting your Site (If not using the Starting Site

***It is recommended that if you are also adopting the baseline starting site, that you start with the whole Baseline Starting Site Solution and then import your site code bit by bit, refactoring as you go***

If you wish to use your own website solution and simply leverage Baseline NuGet packages, keeping in mind that the `XperienceCommunity.Baseline.Core` are required.  Each package is split up into...
* `Models` (Kentico agnostic models and interfaces)
* `Library` (Kentico agnostic code)
* `RCL` (kentico agnostic view components and other razor items)
* `KX13.Models` (kentico required page types or other module classes)
* `KX13.Library` (Kentico implementation)
* `KX13.RCL` (Kentico specific razor items, like page templates or widgets)

## Update to .net 8.0
If a project is only referenced by the MVC Site (and not shared on the admin), make sure those projects are .net 8.0.  This is usually as simple as editing the .csproj and setting the .net version.  [Sean's .net 6 lifecycle article](https://community.kentico.com/blog/xperience-by-kentico-s-net-support-lifecycle-net-6) gives a great outline of what to do.

If the project is referenced by both the Site (MVC) and the Admin, it must remain `.net standard 2.0`

## Reference Baseline NuGet Packages

Lastly, you can now reference the `XperienceCommunity.Baseline` packages.  The `Core` package is required.  From there though you can reference other Baseline packages and systems.  The Wiki eventually will outline all that is available, however roughly you can look at the [older Baseline wiki](https://github.com/HBSTech/Kentico13CoreBaseline/wiki) has many of the features, although **some code samples and items may be out of date**, i'm working on recreating this but time is limited.

I'll link to the talk I gave at the Kentico Connections conference once it's made public.

# Connecting the Starting Site
It is recommended that when adopting the Baseline systems, that you leverage the Baseline Starting Site.  This system is designed to be easy to migrate, test, and designed in such a way to ensure proper separation of concerns.  

## Installation

To install the Starting site, please follow these instructions:

1. Clone the KX13 Starting Site
2. Put the Admin solution in the `KenticoAdmin` folder (this should have your `CMS`, `Lib`, `packages`, `WebApp.sln`, `GlobalAssemblyInfo.c`, and `log.txt` files directly under `KenticoAdmin`)
3. If using IIS instead iis express, update your site reference to point to the new folder and update file permissions.
4. In your `WebApp` solution, add the `Site.UniversalModels` and `XperienceModels` projects that are a sibling to `KenticoAdmin` folder and reference them on the `CMSApp` project, these are `.net standard 2.0` projects that contain your shared Kentico classes and any models that are shared between Admin and MVC
5. Navigate to `MVC\FrontEndDev` and run in terminal `npm install` and `npm build` to build the initial front end development files (if you don't have NodeJS, [install it](https://nodejs.org/en/download), the baseline is using 9.5 but should work fine with higher versions)
6. Navigate to the `MVC\MVC` folder, clone the `appsettings.json` and name the new copy `appsettings.development.json` (this isn't tracked in source control), and add in your connection strings, CMSHash salt, and any other values you wish.
7. Open the `MVC\MVC.sln` , restore nuget packages, and run the site.

## Optional Baseline Site Page Types
The Baseline site offers a handful of useful page types to leverage, found in the `Baseline_Generics.1.0.0.zip` folder in the starting site.  Import this into your Kentico admin if you wish by following these instructions:

1. Log into your Kentico Admin site
2. Go to `Sites` -> `Import site or objects`
3. Upload the `Baseline_Generics.1.0.0.zip` and import
4. Once imported, go to `Page Types` and edit  the new `Generic` page types, going to `Sites` on the left sidebar nav and assigning to the current site.

## Import your code
Now that both the Admin and MVC Sites are connected, you can start migrating any code or rebuilding any code within it.  This may include things like:

* Generate any Page Type or Module classes into the `XperienceModels` library
* Add any shared Event Hooks that should apply to both Admin and MVC to the `XperienceModels` library
* Add any custom module interfaces into the `CMS` application
* Start building out widgets, templates, etc into the `MVC` solution (recommended splitting things into Kentico agnostic view components, models, and interfaces where possible)
* Adjust the `FrontEndDev` `taskconfigs.js` to build out your bundle packages
* Adjust the `_Layout.cshtml` to fit your site's design and file requirements.


# Questions / Bugs
If you encounter a bug, please create a pull request.  Additionally, please feel free to email me at tfayas at (gmail).  I'm also on the [kentico-community.slack.com](kentico-community.slack.com) slack channel, if you don't have access email me and i'll grant you access.