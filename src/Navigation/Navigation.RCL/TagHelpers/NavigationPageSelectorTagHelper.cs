﻿using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Navigation.TagHelpers
{
    [HtmlTargetElement("bl:navigation-page-selector")]
    public class NavigationPageSelectorTagHelper(
        IPageContextRepository _pageContextRepository,
        IUrlResolver _urlResolver) : TagHelper
    {
        [HtmlAttributeName("x-parent-class")]
        public string ParentClass { get; set; } = string.Empty;

        [HtmlAttributeName("x-current-page-path")]
        public string CurrentPagePath { get; set; } = string.Empty;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (CurrentPagePath.AsNullOrWhitespaceMaybe().TryGetValue(out var currentPagePath))
            {
                if (currentPagePath.Equals("/Home", StringComparison.OrdinalIgnoreCase))
                {
                    // Special case for Home since the Link is just "/"
                    CurrentPagePath = "/";
                }
            }
            else
            {
                var currentPage = await _pageContextRepository.GetCurrentPageAsync();
                if (currentPage.TryGetValue(out var curPage))
                {
                    CurrentPagePath = _urlResolver.ResolveUrl(curPage.RelativeUrl);
                }
            }

            CurrentPagePath = CurrentPagePath.AsNullOrWhitespaceMaybe().GetValueOrDefault("/");
            ParentClass = ParentClass.AsNullOrWhitespaceMaybe().GetValueOrDefault("no-parent-class");

            output.TagName = "script";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("type", "text/javascript");
            string Javascript = "" +
                $"var elem = document.querySelector(\".{ParentClass} li[data-navhref='{CurrentPagePath.ToLower()}']\");" +
                $"elem = elem || document.querySelector(\".{ParentClass} li[data-navpath='{CurrentPagePath.ToLower()}']\");" +
                "if (elem)" +
                "{" +
                "   elem.classList.add('active');" +
                "   elem.setAttribute('aria-current', 'page');" +
                "}";
            output.Content.SetHtmlContent(Javascript);
        }

    }
}
