using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Components.AsyncScriptFunctions
{
    /// <summary>
    /// Adds the window.LoadScript and window.OnScriptsLoaded methods to the page.  
    /// 
    /// use window.LoadScript(options) to load your javascript files
    /// options: {src: string, header?: boolean, crossorigin?: string, appendAtEnd?: bool}
    ///    -src: The URL of the javascript file.  Use Html.AddFileVersionToPath(string) to add a File Version to the path
    ///    -header?: if the Javascript should be placed in the header or footer
    ///    -crossorigin?: cross origin value if any
    ///    -appendAtEnd?: Scripts are loaded non-appended-at-end (in order added), then append-at-end (in order added), so use this if you want your script to load AFTER the normal scripts. 
    ///
    /// use window.OnScriptsLoaded(function, ?identifier) to run scripts once all the javascript is loaded
    /// options:
    ///    -function: The function to run (can use () => { console.log('ran'); } type of notation)
    ///    -identifier: optional identifier, if identifiers match it will only run the logic once, useful for things such as widgets with initialization js logic
    /// </summary>
    public class AsyncScriptFunctionsViewComponent : ViewComponent
    {
        public AsyncScriptFunctionsViewComponent() { }

        public IViewComponentResult Invoke()
        {
            return View("/Components/AsyncScriptFunctions/AsyncScriptFunctions.cshtml");
        }
    }
}
