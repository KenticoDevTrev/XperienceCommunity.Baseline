using Account.Features.Account.Confirmation;
using Account.Features.Account.LogIn;
using FluentValidation;

namespace Account.Features.Account.Registration
{
    public class RegistrationController(
        IAccountSettingsRepository _accountSettingsRepository,
        IUserService _userService,
        ILogger _logger,
        IUrlResolver _urlResolver,
        IModelStateService _modelStateService,
        IValidator<RegistrationViewModel> validator,
        IUserManagerService userManagerService,
        IUserRepository userRepository) : Controller
    {
        public const string _routeUrl = "Account/Registration";
        private readonly IValidator<RegistrationViewModel> _validator = validator;
        private readonly IUserManagerService _userManagerService = userManagerService;
        private readonly IUserRepository _userRepository = userRepository;
        public const string _confirmationHashSalt = "f3a8-09GHFB:O#$fp939o4gq4q2h;fa";
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
            if (!ModelState.IsValid) {
                userAccountModel.Password = string.Empty;
                userAccountModel.PasswordConfirm = string.Empty;
                _modelStateService.StoreViewModel(TempData, userAccountModel);
                return Redirect(registrationUrl);
            }

            // Create a basic Kentico User and assign the portal ID
            try {
                var newUserResult = await _userService.CreateUser(userAccountModel.User.GetUser(), userAccountModel.Password, enabled: false);
                if (!newUserResult.TryGetValue(out var newUser, out var error)) {
                    throw new Exception(error);
                }
                // Send confirmation email with registration link
                string confirmationUrl = await _accountSettingsRepository.GetAccountConfirmationUrlAsync(ConfirmationController.GetUrl());
                await _userService.SendRegistrationConfirmationEmailAsync(newUser, _urlResolver.GetAbsoluteUrl(confirmationUrl));
                userAccountModel.RegistrationSuccessful = true;
            } catch (Exception ex) {
                _logger.LogException(ex, nameof(RegistrationController), "Registration", Description: $"For User {userAccountModel.User}");
                userAccountModel.RegistrationFailureMessage = ex.Message;
                userAccountModel.RegistrationSuccessful = false;
            }

            // Store view model for retrieval, without the passwords
            userAccountModel.Password = string.Empty;
            userAccountModel.PasswordConfirm = string.Empty;
            _modelStateService.StoreViewModel(TempData, userAccountModel);

            return Redirect(registrationUrl);

        }

        /// <summary>
        /// Registers the User, uses Email confirmation
        /// </summary>
        /// <param name="UserAccountModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Account/ResendRegistration")]
        public async Task<ActionResult> ResendRegistration(ResendConfirmationViewModel model)
        {
            // var registrationUrl = await _accountSettingsRepository.GetAccountRegistrationUrlAsync(GetUrl());
            // verify token
            var userResult = await _userRepository.GetUserAsync(model.UserName);
            if(userResult.IsFailure) {
                userResult = await _userRepository.GetUserByEmailAsync(model.UserName);
            }

            if (!userResult.TryGetValue(out var user)) {
                return Redirect(await _accountSettingsRepository.GetAccountLoginUrlAsync(LogInController.GetUrl()));
            }

            // Verify hash
            var securityStamp = await _userManagerService.GetSecurityStampAsync(user.UserName);
            var hash = $"{user.UserName}{securityStamp}{_confirmationHashSalt}".ToLower().GetHashCode();
            if (!hash.ToString().Equals(model.VerificationCheck)) {
                return Redirect(await _accountSettingsRepository.GetAccountLoginUrlAsync(LogInController.GetUrl()));
            }

            await _userService.SendRegistrationConfirmationEmailAsync(user, _urlResolver.GetAbsoluteUrl(await _accountSettingsRepository.GetAccountConfirmationUrlAsync(ConfirmationController.GetUrl())));
            
            _modelStateService.ClearViewModel<LogInViewModel>(TempData);
            return Redirect(await _accountSettingsRepository.GetAccountLoginUrlAsync(LogInController.GetUrl()));
        }

        public static string GetUrl()
        {
            return "/" + _routeUrl;
        }
    }
}
