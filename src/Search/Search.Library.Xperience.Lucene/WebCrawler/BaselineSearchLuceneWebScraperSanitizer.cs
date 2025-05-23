﻿using AngleSharp.Html.Parser;
using CMS.Helpers;
using System.Text.RegularExpressions;

namespace Search.WebCrawler
{
    public class BaselineSearchLuceneWebScraperSanitizer
    {
        public virtual string SanitizeHtmlDocument(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent)) {
                return string.Empty;
            }

            var parser = new HtmlParser();
            var doc = parser.ParseDocument(htmlContent);
            var body = doc.Body;
            if (body is null) {
                return string.Empty;
            }

            foreach (var element in body.QuerySelectorAll("script")) {
                element.Remove();
            }

            foreach (var element in body.QuerySelectorAll("style")) {
                element.Remove();
            }

            // Removes elements marked with the default Xperience exclusion attribute
            foreach (var element in body.QuerySelectorAll($"*[{"data-ktc-search-exclude"}]")) {
                element.Remove();
            }

            foreach (var element in body.QuerySelectorAll("header")) {
                element.Remove();
            }

            foreach (var element in body.QuerySelectorAll("footer")) {
                element.Remove();
            }

            // Gets the text content of the body element
            string textContent = body.TextContent;

            // Normalizes and trims whitespace characters
            textContent = RegexHelper.GetRegex("\\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline).Replace(textContent, " ");
            textContent = textContent.Trim();

            string title = doc.Head?.QuerySelector("title")?.TextContent ?? string.Empty;
            string description = doc.Head?.QuerySelector("meta[name='description']")?.GetAttribute("content") ?? string.Empty;

            return string.Join(
                " ",
                new string[] { textContent }.Where(i => !string.IsNullOrWhiteSpace(i))
            );
        }
    }
}
