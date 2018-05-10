using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.Common.Validation;

namespace Monifier.Web.Models.Expenses
{
    public class EditExpense : IValidatable
    {
        public bool Correction { get; set; }

        public int FlowId { get; set; }
        
        [Display(Name = "Статья расходов *")]
        [Required(ErrorMessage = "Выберите статью расходов")]
        public string FlowName { get; set; }
        
        [Display(Name = "Счет *")]
        [Required(ErrorMessage = "Выберите счет")]
        public string Account { get; set; }
             
        [Display(Name = "Дата/время *")]
        [Required(ErrorMessage = "Укажите дату/время")]
        public string DateTime { get; set; }
        
        [Display(Name = "Категория")]
        public string Category { get; set; }
        
        public string CategoryToAdd { get; set; }
        
        [Display(Name = "Товар")]
        public string Product { get; set; }
        
        public string ProductToAdd { get; set; }
        
        [Display(Name = "Сумма *")]
        [Required(ErrorMessage = "Введите сумму")]
        public string Cost { get; set; }

        public string ReturnPage { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            var dateTimeResult = DateTime.ValidateDateTime(nameof(DateTime));
            if (dateTimeResult != null) yield return dateTimeResult;
            var costResult = Cost.ValidateMoney(nameof(Cost));
            if (costResult != null) yield return costResult;
            if (Category == null && Product == null)
            {
                const string message = "Укажите категорию или товар";
                yield return new ModelValidationResult(nameof(Category), message);
                yield return new ModelValidationResult(nameof(Product), message);
            }
        }

        public ExpenseFlowExpense ToModel()
        {
            return new ExpenseFlowExpense
            {
                Correcting = Correction,
                Account = Account,
                ExpenseFlowId = FlowId,
                Category = Category,
                Product = Product,
                DateCreated = DateTime.ParseDtFromStandardString(),
                Cost = Cost.ParseMoneyInvariant()
            };
        }
    }
}