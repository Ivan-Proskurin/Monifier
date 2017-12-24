using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Expenses
{
    public class EditExpense : IValidatable
    {
        public int ExpenseFlowId { get; set; }
        
        public string FlowName { get; set; }
             
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
        
        [Display(Name = "Продолжить ввод расходов после ввода этого")]
        public bool ContinueInput { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            var dateTimeResult = DateTime.ValidateDateTime("Expense.DateTime");
            if (dateTimeResult != null) yield return dateTimeResult;
            var costResult = Cost.ValidateMoney("Expense.Cost");
            if (costResult != null) yield return costResult;
            if (Category == null && Product == null)
            {
                const string message = "Укажите хотя бы категорию или товар";
                yield return new ModelValidationResult("Expense.Category", message);
                yield return new ModelValidationResult("Expense.Product", message);
            }
        }

        public ExpenseFlowExpense ToModel()
        {
            return new ExpenseFlowExpense
            {
                ExpenseFlowId = ExpenseFlowId,
                Category = Category,
                Product = Product,
                DateCreated = DateTime.ParseDtFromStandardString(),
                Cost = Cost.ParseMoneyInvariant()
            };
        }
    }
}