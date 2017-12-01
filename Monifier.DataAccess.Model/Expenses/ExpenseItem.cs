using System;
using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Contract.Model;
using Monifier.DataAccess.Model.Base;

namespace Monifier.DataAccess.Model.Expenses
{
    public class ExpenseItem : IHasId
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string Comment { get; set; }
        public int BillId { get; set; }
        [ForeignKey("BillId")]
        public ExpenseBill Bill {get; set;}
    }
}
