# Page Redirection

Sometimes you need a page to redirect elsewhere (possibly without archiving it).  This could be simply to add a vanity page that redirects to an external location, or who knows what.

The Baseline has this functionality built out for both Kentico Xperience 13 and Xperience by Kentico.

Each features these options:

1. No Redirect
2. Internal (select a page)
3. External (type in a URL)
4. First Child (optionally of a specified page type)
5. For any redirect, If to use a Permanent Redirect (301) or not.

## Kentico Xperience 13

For Kentico Xperience 13, it uses the [Xperience Page Navigation Redirect](https://github.com/wiredviews/xperience-page-navigation-redirects) system which you can enable.  You do need to configure this i believe yourself.

## Xperience by Kentico

For Xperience by Kentico, there is an `IBaseRedirect` reusable schema that you can apply to any Web Page Type Class, and the baseline middleware will detect this and operate as configured.  