namespace Account.Features.Account.LogOut
{
    public class LogOutController(ISignInManagerService _signInManagerService) : Controller
    {
        public const string _routeUrl = "Account/LogOut";

        [HttpGet]
        [Route(_routeUrl)]
        public ActionResult LogOut()
        {
            return View("/Features/Account/LogOut/LogOutManual.cshtml");
        }

        /// <summary>
        /// Action for signing out users. The Authorize attribute allows the action only for users who are already signed in.
        /// </summary>
        [HttpPost]
        [Route(_routeUrl)]
        #pragma warning disable IDE0060 // Remove unused parameter, keeping for view model customizations
        public async Task<ActionResult> LogOut(LogOutViewModel model)
        {
            // Signs out the current user
            await _signInManagerService.SignOutAsync();

            // Redirects to site root
            return Redirect("/");
        }
        #pragma warning restore IDE0060 // Remove unused parameter, keeping for view model customizations

        public static string GetUrl()
        {
            return "/" + _routeUrl;
        }
    }
}
