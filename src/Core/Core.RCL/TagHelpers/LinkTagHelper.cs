using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using System.Text.RegularExpressions;

namespace Core.TagHelpers
{
    [HtmlTargetElement("a", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class LinkTagHelper(IStringLocalizer<SharedResources> stringLocalizer) : TagHelper
    {
        private readonly IStringLocalizer<SharedResources> _stringLocalizer = stringLocalizer;

        public override int Order => 20;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var linkTextHtml = (await output.GetChildContentAsync()).GetContent();
            var linkTextNoHtml = linkTextHtml.RemoveHtmlTags();

            var contentPostfixHtml = "";
            var contentPrefixHtml = "";
            var ariaRole = "link";
            var title = output.Attributes.TryGetAttribute("title", out var titleAttribute) && titleAttribute.Value is string titleVal ? titleVal.AsNullOrWhitespaceMaybe() : Maybe.None;
            var linkTextHasValue = linkTextNoHtml.AsNullOrWhitespaceMaybe().HasValue;
            var noLinkTextNorTitle = !linkTextHasValue && title.HasNoValue;

            if (!output.Attributes.TryGetAttribute("href", out var href)) {
                ariaRole = "button";
                if (linkTextHtml.AsNullOrWhitespaceMaybe().HasNoValue && title.HasNoValue) {
                    linkTextNoHtml = _stringLocalizer.GetStringOrDefault("link.adagenericbutton", "click to interact");
                }
            } else {
                var hrefVal = href.Value?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(hrefVal) || hrefVal.Equals("#") || hrefVal.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)) {
                    ariaRole = "button";
                    if (linkTextNoHtml.AsNullOrWhitespaceMaybe().HasNoValue && title.HasNoValue) {
                        linkTextNoHtml = _stringLocalizer.GetStringOrDefault("link.adagenericbutton", "click to interact");
                    }
                } else {
                    // Links opening in new tabs
                    if (output.Attributes.TryGetAttribute("target", out var targetAttr) && (targetAttr.Value?.ToString() ?? "").Equals("_blank", StringComparison.OrdinalIgnoreCase)) {
                        if (linkTextNoHtml.AsNullOrWhitespaceMaybe().HasNoValue && title.HasNoValue) {
                            linkTextNoHtml = $"{_stringLocalizer.GetStringOrDefault("link.adagenericlink", "go to url")} {hrefVal}";
                        }
                        var srOpenInNewTab = _stringLocalizer.GetStringOrDefault("link.adaopeninnewtab", "(opens in a new tab)");
                        linkTextNoHtml += $" {srOpenInNewTab}";
                        contentPostfixHtml = $" <span class=\"sr-only\">{srOpenInNewTab}</span>";
                    }
                    // Telephone Links
                    else if (hrefVal.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)) {
                        var dialText = _stringLocalizer.GetStringOrDefault("link.adaphone", "Dial Phone Number");
                        if (Regex.IsMatch(linkTextHtml, ".*[0-9]{3}.*")) {
                            linkTextNoHtml = $"{dialText} {linkTextNoHtml}";
                            contentPrefixHtml = $"<span class=\"sr-only\">{dialText}</span> ";
                        } else {
                            var telNum = hrefVal.Replace("tel:", "");
                            linkTextNoHtml = $"{dialText} {telNum} {linkTextNoHtml}";
                            contentPrefixHtml = $"<span class=\"sr-only\">{dialText} {telNum}</span> ";
                        }
                    }
                    // Email Links
                    else if (hrefVal.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) {
                        var sendEmailToText = _stringLocalizer.GetStringOrDefault("link.adaemail", "Send E-mail to");

                        if (linkTextHtml.Contains('@')) {
                            linkTextNoHtml = $"{sendEmailToText} {linkTextNoHtml}";
                            contentPrefixHtml = $"<span class=\"sr-only\">{sendEmailToText}</span> ";
                        } else {
                            var email = hrefVal.Replace("mailto:", "");
                            linkTextNoHtml = $"{sendEmailToText} {email} {linkTextNoHtml}";
                            contentPrefixHtml = $"<span class=\"sr-only\">{sendEmailToText} {email}</span> ";
                        }
                    } else {
                        // general links, anchor or not
                        if (hrefVal.StartsWith("#")) {
                            if (noLinkTextNorTitle) {
                                linkTextNoHtml = $"{_stringLocalizer.GetStringOrDefault("link.adagenericanchor", "jump to section")} {hrefVal.Trim('#')}";
                            }
                        } else {
                            if (noLinkTextNorTitle) {
                                linkTextNoHtml = $"{_stringLocalizer.GetStringOrDefault("link.adagenericlink", "go to url")} {hrefVal}";
                            }
                        }
                    }
                }
            }

            if (linkTextHasValue && !string.IsNullOrWhiteSpace(contentPrefixHtml)) {
                output.PreContent.AppendHtml(contentPrefixHtml);
            }
            if (linkTextHasValue && !string.IsNullOrWhiteSpace(contentPostfixHtml)) {
                output.PostContent.AppendHtml(contentPostfixHtml);
            }

            output.Attributes.AddorReplaceEmptyAttribute("title", linkTextNoHtml);
            output.Attributes.AddorReplaceEmptyAttribute("role", ariaRole);
            output.Attributes.AddorReplaceEmptyAttribute("aria-label", linkTextNoHtml);
        }
    }

    [HtmlTargetElement("a", Attributes = "[bl-render-as-button]", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ButtonLinkTagHelper : TagHelper
    {
        [HtmlAttributeName("bl-render-as-button")]
        public bool UseThisThing { get; set; } = true;

        public override int Order => 10;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (output.Attributes.ContainsName("href")) {
                output.Attributes.Remove(output.Attributes["href"]);
            }
            if (output.Attributes.ContainsName("target")) {
                output.Attributes.Remove(output.Attributes["target"]);
            }
            output.TagName = "button";
            output.Attributes.AddorReplaceEmptyAttribute("type", "button");
            output.Attributes.AddorReplaceEmptyAttribute("role", "button");
        }
    }
}
