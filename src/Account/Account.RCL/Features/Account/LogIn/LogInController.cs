using System.Security.Claims;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Account.Features.Account.LogIn
{
    public class LogInController(
        IUserRepository _userRepository,
        IAccountSettingsRepository _accountSettingsRepository,
        IUserService _userService,
        ILogger _logger,
        IRoleService _roleService,
        ISiteRepository _siteRepository,
        IModelStateService _modelStateService,
        IAuthenticationConfigurations _authenticationConfigurations,
        ISignInManagerService _signInManagerService,
        IUserManagerService _userManagerService) : Controller
    {
        public const string _routeUrl = "Account/LogIn";
        public const string _twoFormAuthenticationUrl = "Account/TwoFormAuthentication";

        /// <summary>
        /// Fall back if not using Page Templates
        /// </summary>
        [HttpGet]
        [Route(_routeUrl)]
        public ActionResult LogIn()
        {
            return View("/Features/Account/LogIn/LogInManual.cshtml");
        }

        /// <summary>
        /// Handles authentication when the sign-in form is submitted. Accepts parameters posted from the sign-in form via the LogInViewModel.
        /// </summary>
        [HttpPost]
        [Route(_routeUrl)]
        [ValidateAntiForgeryToken]
        [ExportModelState]
        public async Task<ActionResult> LogIn(LogInViewModel model, [FromQuery] string returnUrl)
        {
            string loginUrl = await _accountSettingsRepository.GetAccountLoginUrlAsync(GetUrl());

            // Validates the received user credentials based on the view model
            if (!ModelState.IsValid)
            {
                // Displays the sign-in form if the user credentials are invalid
                return Redirect(loginUrl);
            }

            // Attempts to authenticate the user against the Kentico database
            model.Result = SignInResult.Failed;
            try
            {
                var actualUserResult = await _userRepository.GetUserAsync(model.UserName);
                if (actualUserResult.IsFailure)
                {
                    actualUserResult = await _userRepository.GetUserByEmailAsync(model.UserName);
                }

                if (actualUserResult.IsFailure)
                {
                    ModelState.AddModelError(nameof(model.UserName), actualUserResult.Error);
                    // Store model state, then redirect
                    _modelStateService.StoreViewModel(TempData, model);
                    return Redirect("/" + _twoFormAuthenticationUrl);
                }
                var actualUser = actualUserResult.Value;

                var passwordValid = await _userManagerService.CheckPasswordByNameAsync(actualUser.UserName, model.Password);

                if (passwordValid && _authenticationConfigurations.UseTwoFormAuthentication())
                {
                    if (await _signInManagerService.IsTwoFactorClientRememberedByNameAsync(actualUser.UserName))
                    {
                        // Sign in and proceed.
                        await _signInManagerService.SignInByNameAsync(actualUser.UserName, model.StayLogedIn);
                        return await LoggedInRedirect(model.RedirectUrl);
                    }
                    else
                    {
                        // Send email
                        var token = await _userManagerService.GenerateTwoFactorTokenByNameAsync(actualUser.UserName, "Email");
                        await _userService.SendVerificationCodeEmailAsync(actualUser, "Email");

                        // Redirect to Two form auth page
                        var twoFormAuthViewModel = new TwoFormAuthenticationViewModel()
                        {
                            UserName = actualUser.UserName,
                            RedirectUrl = returnUrl,
                            StayLoggedIn = model.StayLogedIn
                        };

                        // Clear login state
                        _modelStateService.ClearViewModel<LogInViewModel>(TempData);

                        // Store model state, then redirect
                        _modelStateService.StoreViewModel(TempData, twoFormAuthViewModel);
                        return Redirect("/" + _twoFormAuthenticationUrl);
                    }
                }

                // Normal sign in
                model.Result = await _signInManagerService.PasswordSignInByNameAsync(actualUser.UserName, model.Password, model.StayLogedIn, false);
            }
            catch (Exception ex)
            {
                // Logs an error into the Kentico event log if the authentication fails
                _logger.LogException(ex, nameof(LogInController), "Login", Description: $"For user {model.UserName}");
            }

            // Store results
            _modelStateService.StoreViewModel(TempData, model);

            // If the authentication was not successful, displays the sign-in form with an "Authentication failed" message
            if (model.Result != SignInResult.Success)
            {
                ModelState.AddModelError(string.Empty, "Authentication failed");
                return Redirect(loginUrl);
            }

            return await LoggedInRedirect(model.RedirectUrl);
        }

        [HttpGet]
        [Route(_twoFormAuthenticationUrl)]
        public async Task<IActionResult> TwoFormAuthentication()
        {
            var model = _modelStateService.GetViewModel<TwoFormAuthenticationViewModel>(TempData).TryGetValue(out var modelVal) ? modelVal : new TwoFormAuthenticationViewModel();

            // Handle no user found
            if (model.UserName.AsNullOrWhitespaceMaybe().TryGetValue(out var userName))
            {
                var userExists = await _userManagerService.UserExistsByNameAsync(userName);
                if (!userExists)
                {
                    string loginUrl = await _accountSettingsRepository.GetAccountLoginUrlAsync(GetUrl());
                    return Redirect(loginUrl);
                }
            }
            return View("/Features/Account/Login/TwoFormAuthentication.cshtml", model);
        }

        [HttpPost]
        [Route(_twoFormAuthenticationUrl)]
        public async Task<IActionResult> TwoFormAuthentication(TwoFormAuthenticationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // This always returns failed, can't figure out why.

                var userExists = await _userManagerService.UserExistsByNameAsync(model.UserName);
                if (!userExists)
                {
                    return Redirect($"/{_routeUrl}");
                }

                // Verify token is correct
                var tokenValid = await _userManagerService.VerifyTwoFactorTokenByNameAsync(model.UserName, "Email", model.TwoFormCode);
                if (tokenValid)
                {
                    await _signInManagerService.SignInByNameAsync(model.UserName, model.StayLoggedIn);
                    // Redirectig away from Login, clear TempData so if they return to login it doesn't persist
                    _modelStateService.ClearViewModel<TwoFormAuthenticationViewModel>(TempData);
                    ModelState.Clear();
                    return await LoggedInRedirect(model.RedirectUrl);
                }

                // Invalid token
                model.Failure = true;
                return View("/Features/Account/Login/TwoFormAuthentication.cshtml", model);
            }

            return View("/Features/Account/Login/TwoFormAuthentication.cshtml", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = "")
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManagerService.ConfigureExternalAuthenticationProperties(provider, redirectUrl ?? "/");
            properties.SetParameter("AuthType", "reauthorize");
            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("Account/ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(LogInViewModel model)
        {
            string loginUrl = await _accountSettingsRepository.GetAccountLoginUrlAsync(GetUrl());

            var infoResult = await _signInManagerService.GetExternalLoginInfoAsync();

            if (!infoResult.TryGetValue(out var info))
            {
                return Redirect(loginUrl);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                return Redirect(loginUrl);
            }

            var applicationUserExists = await _userManagerService.UserExistsByEmailAsync(email);
            if (applicationUserExists)
            {
                _ = await _userManagerService.EnableUserByEmailAsync(email, _authenticationConfigurations.GetExistingInternalUserBehavior() != ExistingInternalUserBehavior.LeaveAsIs);
            }
            else
            {
                // Create new user
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

                await _userService.CreateExternalUserAsync(new User(
                    email: email,
                    userName: email,
                    enabled: true,
                    firstName: firstName ?? string.Empty,
                    lastName: lastName ?? string.Empty,
                    isExternal: true,
                    isPublic: false
                    ));
            }

            // Sign in
            await _signInManagerService.SignInByEmailAsync(email, model.StayLogedIn);

            var externalUserRoles = _authenticationConfigurations.AllExternalUserRoles().ToList();

            switch (info.LoginProvider.ToLowerInvariant())
            {
                case "microsoft":
                    externalUserRoles.AddRange(_authenticationConfigurations.MicrosoftUserRoles());
                    break;
                case "twitter":
                    externalUserRoles.AddRange(_authenticationConfigurations.TwitterUserRoles());
                    break;
                case "facebook":
                    externalUserRoles.AddRange(_authenticationConfigurations.FacebookUserRoles());
                    break;
                case "google":
                    externalUserRoles.AddRange(_authenticationConfigurations.GoogleUserRoles());
                    break;
                default:
                    break;
            }

            int userId = await _userManagerService.GetUserIDByEmailAsync(email);
            foreach (string role in externalUserRoles)
            {
                await _roleService.CreateRoleIfNotExisting(role, _siteRepository.CurrentChannelName().GetValueOrDefault(string.Empty));
                await _roleService.SetUserRole(userId, role, _siteRepository.CurrentChannelName().GetValueOrDefault(string.Empty), true);
            }
            model.Result = SignInResult.Success;

            return await LoggedInRedirect(model.RedirectUrl);
        }

        /// <summary>
        /// Logic to redirect upon authentication
        /// </summary>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        private async Task<RedirectResult> LoggedInRedirect(string redirectUrl)
        {
            if (await _accountSettingsRepository.GetAccountRedirectToAccountAfterLoginAsync())
            {
                // Redirectig away from Login, clear TempData so if they return to login it doesn't persist
                _modelStateService.ClearViewModel<LogInViewModel>(TempData);
                ModelState.Clear();

                string actualRedirectUrl = "";
                // Try to get returnUrl from query
                if (!string.IsNullOrWhiteSpace(redirectUrl))
                {
                    actualRedirectUrl = redirectUrl;
                }
                if (string.IsNullOrWhiteSpace(redirectUrl))
                {
                    actualRedirectUrl = await _accountSettingsRepository.GetAccountMyAccountUrlAsync("/Account/MyAccount");
                }
                if (string.IsNullOrWhiteSpace(redirectUrl))
                {
                    actualRedirectUrl = "/";
                }
                return Redirect(actualRedirectUrl);
            }
            else
            {
                return Redirect("/");
            }
        }

        public static string GetUrl()
        {
            return "/" + _routeUrl;
        }
    }
}
