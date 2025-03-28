﻿using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MVC.Middleware
{
    public class PageBuilderModelStateClearer(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        // IMessageWriter is injected into InvokeAsync
        public async Task InvokeAsync(HttpContext httpContext, IModelStateService modelStateService)
        {
            await _next(httpContext);
            try {
                // Clear Temp Data View Model, only really used in Accounts with the Post Redirect Get
                if (httpContext.Items.TryGetValue("ClearViewModelAfterRequestTempData", out var tempData) && tempData is ITempDataDictionary tempDataDictionary
                    && httpContext.Items.TryGetValue("ClearViewModelAfterRequestType", out var clearType) && clearType is Type clearTypeType) {
                    var method = modelStateService.GetType().GetMethod(nameof(IModelStateService.ClearViewModel));
                    if (method != null) {
                        var generic = method.MakeGenericMethod(clearTypeType);
                        generic.Invoke(modelStateService, [tempDataDictionary]);
                    }
                }
            } catch(Exception) {
                // Ignore, just here so it will never blow anything up, shouldn't.
            }
        }
    }
}
