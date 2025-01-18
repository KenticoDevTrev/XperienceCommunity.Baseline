# FUTURE - Media and Link Accessibility from String HTML

Currently, the Media and Link Accessibility features use AspNet Core's Tag Helper system, which run on compile time.

This means that if you have any rich text from a Widget or from a Long Text field, these links and images won't undergo the same vital processing (alt tags, screen reader helpers, etc).

I plan on, in the future, creating a service that will use Regex or an HTML parser to look for the \<img> and \<a> tags and perform the same operations on them.  This will be there probably before June 2025 (as i need it for my own site).