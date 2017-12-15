using System;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpenseItemModel
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string Category { get; set; }
        public int? ProductId { get; set; }
        public string Product { get; set; }
        public decimal Cost { get; set; }
        public decimal? Quantity { get; set; }
        public string Comment { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }

        public string DisplayName => Product ?? Category;
    }
}
