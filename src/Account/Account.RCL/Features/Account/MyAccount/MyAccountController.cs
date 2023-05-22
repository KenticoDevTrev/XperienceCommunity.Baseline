using Microsoft.AspNetCore.Authorization;

namespace Account.Features.Account.MyAccount
{
    public class MyAccountController : Controller
    {
        // If Adjusted, also adjust LogInController.cs as well

        public MyAccountController()
        {
        }

        /// <summary>
        /// Can enable and create a My Account View.
        /// </summary>
        [HttpGet]
        [Route(MyAccountControllerPath._routeUrl)]
        [Authorize]
        public ActionResult MyAccount()
        {
            return View("/Features/Account/MyAccount/MyAccountManual.cshtml");
        }

    }
}
