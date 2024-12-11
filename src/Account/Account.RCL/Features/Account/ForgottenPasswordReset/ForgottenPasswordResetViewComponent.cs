using Microsoft.Extensions.Primitives;

namespace Account.Features.Account.ForgottenPasswordReset
{
    [ViewComponent]
    public class ForgottenPasswordResetViewComponent(
        IHttpContextAccessor _httpContextAccessor,
        IModelStateService _modelStateService) : ViewComponent
    {
        /// <summary>
        /// Uses the current page context to render meta data
        /// </summary>
        /// <returns></returns>
        public IViewComponentResult Invoke()
        {
            
            // Get values from Query String
            Maybe<Guid> userId = Maybe.None;
            Maybe<string> token = Maybe.None;
            if(_httpContextAccessor.HttpContext.AsMaybe().TryGetValue(out var httpContext))
            {
                if (httpContext.Request.Query.TryGetValue("userId", out StringValues queryUserID) 
                    && queryUserID.FirstOrMaybe(x => !string.IsNullOrWhiteSpace(x)).TryGetValue(out var userIdValue))
                {
                    if (Guid.TryParse(userIdValue, out Guid userIdTemp))
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

            // Merge Model State
            _modelStateService.MergeModelState(ModelState, TempData);

            // Hydrate model State
            var model = _modelStateService.GetViewModel<ForgottenPasswordResetViewModel>(TempData).GetValueOrDefault(new ForgottenPasswordResetViewModel());
            model.UserID = userId.GetValueOrDefault(Guid.Empty);
            model.Token = token.GetValueOrDefault(string.Empty);

            // Clear afterwards
            _modelStateService.ClearViewModelAfterRequest<ForgottenPasswordResetViewModel>(TempData, _httpContextAccessor);
            return View("/Features/Account/ForgottenPasswordReset/ForgottenPasswordReset.cshtml", model);
        }
    }
}
