# Starting Site

The starting sites (one for Kentico Xperience 13 and one for Xperience by Kentico) contain everything hooked up and ready to go.

In the Kentico Xperience 13 instance starts with the Core and Navigation features, as those are the two most widely used.  The rest you can opt-into.

In the Xperience by Kentico instance, it starts with all modules and you can simply remove the nuget packages of the modules you do not want.

## Features

The starting sites contain the following features:

1. Already integrated with the Baseline nuget packages (KX13 Core and Navigation with rest opt in, Xperience by Kentico all are installed with easy opt out)
2. Has the [Front End Dev System](../general/front-end-development.md) for optimized React/Typescript/Javascript/Sass/CSS/Images handling
3. Has a library structure that allows you to have Models, Interfaces, Components that are kentico agnostic for easier testing and migration *
4. Have Layouts that have the highest performing setups already there in terms of css, javascript, your page metadata, and a spot for navigation (preloading, 100% async load javascript after DOM, etc)
5. Have some foundational pieces easy to install (Home Page, Basic Page) **
6. Have a CI/CD Sample (Xperience by Kentico Only)
7. .gitignore samples

* In KX13, recreating and/or faking the TreeNode or any related model was brutal.  Having separation of concerns was vital to an easy upgrade path to Xperience by Kentico as well.  In Xperience by Kentico, neither of these issues apply so you can choose to 'not' separate out your models and such into a Kentico agnostic library.

** Installation of these page types varies between Kentico Xperience 13 and Xperience by Kentico