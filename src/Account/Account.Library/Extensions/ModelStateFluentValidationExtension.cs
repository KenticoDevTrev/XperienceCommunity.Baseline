
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Account.Extensions
{
    public static class ModelStateFluentValidationExtension
    {
        public static void ApplyFluentValidationResults(this ModelStateDictionary ModelState, ValidationResult validationResult)
        {
            var errorsByKey = validationResult.Errors.GroupBy(x => x.PropertyName).ToDictionary(key => key.Key, value => value.Select(x => x));
            foreach (var propertyName in errorsByKey.Keys) {
                    ModelState.AddModelError(propertyName, string.Join(" ", errorsByKey[propertyName].Select(x => x.ErrorMessage)));
                
            }
        }
    }
}
