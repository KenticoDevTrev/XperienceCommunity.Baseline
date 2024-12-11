using Core.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Core.Attributes
{
    /// <summary>
    /// https://andrewlock.net/post-redirect-get-using-tempdata-in-asp-net-core/
    /// </summary>
    public class ExportModelStateAttribute : ModelStateTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //Only export when ModelState is not valid
            // NOTE: we are going to always transfer in our case
            //if (!filterContext.ModelState.IsValid)
            //{
            //Export if we are redirecting
                if (filterContext.Result is RedirectResult
                    || filterContext.Result is RedirectToRouteResult
                    || filterContext.Result is RedirectToActionResult)
                {
                if (filterContext.Controller is Controller controller && filterContext.ModelState != null)
                {
                    // Try to clear out any passwords, not fool-proof but hopefully keys have "Password" in them...
                    var passwordKeys = filterContext.ModelState.Keys.Where(x => x.Contains("password", StringComparison.OrdinalIgnoreCase));
                    foreach (var passwordKey in passwordKeys) {
                        var item = filterContext.ModelState[passwordKey];
                        if (item != null) {
                            item.AttemptedValue = string.Empty;
                            item.RawValue = null;
                        }
                    }

                    var modelState = ModelStateHelpers.SerialiseModelState(filterContext.ModelState);
                    controller.TempData[Key] = modelState;
                }
            }
            //}

            base.OnActionExecuted(filterContext);
        }
    }
}
