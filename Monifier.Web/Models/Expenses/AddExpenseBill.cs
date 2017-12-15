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

        [DisplayName("Категория")]
        public string Category { get; set; }
        
        public string CategoryToAdd { get; set; }

        [DisplayName("Товар")]
        public string Product { get; set; }
        
        public string ProductToAdd { get; set; }

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
            var priceResult = Price.ValidateMoney("Good.Price");
            if (priceResult != null) yield return priceResult;
            if (Category == null && Product == null)
            {
                yield return new ModelValidationResult("Good.Category", "Укажите категорию или товар");
                yield return new ModelValidationResult("Good.Product", "Укажите категорию или товар");
            }
        }

        public void ClearInput()
        {
            Category = null;
            Product = null;
            Price = null;
            Quantity = null;
            Comment = null;
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
                Product = model.Product,
                Quantity = model.Quantity?.ParseMoneyInvariant()
            };
        }
    }
}