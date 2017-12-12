using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Monifier.Web.Models.Expenses
{
    public class ExpenseItemViewModel
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Дата/время")]
        public string DateTime { get; set; }

        [Required]
        [DisplayName("Категория")]
        public string Category { get; set; }

        [Required]
        [DisplayName("Продукт")]
        public string Product { get; set; }

        [Required]
        [DisplayName("Сумма")]
        public string Price { get; set; }

        [DisplayName("Количество")]
        public string Quantity { get; set; }

        [DisplayName("Комментарий")]
        public string Comment { get; set; }

        public List<string> AvailCategories { get; set; }

        public bool NoItems { get; set; }
    }
}