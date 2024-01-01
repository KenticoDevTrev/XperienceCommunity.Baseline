namespace Core.Components.AsyncScriptLoader
{
    public class AsyncScriptLoaderViewComponent : ViewComponent
    {
        /// <summary>
        /// Placed at the end of the page, this places the code to loop through all the scripts, and calls the ScriptRunnerPath at the end.
        /// </summary>
        /// <param name="xScriptRunnerPath">Path to the Script Runner JS file.  The file should contain this javascript code:
        /// window.ScriptsLoaded = true;
        /// for (var queuedScripts = window.PreloadQueue || [], i = 0; i<queuedScripts.length; i++)
        /// queuedScripts[i] ();
        /// </param>
        /// <returns></returns>
        public IViewComponentResult Invoke(string xScriptRunnerPath)
        {
            var model = new AsyncScriptLoaderViewModel(xScriptRunnerPath);
            return View("/Components/AsyncScriptLoader/AsyncScriptLoader.cshtml", model);
        }
    }
    public record AsyncScriptLoaderViewModel
    {
        public string ScriptRunnerPath { get; init; }

        public AsyncScriptLoaderViewModel(string scriptRunnerPath)
        {
            ScriptRunnerPath = scriptRunnerPath;
        }
    }
}
