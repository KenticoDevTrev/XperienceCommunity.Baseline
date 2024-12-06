using Account.Features.Account.ForgotPassword;
using Account.Features.Account.MyAccount;
using Account.Features.Account.Registration;
using Microsoft.Extensions.Primitives;

namespace Account.Features.Account.LogIn
{
    [ViewComponent]
    public class LogInViewComponent(
        IHttpContextAccessor _httpContextAccessor,
        IAccountSettingsRepository _accountSettingsRepository,
        IPageContextRepository _pageContextRepository,
        ISignInManagerService _signInManagerService,
        IModelStateService _modelStateService) : ViewComponent
    {
        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Merge Model State
            _modelStateService.MergeModelState(ModelState, TempData);

            string redirectUrl = "";

            // Try to get returnUrl from query
            if (_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext))
            {
                if (httpContext.Request.Query.TryGetValue("returnUrl", out StringValues queryReturnUrl) 
                    && queryReturnUrl.FirstOrMaybe(x => !string.IsNullOrWhiteSpace(x)).TryGetValue(out var queryReturnUrlVal))
                {
                    redirectUrl = queryReturnUrlVal;
                }
            }
            // Check google configuration


            var model = new LogInViewModel()
            {
                ReturnUrl = redirectUrl,
                MyAccountUrl = await _accountSettingsRepository.GetAccountMyAccountUrlAsync(MyAccountControllerPath.GetUrl()),
                RegistrationUrl = await _accountSettingsRepository.GetAccountRegistrationUrlAsync(RegistrationController.GetUrl()),
                ForgotPassword = await _accountSettingsRepository.GetAccountForgotPasswordUrlAsync(ForgotPasswordController.GetUrl()),
                ExternalLoginProviders = (await _signInManagerService.GetExternalAuthenticationSchemesAsync()).ToList(),
            };

            // Set this value fresh
            if(User.Identity.AsMaybe().TryGetValue(out var identity))
            {
                model.AlreadyLogedIn = !(await _pageContextRepository.IsEditModeAsync()) && identity.IsAuthenticated;
            } else
            {
                model.AlreadyLogedIn = false;
            }

            return View("/Features/Account/LogIn/LogIn.cshtml", model);
        }
    }
}
