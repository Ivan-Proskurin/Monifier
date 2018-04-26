using System;

namespace Monifier.BusinessLogic.Model.Incomes
{
    public class IncomeItemModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int IncomeTypeId { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Total { get; set; }
        public bool IsCorrection { get; set; }
    }
}