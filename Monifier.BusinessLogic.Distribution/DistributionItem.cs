using Monifier.BusinessLogic.Model.Expenses;

namespace Monifier.BusinessLogic.Distribution
{
    public class DistributionItem
    {
        public ExpenseFlowModel Flow { get; set; }
        public DistributionMode Mode { get; set; }
        public decimal StartBalance { get; set; }
        public decimal Amount { get; set; }
        public decimal Total => Mode == DistributionMode.RegularExpenses 
            ? (Flow.Balance >= 0 ? Amount : Amount - Flow.Balance)
            : (Amount > Flow.Balance ? Amount - Flow.Balance : 0);
    }

    public enum DistributionMode
    {
        RegularExpenses,
        Accumulation
    }
}