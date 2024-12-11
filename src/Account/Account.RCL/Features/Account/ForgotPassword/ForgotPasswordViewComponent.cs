namespace Account.Features.Account.ForgotPassword
{
    [ViewComponent]
    public class ForgotPasswordViewComponent(IModelStateService _modelStateService, IHttpContextAccessor httpContextAccessor) : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public IViewComponentResult Invoke()
        {
            // Hydrate Model State
            _modelStateService.MergeModelState(ModelState, TempData);

            // Get View Model State
            var model = _modelStateService.GetViewModel<ForgotPasswordViewModel>(TempData).GetValueOrDefault(new ForgotPasswordViewModel());

            // Set to clear after this request
            _modelStateService.ClearViewModelAfterRequest<ForgotPasswordViewModel>(TempData, _httpContextAccessor);
            return View("/Features/Account/ForgotPassword/ForgotPassword.cshtml", model);
        }
    }
}
