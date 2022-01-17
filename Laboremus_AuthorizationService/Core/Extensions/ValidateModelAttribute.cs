#pragma warning disable CS1591 // Missing XML comment
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Laboremus_AuthorizationService.Core.Extensions
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid) 
                return;
            var errorsList = context.ModelState.Select(it =>
            {
                return (Field: it.Key, Errors: it.Value.Errors.Select(er => er.ErrorMessage).ToList());
            }).ToList();

            var errorDict = new Dictionary<string, List<string>>();
            foreach (var (field, errors) in errorsList)
            {
                errorDict[LowerInvariant(field)] = errors;
            }

            var error = new
            {
                Message = "The request is invalid",
                Errors = errorDict
            };
            context.Result = new BadRequestObjectResult(error);
        }

        private static string LowerInvariant(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;
            return char.ToLowerInvariant(word[0]) + word.Substring(1);
        }
    }
}