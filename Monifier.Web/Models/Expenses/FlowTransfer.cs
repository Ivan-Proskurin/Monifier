using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Expenses
{
    public class FlowTransfer : IValidatable
    {
        public int FlowId { get; set; }
        
        [Required(ErrorMessage = "Укажите счет")]
        [Display(Name = "Со счета")]
        public string AccountFrom { get; set; }
        
        [Required(ErrorMessage = "Введите сумму")]
        [Display(Name = "Сумма")]
        public string Amount { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            var amountResult = Amount.ValidateMoney(nameof(Amount));
            if (amountResult != null) yield return amountResult;
        }
    }
}