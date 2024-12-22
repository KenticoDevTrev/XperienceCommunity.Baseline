using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Web;

namespace Core.TagHelpers
{
    [HtmlTargetElement("img", Attributes = "bl-optimize")]
    public class OptimizedPictureTagHelper : TagHelper
    {
        private readonly string[] _optimizedImageExtensions = ["jpg", "jpeg", "png"];
        private readonly string[] _webpImageExtensions = ["jpg", "jpeg"];

        [HtmlAttributeName("bl-optimize")]
        public bool UseThisThing { get; set; } = true;


        public override int Order => 50;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if(output.Attributes.TryGetAttribute("src", out TagHelperAttribute attribute) && attribute.Value != null)
            {
                var attrValueMaybe = (attribute.Value is HtmlString attrStrVal ? attrStrVal.Value : attribute.ToString()).AsNullOrWhitespaceMaybe();
                if (attrValueMaybe.TryGetValue(out var attrValue))
                {
                    string imgSrc = "/" + attrValue.Trim('/');

                    if (imgSrc.StartsWith("/images/source", StringComparison.OrdinalIgnoreCase))
                    {
                        string imageExtension = System.IO.Path.GetExtension(imgSrc).Trim('.');
                        bool hasOptimized = _optimizedImageExtensions.Contains(imageExtension, StringComparer.OrdinalIgnoreCase);
                        bool hasWebp = _webpImageExtensions.Contains(imageExtension, StringComparer.OrdinalIgnoreCase);

                        if (hasOptimized || hasWebp)
                        {
                            output.PreElement.AppendHtml("<picture>");

                            var altString = output.Attributes.ContainsName("alt") ? $"alt=\"{HttpUtility.HtmlAttributeEncode(output.Attributes["alt"].Value.ToString() ?? "")}\"" : "";
                            var heightString = output.Attributes.ContainsName("height") ? $"height=\"{HttpUtility.HtmlAttributeEncode(output.Attributes["width"].Value.ToString() ?? "")}\"" : "";
                            var widthString = output.Attributes.ContainsName("width") ? $"width=\"{HttpUtility.HtmlAttributeEncode(output.Attributes["height"].Value.ToString() ?? "")}\"" : "";

                            // use WebP
                            if (hasWebp)
                            {
                                output.PreElement.AppendHtml($"<source srcset=\"{imgSrc.Replace("/source/", "/webp/", StringComparison.OrdinalIgnoreCase).Replace(imageExtension, "webp", StringComparison.OrdinalIgnoreCase)}\" type=\"image/webp\" {altString} {heightString} {widthString} />");
                            }
                            if (hasOptimized)
                            {
                                output.PreElement.AppendHtml($"<source srcset=\"{imgSrc.Replace("/source/", "/optimized/", StringComparison.OrdinalIgnoreCase)}\" type=\"image/{imageExtension.Replace("jpg", "jpeg", StringComparison.OrdinalIgnoreCase)}\" {altString} {heightString} {widthString} />");
                            }
                            // normal image tag will appear here


                            output.PostElement.AppendHtml("</picture>");
                        }
                    }
                }
            }
            

            base.Process(context, output);
        }
    }
}
