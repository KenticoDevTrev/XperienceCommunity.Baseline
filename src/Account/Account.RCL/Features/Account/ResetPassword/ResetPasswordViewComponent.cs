using Account.Features.Account.MyAccount;

namespace Account.Features.Account.ResetPassword
{
    [ViewComponent]
    public class ResetPasswordViewComponent(IModelStateService modelStateService, IUserRepository userRepository, IAccountSettingsRepository accountSettingsRepository, IHttpContextAccessor httpContextAccessor) : ViewComponent
    {
        private readonly IModelStateService _modelStateService = modelStateService;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IAccountSettingsRepository _accountSettingsRepository = accountSettingsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = await _userRepository.GetCurrentUserAsync();

            // Merge Model State
            _modelStateService.MergeModelState(ModelState, TempData);

            // Hydrate Model
            var model = _modelStateService.GetViewModel<ResetPasswordViewModel>(TempData).GetValueOrDefault(new ResetPasswordViewModel());
            model.IsExternal = currentUser.IsExternal;
            model.MyAccountUrl = await _accountSettingsRepository.GetAccountMyAccountUrlAsync(MyAccountControllerPath._routeUrl);

            // Clear after request
            _modelStateService.ClearViewModelAfterRequest<ResetPasswordViewModel>(TempData, _httpContextAccessor);
            return View("/Features/Account/ResetPassword/ResetPassword.cshtml", model);
        }
    }
}
