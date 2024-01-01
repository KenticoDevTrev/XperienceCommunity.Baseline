using Core.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Core.Attributes
{
    /// <summary>
    /// https://andrewlock.net/post-redirect-get-using-tempdata-in-asp-net-core/
    /// </summary>
    public class ImportModelStateAttribute : ModelStateTransfer
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Controller is Controller controller)
            {
                if (controller.TempData[Key] is string serialisedModelState)
                {
                    //Only Import if we are viewing
                    if (filterContext.Result is ViewResult)
                    {
                        var modelState = ModelStateHelpers.DeserialiseModelState(serialisedModelState);
                        filterContext.ModelState.Merge(modelState);
                    }
                    else
                    {
                        //Otherwise remove it.
                        controller.TempData.Remove(Key);
                    }
                }

                base.OnActionExecuted(filterContext);
            }
        }
    }
}
