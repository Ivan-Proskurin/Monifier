using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Expenses
{
    public class AddExpenseBill : IValidatable
    {
        public int ExpenseFlowId { get; set; }
        
        public string Bill { get; set; }

        [Required(ErrorMessage = "Укажите дату/время")]
        [DisplayName("Дата/время*")]
        public string DateTime { get; set; }

//        [Required(ErrorMessage = "Укажите категорию товаров")]
        [DisplayName("Категория")]
        public string Category { get; set; }

//        [Required(ErrorMessage = "Укажите товар")]
        [DisplayName("Товар")]
        public string Product { get; set; }

        [Required(ErrorMessage = "Укажите сумму")]
        [DisplayName("Сумма*")]
        public string Price { get; set; }

        [DisplayName("Количество")]
        public string Quantity { get; set; }

        [DisplayName("Комментарий")]
        public string Comment { get; set; }

        public List<string> AvailCategories { get; set; }

        public bool NoItems { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            var dateResult = DateTime.ValidateDateTime("Good.DateTime");
            if (dateResult != null) yield return dateResult;
            var priceResult = Price.ValidateMoney("Good.Price");
            if (priceResult != null) yield return priceResult;
            if (Category == null && Product == null)
            {
                yield return new ModelValidationResult("Good.Category", "Укажите категорию или товар");
                yield return new ModelValidationResult("Good.Product", "Укажите категорию или товар");
            }
        }
    }

    public static class AddExpenseBillMapper
    {
        public static ExpenseItemModel ToItemModel(this AddExpenseBill model)
        {
            return new ExpenseItemModel
            {
                Category = model.Category,
                Comment = model.Comment,
                Cost = model.Price.ParseMoneyInvariant(),
                DateTime = model.DateTime.ParseDtFromStandardString(),
                Product = model.Product,
                Quantity = model.Quantity?.ParseMoneyInvariant()
            };
        }
    }
}