using Account.Features.Account.ForgotPassword;
using Account.Features.Account.LogIn;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Account.Features.Account.ForgottenPasswordReset
{
    public class ForgottenPasswordResetController(
        IUserRepository _userRepository,
        IAccountSettingsRepository _accountSettingsRepository,
        IUserService _userService,
        ILogger _logger,
        IValidator<ForgottenPasswordResetViewModel> validator,
        IModelStateService modelStateService) : Controller
    {
        public const string _routeUrl = "Account/ForgottenPasswordReset";
        private readonly IValidator<ForgottenPasswordResetViewModel> _validator = validator;
        private readonly IModelStateService _modelStateService = modelStateService;

        /// <summary>
        /// Retrieves the UserGUID and the Token and presents the password reset.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(_routeUrl)]
        public ActionResult ForgottenPasswordReset()
        {
            return View("/Features/Account/ForgottenPasswordReset/ForgottenPasswordResetManual.cshtml");
        }

        /// <summary>
        /// Validates and resets the password
        /// </summary>
        [HttpPost]
        [Route(_routeUrl)]
        [ExportModelState]
        public async Task<ActionResult> ForgottenPasswordReset(ForgottenPasswordResetViewModel model)
        {
            var forgottenPasswordResetUrl = await _accountSettingsRepository.GetAccountForgottenPasswordResetUrlAsync(GetUrl());
            var validationResults = await _validator.ValidateAsync(model);
            ModelState.ApplyFluentValidationResults(validationResults);
            if (!ModelState.IsValid)
            {
                return Redirect(forgottenPasswordResetUrl);
            }
            try
            {
                model.ResultIdentity = IdentityResult.Failed();
                var userResult = await _userRepository.GetUserAsync(model.UserID);
                if (userResult.IsFailure)
                {
                    model.ResultIdentity = IdentityResult.Failed(new IdentityError() { Code = "NoUser", Description = userResult.Error });
                }
                else
                {
                    model.ResultIdentity = await _userService.ResetPasswordFromTokenAsync(userResult.Value, model.Token, model.Password);
                    model.LoginUrl = await _accountSettingsRepository.GetAccountLoginUrlAsync(LogInController.GetUrl());
                }
            }
            catch (Exception ex)
            {
                model.ResultIdentity = IdentityResult.Failed(new IdentityError() { Code = "Unknown", Description = "An error occurred." });
                _logger.LogException(ex, nameof(ForgottenPasswordResetController), "ForgottenPasswordReset", Description: $"For userid {model.UserID}");
            }

            // Set this property as the View uses it instead of the IdentityResult, which doesn't serialize/deserialize properly and doesn't make it through the StoreViewModel/GetViewModel
            model.Succeeded = model.ResultIdentity.Succeeded;

            // Don't store passwords in TempPassword
            model.Password = string.Empty;
            model.PasswordConfirm = string.Empty;

            _modelStateService.StoreViewModel(TempData, model);
            return Redirect(forgottenPasswordResetUrl);
        }

        public static string GetUrl()
        {
            return "/"+ _routeUrl;
        }
    }
}
