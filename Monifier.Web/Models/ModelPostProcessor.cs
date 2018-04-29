using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Monifier.Common.Extensions;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models
{
    public static class ModelPostProcessor
    {
        public static async Task<IActionResult> ProcessAsync(
            this IValidatable model,
            ModelStateDictionary modelState,
            string propertyName,
            Func<Task<IActionResult>> success, 
            Func<Task<IActionResult>> failure,
            Func<List<ModelValidationResult>, Task> validate = null,
            Action<Exception> handleError = null)
        {
            if (modelState == null)
                throw new ArgumentNullException(nameof(modelState));
            if (propertyName.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(propertyName));
        
            foreach (var vr in model.Validate())
            {
                modelState.AddModelError($"{propertyName}.{vr.PropertyName}", vr.Message);
            }
            
            if (validate != null)
            {
                var vrList = new List<ModelValidationResult>();
                await validate(vrList);
                foreach (var vr in vrList)
                {
                    if (vr.PropertyName.IsNullOrEmpty())
                        modelState.AddModelError(string.Empty, vr.Message);
                    else
                        modelState.AddModelError($"{propertyName}.{vr.PropertyName}", vr.Message);
                }
            }     
            
            return modelState.IsValid ? await TrySuccess(modelState, success, failure, handleError) : await failure();
        }
        
        private static async Task<IActionResult> TrySuccess(ModelStateDictionary modelState,
            Func<Task<IActionResult>> success, Func<Task<IActionResult>> failure, Action<Exception> handleError)
        {
            try
            {
                return await success();
            }
            catch (Exception exc)
            {
                if (handleError != null)
                {
                    handleError(exc);
                }
                else
                {
                    modelState.AddModelError(string.Empty, exc.Message);
                }
                return await failure();
            }
        }
    }
}