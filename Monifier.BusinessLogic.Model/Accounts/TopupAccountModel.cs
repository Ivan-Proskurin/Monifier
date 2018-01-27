using System;

namespace Monifier.BusinessLogic.Model.Accounts
{
    public class TopupAccountModel
    {
        public bool Correcting { get; set; }
        public int AccountId { get; set; }
        public DateTime TopupDate { get; set; }
        public int? IncomeTypeId { get; set; }
        public string AddIncomeTypeName { get; set; }
        public decimal Amount { get; set; }
    }
}