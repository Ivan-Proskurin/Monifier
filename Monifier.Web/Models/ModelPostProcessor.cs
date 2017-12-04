using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.Diagnostics;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models
{
    public static class ModelPostProcessor
    {
        public static async Task<IActionResult> ProcessAsync(this IValidatable model, 
            ModelStateDictionary modelState,
            Func<Task<IActionResult>> success, 
            Func<Task<IActionResult>> failure,
            Func<List<ModelValidationResult>, Task> validate = null)
        {
            foreach (var vr in model.Validate())
            {
                modelState.AddModelError(vr.PropertyName, vr.Message);
            }
            
            if (validate != null)
            {
                var vrList = new List<ModelValidationResult>();
                await validate(vrList);
                foreach (var vr in vrList)
                {
                    modelState.AddModelError(vr.PropertyName, vr.Message);
                }
            }     
            
            return modelState.IsValid ? await success() : await failure();
        }
        
        public static async Task<IActionResult> TryProcessAsync<EFailure>(this IValidatable model, 
            ModelStateDictionary modelState,
            Func<Task<IActionResult>> success, 
            Func<Task<IActionResult>> failure,
            Func<List<ModelValidationResult>, Task> validate = null)
        where EFailure : Exception
        {
            foreach (var vr in model.Validate())
            {
                modelState.AddModelError(vr.PropertyName, vr.Message);
            }
            
            if (validate != null)
            {
                var vrList = new List<ModelValidationResult>();
                await validate(vrList);
                foreach (var vr in vrList)
                {
                    modelState.AddModelError(vr.PropertyName, vr.Message);
                }
            }     
            
            return modelState.IsValid ? await TrySuccess<EFailure>(modelState, success, failure) : await failure();
        }

        private static async Task<IActionResult> TrySuccess<EFailure>(ModelStateDictionary modelState,
            Func<Task<IActionResult>> success, Func<Task<IActionResult>> failure)
            where EFailure : Exception
        {
            try
            {
                return await success();
            }
            catch (EFailure exc)
            {
                modelState.AddModelError(string.Empty, exc.Message);
                return await failure();
            }
        }
    }
}