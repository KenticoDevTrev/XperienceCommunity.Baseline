﻿using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Core.Middleware
{
    public static class LocationExpanderMiddlewareExtension
    {
        public static IServiceCollection UseFeatureFoldersAndLocationExpansions(this IServiceCollection services)
        {
            return services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new CustomLocationExpander());
            });
        }
    }

    public partial class CustomLocationExpander : IViewLocationExpander
    {
        private const string _customViewPath = "CustomViewPath";
        private const string _customController = "CustomController";
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var DefaultKenticoViewDetector = ViewDetectorRegex();
            var DefaultKenticoPageTypeDetector = PageTypeRegex();
            var DefaultComponentDetector = ComponentRegex();

            /* If successful
             * Group 0 = FullMatch (ex "Widgets/_My_CustomWidget")
             * Group 1 = Widgets/Sections/PageTemplates/PageTypes (ex "Widgets")
             * Group 2 = The Code Name (ex "My_CustomWidget")
             * Group 3 = Namespace (ex "My")
             * Group 4 = Code (ex "CustomWidget")
             * ex 0 =, 1 = , 2 = , 3 = , 4 = 
             * */
            var DefaultKenticoViewsMatch = DefaultKenticoViewDetector.Match(context.ViewName);
            var DefaultKenticoPageTypeMatch = DefaultKenticoPageTypeDetector.Match(context.ViewName);

            /*
             * If successful, 
             * Group 0 = FullMatch (ex "Components/MyComponent/Default")
             * Group 1 = Components (ex "Component")
             * Group 2 = Component Name (ex "MyComponent")
             * Group 3 = View Name (ex "Default")
             * */
            var DefaultComponentMatch = DefaultComponentDetector.Match(context.ViewName);

            if (DefaultKenticoViewsMatch.Success)
            {
                // I'm going to store Widgets, Sections, and Page Templates as the Widgets|Sections|PageTemplates/WidgetCodeName/Default
                context.Values.Add(_customViewPath, string.Format("{0}/{1}/Default", DefaultKenticoViewsMatch.Groups[1].Value, DefaultKenticoViewsMatch.Groups[2].Value.Replace("_", "")));
                context.Values.Add(_customController, DefaultKenticoViewsMatch.Groups[1].Value);
            }
            else if (DefaultKenticoPageTypeMatch.Success)
            {
                context.Values.Add(_customViewPath, string.Format("{0}/{1}/Default", DefaultKenticoPageTypeMatch.Groups[1].Value, DefaultKenticoPageTypeMatch.Groups[2].Value.Replace("_", "")));
                context.Values.Add(_customController, DefaultKenticoPageTypeMatch.Groups[1].Value);
            }
            else if (DefaultComponentMatch.Success)
            {
                // Stripping "Component" out so widget, section, page template View components can go under the main root
                context.Values.Add(_customViewPath, string.Format("{0}/{1}", DefaultComponentMatch.Groups[2].Value, DefaultComponentMatch.Groups[3].Value));
                context.Values.Add(_customController, context.ControllerName);
            }

        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            /* Parameters:
             * {2} - Area Name
             * {1} - Controller Name
             * {0} - View Name
             */
            List<string> Paths = [ 
                // Default View Locations to support imported / legacy paths
                "/Views/{1}/{0}.cshtml",
                "/Views/Shared/{0}.cshtml",

                // Adds Feature Folder Rendering
                "/Features/{1}/{0}.cshtml",
                "/Features/Shared/{0}.cshtml",

                // Custom KM
                // Adds /Components/PageTypes/{ComponentName}/{ComponentViewName}.cshtml
                "/Components/InlineEditors/{0}.cshtml",
                "/Components/Navigation/{0}.cshtml",
                "/Components/Widgets/{0}.cshtml",

                // Some components are in Features if they are a page template
                "/Features/{0}.cshtml",

                // Paths for my Custom Structure, leveraged with the _CustomViewPath and _CustomController values set in PopulateValues
                // Handles Basic Widgets/Sections/PageTemplates
                "/{0}.cshtml", 
                // Adds /Components/{ComponentName}/{ComponentViewName}.cshtml
                "/Components/{0}.cshtml",
                ];

            // Add "Hard Coded" custom view paths to checks, along with the normal default view paths for backward compatability
            if (context.Values.TryGetValue(_customViewPath, out var customPath))
            {
                var CombinedPaths = new List<string>(Paths.Select(x => string.Format(x, customPath, context.Values[_customController], "")));
                CombinedPaths.AddRange(Paths);
                return CombinedPaths;
            }

            // Returns the normal view paths
            return Paths;
        }

        [GeneratedRegex(@"^((?:[Ww]idgets|[Ss]ections|[Pp]age[Tt]emplates))+\/_+((.+)+_+(.+))")]
        private static partial Regex ViewDetectorRegex();
        
        [GeneratedRegex(@"^((?:[Pp]age[Tt]ypes))+\/+((.+)+_+(.+))")]
        private static partial Regex PageTypeRegex();

        [GeneratedRegex(@"^((?:[Cc]omponents))+\/+([\w\.]+)\/+(.*)")]
        private static partial Regex ComponentRegex();
    }
}