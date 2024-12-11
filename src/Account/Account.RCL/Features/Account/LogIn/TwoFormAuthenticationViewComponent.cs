namespace Account.Features.Account.LogIn
{
    [ViewComponent]
    public class TwoFormAuthenticationViewComponent(IModelStateService modelStateService,
                                                    IUserManagerService userManagerService,
                                                    IAccountSettingsRepository accountSettingsRepository,
                                                    IHttpContextAccessor httpContextAccessor) : ViewComponent
    {
        private readonly IModelStateService _modelStateService = modelStateService;
        private readonly IUserManagerService _userManagerService = userManagerService;
        private readonly IAccountSettingsRepository _accountSettingsRepository = accountSettingsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Merge Model State
            _modelStateService.MergeModelState(ModelState, TempData);

            // Clear Log In
            _modelStateService.ClearViewModel<LogInViewModel>(TempData);

            // Hydrate Model
            var model = _modelStateService.GetViewModel<TwoFormAuthenticationViewModel>(TempData).TryGetValue(out var modelVal) ? modelVal : new TwoFormAuthenticationViewModel();

            // Handle no user found
            if (!model.UserName.AsNullOrWhitespaceMaybe().TryGetValue(out var userName)
                || (
                !(await _userManagerService.UserExistsByNameAsync(userName) || await _userManagerService.UserExistsByEmailAsync(userName)))) {
                    string loginUrl = await _accountSettingsRepository.GetAccountLoginUrlAsync(LogInController.GetUrl());
                    model = model with { LoginUrl = loginUrl };
            }

            // Clear after request
            _modelStateService.ClearViewModelAfterRequest<TwoFormAuthenticationViewModel>(TempData, _httpContextAccessor);
            return View("/Features/Account/Login/TwoFormAuthentication.cshtml", model);
        }
    }
}
