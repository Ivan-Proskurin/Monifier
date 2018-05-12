using System;

namespace Monifier.BusinessLogic.Model.Transactions
{
    public class TransactionViewModel
    {
        public bool IsExpense { get; set; }
        public string Type { get; set; }
        public string Total { get; set; }
        public DateTime DateTime { get; set; }
        public string Target { get; set; }
        public string Balance { get; set; }
    }
}