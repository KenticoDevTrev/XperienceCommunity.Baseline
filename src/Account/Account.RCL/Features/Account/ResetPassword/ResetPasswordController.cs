using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace Account.Features.Account.ResetPassword
{
    public class ResetPasswordController(
        IUserRepository _userRepository,
        IAccountSettingsRepository _accountSettingsRepository,
        IUserService _userService,
        ILogger _logger,
        IModelStateService _modelStateService,
        IValidator<ResetPasswordViewModel> validator,
        IUserManagerService userManagerService) : Controller
    {
        public const string _routeUrl = "Account/ResetPassword";
        private readonly IValidator<ResetPasswordViewModel> _validator = validator;
        private readonly IUserManagerService _userManagerService = userManagerService;

        /// <summary>
        /// Password Reset, must be authenticated to reset password this way.
        /// </summary>        
        [HttpGet]
        [Authorize]
        [Route(_routeUrl)]
        public ActionResult ResetPassword()
        {
            return View("/Features/Account/ResetPassword/ResetPasswordManual.cshtml");
        }

        /// <summary>
        /// Password Reset, must be authenticated to reset password this way.
        /// </summary>
        [HttpPost]
        [Route(_routeUrl)]
        [Authorize]
        [ExportModelState]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            string resetPasswordUrl = await _accountSettingsRepository.GetAccountResetPasswordUrlAsync(GetUrl());

            var validationResults = await _validator.ValidateAsync(model);
            ModelState.ApplyFluentValidationResults(validationResults);
            if (!ModelState.IsValid)
            {
                return Redirect(resetPasswordUrl);
            }
            if (User.Identity.AsMaybe().TryGetValue(out var identity) && identity.Name.AsNullOrWhitespaceMaybe().TryGetValue(out var name))
            {
                try
                {
                    var currentUser = await _userRepository.GetUserAsync(name);
                    if (currentUser.TryGetValue(out var currentUserVal))
                    {
                        // Everything valid, reset password
                        await _userService.ResetPasswordAsync(currentUserVal, model.Password, model.CurrentPassword);
                        model.Succeeded = true;
                    } else
                    {
                        model.Succeeded = false;
                        model.Error = currentUser.Error;
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, nameof(ResetPasswordController), "ResetPassword", Description: $"For user {name}");
                    model.Succeeded = false;
                    model.Error = "An error occurred in changing the password.";
                }
            }
            // Ensure Passwords not stored in Temp Storage
            model.Password = string.Empty;
            model.PasswordConfirm = string.Empty;
            _modelStateService.StoreViewModel(TempData, model);
            return Redirect(resetPasswordUrl);
        }

        public static string GetUrl()
        {
            return "/" + _routeUrl;
        }

    }

}
