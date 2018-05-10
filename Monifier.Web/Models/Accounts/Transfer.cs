using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.Common.Validation;

namespace Monifier.Web.Models.Accounts
{
    public class Transfer : IValidatable
    {
        [Required(ErrorMessage = "Укажите счет")]
        [Display(Name = "Со счета")]
        public string AccountFrom { get; set; }
        
        [Required(ErrorMessage = "Укажите счет")]
        [Display(Name = "На счет")]
        public string AccountTo { get; set; }
        
        [Required(ErrorMessage = "Введите сумму")]
        [Display(Name = "Сумма")]
        public string Amount { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            var amountResult = Amount.ValidateMoney(nameof(Amount));
            if (amountResult != null) yield return amountResult;
            if (!string.IsNullOrEmpty(AccountFrom) && AccountFrom == AccountTo)
            {
                yield return new ModelValidationResult(nameof(AccountFrom), "Счета не должны совпадать");
                yield return new ModelValidationResult(nameof(AccountTo), "Счета не должны совпадать");
            }
        }
    }
}