using System;
using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Contract.Model;
using Monifier.DataAccess.Model.Base;

namespace Monifier.DataAccess.Model.Incomes
{
    public class IncomeItem : IHasId
    {
        public int Id { get; set; }

        public int IncomeTypeId { get; set; }

        [ForeignKey("IncomeTypeId")]
        public IncomeType IncomeType { get; set; }

        public DateTime DateTime { get; set; }

        public decimal Total { get; set; }

        public int AccountId { get; set; }

        [ForeignKey("AccountId")]
        public Account Account { get; set; }
    }
}