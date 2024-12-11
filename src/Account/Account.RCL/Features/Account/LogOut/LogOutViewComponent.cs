namespace Account.Features.Account.LogOut
{
    [ViewComponent]
    public class LogOutViewComponent(IUserRepository userRepository) : ViewComponent
    {
        private readonly IUserRepository _userRepository = userRepository;

        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Nothing in View Model to need IModelStateService to restore
            // Any retrieval here
            var model = new LogOutViewModel(IsSignedIn: !(await _userRepository.GetCurrentUserAsync()).IsPublic);
            return View("/Features/Account/LogOut/LogOut.cshtml", model);
        }
    }
}
