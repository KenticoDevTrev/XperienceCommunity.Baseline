# Account Overview

The Account Module is designed to be a working complete sample of Membership handling on a website.  It is usable as-is, and with [standard view customization](../general/customization-points.md) you can overwrite and modify all the core systems to meet your design needs.  However be aware that in most cases you may decide to install only the Library logic and not the `RCL` classes and create your own controllers and systems to your liking.

Most sites will have more than the basic User Registration and user models, and may have other advanced workflows and processes.  The Baseline's Libraries (without the RCL) still provide flexibility and should be usable for any account system, so please remember you don't need the `RCL` libraries to still get all the benefits the Baseline offers for account management.

## Post Redirect Get vs. htmx

The Baseline was designed using a custom [Post Redirect Get](https://en.wikipedia.org/wiki/Post/Redirect/Get) methodology that allows you to have a PageBuilder page, POST to a controller, store the `ModelState` (validation) of forms (such as Registration or Login) in TempData (either through a cookie or session state), Redirect back to the PageBuilder page, and the Get the ModelState from the TempData and hydrate the current ModelState, allowing forms to show validation messages and such.

This system is, well, complicated, and depends on a lot of things set up correctly (see `persistantStorageConfiguration` options in the `AddCoreBaseline`).

A better approach would probably to use a lightweight library called [htmx](https://htmx.org/), which allows you to easily convert a \<form> into an Ajax-form that posts to the given controller, and returns the result with validation.  It does this through simple html attributes, and makes it where you don't need the `Post Redirect Get` system because you never leave the current page.

You can see this in action on the [Xperience By Kentico Community Portal](https://github.com/Kentico/community-portal/blob/7c260caf09ab70ec3fd276cf56e0a5253c8e622e/src/Kentico.Community.Portal.Web/Features/QAndA/Components/Form/QAndAQuestionForm.cshtml#L25)

