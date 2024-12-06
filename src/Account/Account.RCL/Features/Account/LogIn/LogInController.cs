using Account.Features.Account.MyAccount;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Design;
using System.Security.Claims;
using System.Web;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Account.Features.Account.LogIn
{
    /// <summary>
    /// NOTICE: This implementation will be force to inject the standard User version of various interfaces
    /// </summary>
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
        IUserManagerService _userManagerService,
        IUrlResolver urlResolver) : Controller
    {
        public const string _routeUrl = "Account/LogIn";
        public const string _twoFormAuthenticationUrl = "Account/TwoFormAuthentication";
        private readonly IUrlResolver _urlResolver = urlResolver;

        /// <summary>
        /// Fall back if not using Page Templates
        /// </summary>
        [HttpGet]
        [Route(_routeUrl)]
        public ActionResult LogIn([FromQuery] string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View("/Features/Account/LogIn/LogInManual.cshtml");
        }

        /// <summary>
        /// Handles authentication when the sign-in form is submitted. Accepts parameters posted from the sign-in form via the LogInViewModel.
        /// </summary>
        [HttpPost]
        [Route(_routeUrl)]
        [ValidateAntiForgeryToken]
        [ExportModelState]
        public async Task<IActionResult> LogIn(LogInViewModel model, [FromQuery] string? returnUrl = null)
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
                        // May be Redirecting away from Login, clear TempData so if they return to login it doesn't persist
                        _modelStateService.ClearViewModel<LogInViewModel>(TempData);
                        ModelState.Clear();
                        return await LoggedInRedirect(model.ReturnUrl);
                    }
                    else
                    {
                        // Send email
                        var token = await _userManagerService.GenerateTwoFactorTokenByNameAsync(actualUser.UserName, "Email");
                        await _userService.SendVerificationCodeEmailAsync(actualUser, token);

                        // Redirect to Two form auth page
                        var twoFormAuthViewModel = new TwoFormAuthenticationViewModel()
                        {
                            UserName = actualUser.UserName,
                            RedirectUrl = model.ReturnUrl.AsNullOrWhitespaceMaybe().GetValueOrDefault(returnUrl ?? string.Empty),
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

            // May be Redirecting away from Login, clear TempData so if they return to login it doesn't persist
            _modelStateService.ClearViewModel<LogInViewModel>(TempData);
            ModelState.Clear();
            return await LoggedInRedirect(model.ReturnUrl);
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
                    _modelStateService.ClearViewModel<LogInViewModel>(TempData);
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
            var redirectUrl = _urlResolver.ResolveUrl(Url.Action(nameof(ExternalLoginCallback), "Account") ?? "/")+(!string.IsNullOrWhiteSpace(returnUrl) ? $"?returnUrl={HttpUtility.UrlEncode(returnUrl)}" : "");
            var properties = _signInManagerService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            properties.SetParameter("AuthType", "reauthorize");
            return Challenge(properties, provider);
        }

        [HttpGet]
        [Route("Account/ExternalLoginCallback")]
        [Route("signin-twitter")] // Required for localhost testing and default routing
        [Route("signin-microsoft")] // Required for localhost testing and default routing
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "")
        {
            var model = new LogInViewModel();

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

                var createResult = await _userService.CreateExternalUser(new User() {
                    Email = email,
                    UserName = email,
                    Enabled = true,
                    FirstName = firstName ?? string.Empty,
                    LastName = lastName ?? string.Empty,
                    IsExternal = true,
                    IsPublic = false
                });
                  

                // Failure to create the user for some reason.
                if(createResult.IsFailure) {
                    return Redirect(loginUrl);
                }
                model.Result = SignInResult.Success;
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

            // May be Redirecting away from Login, clear TempData so if they return to login it doesn't persist
            _modelStateService.ClearViewModel<LogInViewModel>(TempData);
            ModelState.Clear();
            return await LoggedInRedirect(returnUrl);
        }

        /// <summary>
        /// Logic to redirect upon authentication
        /// </summary>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        private async Task<IActionResult> LoggedInRedirect(string? redirectUrl)
        {
            var actualRedirectUrl = "";
            if (!string.IsNullOrEmpty(redirectUrl)) {
                actualRedirectUrl = redirectUrl;
            } else if (await _accountSettingsRepository.GetAccountRedirectToAccountAfterLoginAsync()) {
                actualRedirectUrl = await _accountSettingsRepository.GetAccountMyAccountUrlAsync($"/{MyAccountControllerPath._routeUrl}");
            }

            if (!string.IsNullOrWhiteSpace(actualRedirectUrl)) {
                // Redirecting away from Login, clear TempData so if they return to login it doesn't persist
                _modelStateService.ClearViewModel<LogInViewModel>(TempData);
                ModelState.Clear();
                return Redirect(actualRedirectUrl);
            }

            // Redirecting back to the Login, should set the view model state
            ModelState.Clear();
            _modelStateService.StoreViewModel(TempData, new LogInViewModel() { Result = SignInResult.Success });
            return Redirect(await _accountSettingsRepository.GetAccountLoginUrlAsync($"/{_routeUrl}"));
        }

        public static string GetUrl()
        {
            return "/" + _routeUrl;
        }
    }
}
