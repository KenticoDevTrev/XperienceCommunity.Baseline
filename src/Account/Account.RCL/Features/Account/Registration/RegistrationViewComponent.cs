namespace Account.Features.Account.Registration
{
    [ViewComponent]
    public class RegistrationViewComponent(IModelStateService modelStateService,
        IHttpContextAccessor httpContextAccessor) : ViewComponent
    {
        private readonly IModelStateService _modelStateService = modelStateService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public IViewComponentResult Invoke()
        {
            // Merge Model State
            _modelStateService.MergeModelState(ModelState, TempData);

            // Hydrate Model
            var model = _modelStateService.GetViewModel<RegistrationViewModel>(TempData).GetValueOrDefault(new RegistrationViewModel());

            // Clear after request
            _modelStateService.ClearViewModelAfterRequest<RegistrationViewModel>(TempData, _httpContextAccessor);
            return View("/Features/Account/Registration/Registration.cshtml", model);
        }
    }
}
