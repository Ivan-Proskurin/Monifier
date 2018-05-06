using System;

namespace Monifier.BusinessLogic.Model.Transactions
{
    public class TransactionViewModel
    {
        public string Type { get; set; }
        public decimal Total { get; set; }
        public DateTime DateTime { get; set; }
        public string Target { get; set; }
    }
}