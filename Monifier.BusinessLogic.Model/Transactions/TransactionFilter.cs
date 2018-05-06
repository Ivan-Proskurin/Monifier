namespace Monifier.BusinessLogic.Model.Transactions
{
    public class TransactionFilter
    {
        public TransactionFilter()
        {
        }

        public TransactionFilter(int accountId)
        {
            AccountId = accountId;
        }

        public int AccountId { get; set; }
        public string Operation { get; set; }
        public int PageNumber { get; set; }
    }
}