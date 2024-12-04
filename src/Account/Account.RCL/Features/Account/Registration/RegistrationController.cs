using Account.Features.Account.Confirmation;
using FluentValidation;

namespace Account.Features.Account.Registration
{
    public class RegistrationController(
        IAccountSettingsRepository _accountSettingsRepository,
        IUserService _userService,
        ILogger _logger,
        IUrlResolver _urlResolver,
        IModelStateService _modelStateService,
        IValidator<RegistrationViewModel> validator) : Controller
    {
        public const string _routeUrl = "Account/Registration";
        private readonly IValidator<RegistrationViewModel> _validator = validator;

        /// <summary>
        /// Fall back, should use Account Page Templates instead
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(_routeUrl)]
        public ActionResult Registration()
        {
            return View("/Features/Account/Registration/RegistrationManual.cshtml");
        }

        /// <summary>
        /// Registers the User, uses Email confirmation
        /// </summary>
        /// <param name="UserAccountModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(_routeUrl)]
        [ExportModelState]
        public async Task<ActionResult> Registration(RegistrationViewModel userAccountModel)
        {
            var registrationUrl = await _accountSettingsRepository.GetAccountRegistrationUrlAsync(GetUrl());
            
            // Ensure valid
            var result = await _validator.ValidateAsync(userAccountModel);
            ModelState.ApplyFluentValidationResults(result);
            if (!ModelState.IsValid)
            {
                _modelStateService.StoreViewModel(TempData, userAccountModel);
                return Redirect(registrationUrl);
            }

            // Create a basic Kentico User and assign the portal ID
            try
            {
                var newUserResult = await _userService.CreateUser(userAccountModel.User.GetUser(), userAccountModel.Password);
                if(!newUserResult.TryGetValue(out var newUser, out var error)) {
                    throw new Exception(error);
                }
                // Send confirmation email with registration link
                string confirmationUrl = await _accountSettingsRepository.GetAccountConfirmationUrlAsync(ConfirmationController.GetUrl());
                await _userService.SendRegistrationConfirmationEmailAsync(newUser, _urlResolver.GetAbsoluteUrl(confirmationUrl));
                userAccountModel.RegistrationSuccessful = true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, nameof(RegistrationController), "Registration", Description: $"For User {userAccountModel.User}");
                userAccountModel.RegistrationFailureMessage = ex.Message;
                userAccountModel.RegistrationSuccessful = false;
            }

            // Store view model for retrieval
            _modelStateService.StoreViewModel(TempData, userAccountModel);

            return Redirect(registrationUrl);

        }

        public static string GetUrl()
        {
            return "/" + _routeUrl;
        }
    }
}
