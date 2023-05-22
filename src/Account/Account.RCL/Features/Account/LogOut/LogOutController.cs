namespace Account.Features.Account.LogOut
{
    public class LogOutController : Controller
    {
        public const string _routeUrl = "Account/LogOut";
        private readonly ISignInManagerService _signInManagerService;

        public LogOutController(ISignInManagerService signInManagerService)
        {
            _signInManagerService = signInManagerService;
        }

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
        public async Task<ActionResult> LogOut(LogOutViewModel model)
        {
            // Signs out the current user
            await _signInManagerService.SignOutAsync();

            // Redirects to site root
            return Redirect("/");
        }

        public static string GetUrl()
        {
            return "/" + _routeUrl;
        }
    }
}
