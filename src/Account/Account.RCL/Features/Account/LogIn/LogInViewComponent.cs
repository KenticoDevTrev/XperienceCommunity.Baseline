using Account.Features.Account.ForgotPassword;
using Account.Features.Account.MyAccount;
using Account.Features.Account.Registration;
using Microsoft.Extensions.Primitives;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Account.Features.Account.LogIn
{
    [ViewComponent]
    public class LogInViewComponent(
        IHttpContextAccessor _httpContextAccessor,
        IAccountSettingsRepository _accountSettingsRepository,
        IPageContextRepository _pageContextRepository,
        ISignInManagerService _signInManagerService,
        IModelStateService _modelStateService,
        IHttpContextAccessor httpContext,
        IUserRepository userRepository) : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContext = httpContext;
        private readonly IUserRepository _userRepository = userRepository;

        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {

            string redirectUrl = "";
            var username = "";
            
            // Merge Model State
            _modelStateService.MergeModelState(ModelState, TempData);

            // Hydrate Model
            var model = _modelStateService.GetViewModel<LogInViewModel>(TempData).GetValueOrDefault(new LogInViewModel());

            // Try to get returnUrl from query
            if (_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext)) {
                if (httpContext.Request.Query.TryGetValue("returnUrl", out StringValues queryReturnUrl)
                    && queryReturnUrl.FirstOrMaybe(x => !string.IsNullOrWhiteSpace(x)).TryGetValue(out var queryReturnUrlVal)) {
                    redirectUrl = queryReturnUrlVal;
                }
                username = model.UserName.AsNullOrWhitespaceMaybe().GetValueOrDefault(httpContext.User.Identity?.Name ?? "");
            }


            // Set values
            model.ReturnUrl = redirectUrl;
            model.MyAccountUrl = await _accountSettingsRepository.GetAccountMyAccountUrlAsync(MyAccountControllerPath.GetUrl());
            model.RegistrationUrl = await _accountSettingsRepository.GetAccountRegistrationUrlAsync(RegistrationController.GetUrl());
            model.ForgotPassword = await _accountSettingsRepository.GetAccountForgotPasswordUrlAsync(ForgotPasswordController.GetUrl());
            model.ExternalLoginProviders = (await _signInManagerService.GetExternalAuthenticationSchemesAsync()).ToList();

            // Remove the state on any boolean, this blows up sadly when passed to the ASPNet
            // ModelState.Remove(nameof(LogInViewModel.StayLogedIn));

            // Set these values fresh
            var user = (await _userRepository.GetUserAsync(username));
            if (user.IsFailure) {
                user = (await _userRepository.GetUserByEmailAsync(username));
            }
            if (user.TryGetValue(out var userVal) && !userVal.IsPublic) {
                // This value is not serializing properly, so check for this scenario
                if (model.ResultOfSignIn != null && !userVal.Enabled && !string.IsNullOrWhiteSpace(model.ResendConfirmationToken)) {
                    model.ResultOfSignIn = SignInResult.NotAllowed;
                    model.AlreadyLogedIn = false;
                } else if (await _pageContextRepository.IsEditModeAsync() || await _pageContextRepository.IsPreviewModeAsync()) {
                    model.AlreadyLogedIn = false;
                } else if (model.ResultOfSignIn != null && !model.ResultOfSignIn.Succeeded) {
                    model.AlreadyLogedIn = false;
                } else if (userVal.Enabled) {
                    model.AlreadyLogedIn = true;
                }
            } else {
                model.AlreadyLogedIn = false;
            }

            // Clear after request
            _modelStateService.ClearViewModelAfterRequest<LogInViewModel>(TempData, _httpContext);

            return View("/Features/Account/LogIn/LogIn.cshtml", model);
        }
    }
}
