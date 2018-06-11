using System.Collections.Generic;
using System.Linq;

namespace Monifier.BusinessLogic.Distribution
{
    public class DistributionModel
    {
        public List<DistributionAccount> Accounts { get; set; }
        public List<DistributionItem> Items { get; set; }

        public decimal FlowFund =>
            Items.Where(x => x.Mode == DistributionMode.RegularExpenses && x.Flow.Balance > 0).Sum(x => x.Flow.Balance) +
            Items.Where(x => x.Mode == DistributionMode.Accumulation && x.Amount < x.Flow.Balance)
                .Sum(x => x.Flow.Balance - x.Amount);

        public decimal TotalFund =>
            Accounts.Where(x => x.UseInDistribution && x.Account.AvailBalance > 0)
                .Sum(x => x.Account.AvailBalance) + FlowFund;

        public decimal FundDistributed { get; set; }

        public decimal Total => Items.Sum(x => x.Total);

        public bool CanDistribute => Total <= TotalFund;
    }
}