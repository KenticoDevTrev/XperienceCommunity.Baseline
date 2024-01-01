using Account.Features.Account.LogIn;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;

namespace Account.Features.Account.Confirmation
{
    [ViewComponent]
    public class ConfirmationViewComponent(
        IUserRepository _userRepository,
        IUserService _userService,
        IAccountSettingsRepository _accountSettingsRepository,
        IHttpContextAccessor _httpContextAccessor,
        IPageContextRepository _pageContextRepository) : ViewComponent
    {

        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get values from Query String
            Maybe<Guid> userId = Maybe.None;
            Maybe<string> token = Maybe.None;
            if (_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext))
            {
                if (httpContext.Request.Query.TryGetValue("userId", out StringValues queryUserID) 
                    && queryUserID.FirstOrMaybe(x => !string.IsNullOrWhiteSpace(x)).TryGetValue(out var queryUserIdVal))
                {
                    if (Guid.TryParse(queryUserIdVal, out Guid userIdTemp))
                    {
                        userId = userIdTemp;
                    }
                }
                if (httpContext.Request.Query.TryGetValue("token", out StringValues queryToken)
                    && queryToken.FirstOrMaybe(x => !string.IsNullOrWhiteSpace(x)).TryGetValue(out var queryTokenVal))
                {
                    token = queryTokenVal;
                }
            }

            bool isEditMode = await _pageContextRepository.IsEditModeAsync();
            ConfirmationViewModel model;

            try
            {
                if (!userId.HasValue)
                {
                    throw new InvalidOperationException("No user Identity Provided");
                }
                // Convert Guid to ID
                var userResult = await _userRepository.GetUserAsync(userId.Value);

                if (userResult.IsFailure)
                {
                    throw new InvalidOperationException(userResult.Error);
                }
                // Verifies the confirmation parameters and enables the user account if successful
                var results = await _userService.ConfirmRegistrationConfirmationTokenAsync(userResult.Value, token.GetValueOrDefault(string.Empty));
                model = new ConfirmationViewModel(
                    result: results,
                    isEditMode: isEditMode
                    )
                {
                    LoginUrl = results.Succeeded ? await _accountSettingsRepository.GetAccountLoginUrlAsync(LogInController.GetUrl()) : Maybe.None
                };
                
            }
            catch (InvalidOperationException ex)
            {
                // An InvalidOperationException occurs if a user with the given ID is not found
                model = new ConfirmationViewModel(
                    result: IdentityResult.Failed(new IdentityError() { Description = ex.Message }),
                    isEditMode: isEditMode
                    );
            }

            return View("/Features/Account/Confirmation/Confirmation.cshtml", model);
        }

    }
}
