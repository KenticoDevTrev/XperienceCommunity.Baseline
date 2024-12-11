using Core.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Core.Services.Implementations
{
    public class ModelStateService : IModelStateService
    {
        public void MergeModelState(ModelStateDictionary modelState, ITempDataDictionary tempData)
        {
            string key = typeof(ModelStateTransfer).FullName ?? typeof(ModelStateTransfer).Name;
            if (tempData[key] is string serialisedModelState)
            {
                var retrievedModelState = ModelStateHelpers.DeserialiseModelState(serialisedModelState);
                // Merge
                if (modelState.Keys.Any())
                {
                    retrievedModelState.Merge(modelState);
                }
                else
                {
                    // Populate
                    foreach (string modelStateKey in retrievedModelState.Keys)
                    {
                        var stateItem = retrievedModelState[modelStateKey];
                        if (stateItem != null)
                        {
                            // handle boolean conversion
                            if (stateItem.RawValue?.ToString() == "false" || stateItem.AttemptedValue?.ToString() == "false,true") {
                                modelState.SetModelValue(modelStateKey, false, stateItem.AttemptedValue);

                            } else if(stateItem.RawValue?.ToString() == "true" || stateItem.AttemptedValue?.ToString() == "true,false") {
                                modelState.SetModelValue(modelStateKey, true, stateItem.AttemptedValue);
                            } else { 
                                modelState.SetModelValue(modelStateKey, stateItem.RawValue, stateItem.AttemptedValue);
                            }
                            foreach (var error in stateItem.Errors)
                            {
                                modelState.AddModelError(modelStateKey, error.ErrorMessage);
                            }
                        }
                    }
                }
            }
        }

        public void StoreViewModel<TModel>(ITempDataDictionary tempData, TModel viewModel)
        {
            tempData.Put<TModel>($"GetViewModel_{typeof(TModel).FullName}", viewModel);
        }

        public Result<TModel> GetViewModel<TModel>(ITempDataDictionary tempData)
        {
            var obj = tempData.Get<TModel>($"GetViewModel_{typeof(TModel).FullName}");
            return obj != null ? (TModel)obj : Result.Failure<TModel>("No model found");
        }

        public void ClearViewModel<TModel>(ITempDataDictionary tempData)
        {
            tempData.Remove($"GetViewModel_{typeof(TModel).FullName}");
        }

        public void ClearViewModelAfterRequest<T>(ITempDataDictionary tempData, IHttpContextAccessor httpContextAccessor)
        {
            if(httpContextAccessor.HttpContext != null) {
                httpContextAccessor.HttpContext.Items.Add("ClearViewModelAfterRequestTempData", tempData);
                httpContextAccessor.HttpContext.Items.Add("ClearViewModelAfterRequestType", typeof(T));
            }
        }
    }

    public static class TempDataExtensions
    {
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value)
        {
            tempData[key] = JsonSerializer.Serialize<T>(value);
        }

        public static T? Get<T>(this ITempDataDictionary tempData, string key)
        {
            if (tempData.TryGetValue(key, out var o))
            {
                return o == null ? default : JsonSerializer.Deserialize<T>((string)o);
            }
            return default;
        }
    }
}
