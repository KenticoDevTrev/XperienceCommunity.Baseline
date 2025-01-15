# Link Accessability

It should be the goal of every website to be accessible.  There are many elements that make up an accessible site, and one of those is proper Aria tagging and descriptions for links.

- Anchors or buttons that are not links but interactive should have `aira-role="button"`
- `title` attributes should be copied to `aira-label`
- Anchors without a `title` and do not contain any inner text should have some description generated
- Links that open in a new window should indicate that action
- Email links should indicate that it's an email link
- Telephone links should indicate this will dial a number
- On page anchor links should indicate they are going somewhere.

The Baseline provides a `LinkTagHelper` that handles making your links accessible for you.  It automatically handles all anchor tags.

## Examples

Below are a set of links to demonstrate how they are transformed.  Some have a blank image to represent a link that does not have any inner text that a screen reader would be able to get context for.

```html
<h2>Links that are buttons</h2>
<a href="#" class="some-hook">Some button link</a>
<a href="#" class="some-hook"><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>
<a href="javascript:void(0)" class="some-other-hook">Some button link</a>
<a class="some-other-hook">Some button link</a>

<a href="#" class="some-hook" bl-render-as-button>Some button link</a>
<a href="#" class="some-hook" bl-render-as-button><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>
<a href="javascript:void(0)" class="some-other-hook" bl-render-as-button>Some button link</a>
<a class="some-other-hook" bl-render-as-button>Some button link</a>

<h2>Anchor Links</h2>
<a href="#Some-Link">Anchor to Some Link</a>
<a href="#Some-Link"><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>

<a href="https://www.google.com" target="_blank">Some button link</a>
<a href="https://www.google.com" target="_blank"><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>

<a href="/home">Goes Home</a>
<a href="/home"><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>

<a href="tel:5555555555">Call Me!</a>
<a href="tel:5555555555">(555) 555-5555</a>
<a href="tel:5555555555"><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>

<a href="mailto:test@localhost">Email Me!</a>
<a href="mailto:test@localhost">test@localhost</a>
<a href="mailto:test@localhost"><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>
```

These are transformed to:
```html
<h2>Links that are buttons</h2>
<a href="#" class="some-hook" title="Some button link" role="button" aria-label="Some button link">Some button link</a>
<a href="#" class="some-hook" title="click to interact" role="button" aria-label="click to interact"><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>
<a href="javascript:void(0)" class="some-other-hook" title="Some button link" role="button" aria-label="Some button link">Some button link</a>
<a class="some-other-hook" title="Some button link" role="button" aria-label="Some button link">Some button link</a>

<button class="some-hook" type="button" role="button" title="Some button link" aria-label="Some button link">Some button link</button>
<button class="some-hook" type="button" role="button" title="" aria-label=""><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></button>
<button class="some-other-hook" type="button" role="button" title="Some button link" aria-label="Some button link">Some button link</button>
<button class="some-other-hook" type="button" role="button" title="Some button link" aria-label="Some button link">Some button link</button>

<h2>Anchor Links</h2>
<a href="#Some-Link" title="Anchor to Some Link" role="link" aria-label="Anchor to Some Link">Anchor to Some Link</a>
<a href="#Some-Link" title="jump to section Some-Link" role="link" aria-label="jump to section Some-Link"><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>

<a href="https://www.google.com" target="_blank" title="Some button link (opens in a new tab)" role="link" aria-label="Some button link (opens in a new tab)">Some button link <span class="sr-only">(opens in a new tab)</span></a>
<a href="https://www.google.com" target="_blank" title="go to url https://www.google.com (opens in a new tab)" role="link" aria-label="go to url https://www.google.com (opens in a new tab)"><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>

<a href="/home" title="Goes Home" role="link" aria-label="Goes Home">Goes Home</a>
<a href="/home" title="go to url /home" role="link" aria-label="go to url /home"><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>

<a href="tel:5555555555" title="Dial Phone Number 5555555555 Call Me!" role="link" aria-label="Dial Phone Number 5555555555 Call Me!"><span class="sr-only">Dial Phone Number 5555555555</span> Call Me!</a>
<a href="tel:5555555555" title="Dial Phone Number (555) 555-5555" role="link" aria-label="Dial Phone Number (555) 555-5555"><span class="sr-only">Dial Phone Number</span> (555) 555-5555</a>
<a href="tel:5555555555" title="Dial Phone Number 5555555555 " role="link" aria-label="Dial Phone Number 5555555555 "><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>

<a href="mailto:test@localhost" title="Send E-mail to test@localhost Email Me!" role="link" aria-label="Send E-mail to test@localhost Email Me!"><span class="sr-only">Send E-mail to test@localhost</span> Email Me!</a>
<a href="mailto:test@localhost" title="Send E-mail to test@localhost" role="link" aria-label="Send E-mail to test@localhost"><span class="sr-only">Send E-mail to</span> test@localhost</a>
<a href="mailto:test@localhost" title="Send E-mail to test@localhost " role="link" aria-label="Send E-mail to test@localhost "><img src="data:image/gif;base64,R0lGODlhAQABAAD/ACwAAAAAAQABAAACADs=" width="0" height="0" alt="pretend-image-link" /></a>

```

## Localization

The Screen Reader Only text are retrieved through the `IStringLocalizer` with fallback values.  If you wish to localize these values to different languages, use the below keys (and default values) to add localization key and values in the Admin

- **link.adagenericbutton**: click to interact
- **link.adagenericlink**: go to url
- **link.adaopeninnewtab**: (opens in a new tab)
- **link.adaphone**: Dial Phone Number
- **link.adaemail**: Send E-mail to
- **link.adagenericanchor**: jump to section

## Widget / string HTML Content

Tag Helpers often do not work with rendered text through the `@Html.Raw()`.  While this is not in the Baseline yet, I will be adding a version of these that can parse and update HTML in a raw string.