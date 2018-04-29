using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Model.Auth;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Auth
{
    public class UserViewModel : IValidatable
    {
        [Display(Name = "Логин")]
        public string Login { get; set; }

        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Новый пароль")]
        public string Password { get; set; }

        [Display(Name = "Подтверждение")]
        public string PasswordConfirmation { get; set; }

        public string TimeZoneOffset { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            if (Name.IsNullOrEmpty())
                yield return new ModelValidationResult(nameof(Name), "Введите имя");
            if (Password.IsNullOrEmpty() && PasswordConfirmation.IsNullOrEmpty()) yield break;
            if (Password == PasswordConfirmation) yield break;
            const string message = "Пароль и подтверждение должны совпадать";
            yield return new ModelValidationResult(nameof(Password), message);
            yield return new ModelValidationResult(nameof(PasswordConfirmation), message);
        }
    }

    public static class UserExtensions
    {
        public static UserViewModel ToViewModel(this User user)
        {
            return new UserViewModel
            {
                Login = user.Login,
                Name = user.Name,
                Password = string.Empty,
                PasswordConfirmation = string.Empty
            };
        }
    }
}