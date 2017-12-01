using System;

namespace Monifier.BusinessLogic.Model.Incomes
{
    public class IncomeItemModel
    {
        public int Id { get; set; }
        public int IncomeTypeId { get; set; }
        public string IncomeType { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Total { get; set; }
    }
}