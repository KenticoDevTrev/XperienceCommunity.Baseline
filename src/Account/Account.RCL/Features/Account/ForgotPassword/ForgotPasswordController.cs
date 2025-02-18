﻿using Account.Features.Account.ForgottenPasswordReset;

namespace Account.Features.Account.ForgotPassword
{
    public class ForgotPasswordController(
    IAccountSettingsRepository accountSettingsRepository,
    IUserService userService,
    IUserRepository userRepository,
    IUrlResolver urlResolver,
    IModelStateService modelStateService) : Controller
    {
        public const string _routeUrl = "Account/ForgotPassword";
        private readonly IAccountSettingsRepository _accountSettingsRepository = accountSettingsRepository;
        private readonly IUserService _userService = userService;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUrlResolver _urlResolver = urlResolver;
        private readonly IModelStateService _modelStateService = modelStateService;
        
        /// <summary>
        /// Fallback if not using Page Templates
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(_routeUrl)]
        public ActionResult ForgotPassword()
        {
            return View("/Features/Account/ForgotPassword/ForgotPasswordManual.cshtml");
        }

        /// <summary>
        /// For security, will always show that it sent an email to that user, even if it didn't find it.  The email will contain the link to reset the password
        /// </summary>
        [HttpPost]
        [Route(_routeUrl)]
        [ExportModelState]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            string forgotPasswordUrl = await _accountSettingsRepository.GetAccountForgotPasswordUrlAsync(GetUrl());
            if (!ModelState.IsValid)
            {
                return Redirect(forgotPasswordUrl);
            }

            try
            {
                var userResult = await _userRepository.GetUserByEmailAsync(model.EmailAddress);
                if (userResult.TryGetValue(out var user))
                {
                    if(user.IsExternal) {
                        model.Succeeded = false;
                        model.Error = "Your user is an External User (authenticated externally), there is no password to reset, please log in with the appropriate Single Sign On Provider.";
                    } else { 
                        string forgottenPasswordResetUrl = await _accountSettingsRepository.GetAccountForgottenPasswordResetUrlAsync(ForgottenPasswordResetController.GetUrl());
                        await _userService.SendPasswordResetEmailAsync(user, _urlResolver.GetAbsoluteUrl(forgottenPasswordResetUrl));
                        model.Succeeded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                model.Succeeded = false;
                model.Error = ex.Message;
            }
            _modelStateService.StoreViewModel(TempData, model);

            return Redirect(forgotPasswordUrl);
        }

        public static string GetUrl()
        {
            return "/" + _routeUrl;
        }
    }
}
