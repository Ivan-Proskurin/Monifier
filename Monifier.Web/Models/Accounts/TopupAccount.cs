using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Accounts
{
    public class TopupAccount : IValidatable
    {
        public int Id { get; set; }
        
        public bool Correcting { get; set; }
        
        [Display(Name = "Счет пополнения")]
        [Required(ErrorMessage = "Выберите счет пополнения")]
        public string AccountName { get; set; }
        
        [Display(Name = "Дата пополнения")]
        [Required(ErrorMessage = "Введите время пополнения")]
        public string TopupDate { get; set; }
        
        [Display(Name = "Статья дохода")]
        [Required(ErrorMessage = "Должна быть указана статья дохода")]
        public string IncomeType { get; set; }
        
        [Display(Name = "Сумма")]
        [Required(ErrorMessage = "Введите сумму")]
        public string Amount { get; set; }
        
        public bool AddNonexistentIncomeType { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            var amountResult = Amount.ValidateMoney(nameof(Amount));
            if (amountResult != null) yield return amountResult;
            var dateResult = TopupDate.ValidateDateTime(nameof(TopupDate));
            if (dateResult != null) yield return dateResult;
        }
    }
}