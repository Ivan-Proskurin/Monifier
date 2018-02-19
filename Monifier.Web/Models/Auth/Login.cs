using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.Common.Extensions;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Auth
{
    public class Login : IValidatable
    {
        [Display(Name = "Логин")]
        public string UserName { get; set; }
        
        [Display(Name = "Пароль")]
        public string Password { get; set; }
        
        public string ReturnUrl { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            if (UserName.IsNullOrEmpty()) 
                yield return new ModelValidationResult(nameof(UserName), "Введите логин");
            if (Password.IsNullOrEmpty())
                yield return new ModelValidationResult(nameof(Password), "Введите пароль");
        }
    }
}