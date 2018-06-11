using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Contract.Distribution;
using Monifier.BusinessLogic.Distribution;
using Monifier.BusinessLogic.Distribution.Model;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Distribution;

namespace Monifier.BusinessLogic.Queries.Distribution
{
    public class DistributionCommands : IDistributionCommands
    {
        private readonly IEntityRepository _repository;
        private readonly IFlowDistributor _distributor;
        private readonly ITimeService _timeService;
        private readonly ICurrentSession _currentSession;

        public DistributionCommands(
            IEntityRepository repository,
            IFlowDistributor distributor,
            ITimeService timeService,
            ICurrentSession currentSession)
        {
            _repository = repository;
            _distributor = distributor;
            _timeService = timeService;
            _currentSession = currentSession;
        }
        
        public Task Distribute(DistributionBoard board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            
            return board.Distribute(_repository, _distributor);
        }

        public Task Save(DistributionBoard board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            return board.Save(_repository, _timeService.ClientLocalNow);
        }

        public async Task Distribute(DistributionModel distribution)
        {
            if (!distribution.CanDistribute)
                throw new InvalidOperationException("Can not distribute cause total is above total available fund");

            var sources = new List<IFlowEndPoint>();
            var flowFund = new FlowFundEndPoint(distribution.FlowFund);
            sources.Add(flowFund);

            sources.AddRange(distribution.Accounts
                .Where(x => x.UseInDistribution)
                .OrderByDescending(x => x.Account.IsDefault)
                .ThenByDescending(x => x.Account.AvailBalance));

            foreach (var item in distribution.Items)
            {
                var total = item.Total;
                item.Flow.Balance = item.Amount;
                _repository.Update(item.Flow.ToEntity(_currentSession.UserId));
                if (!Withdraw(sources, total))
                    throw new InvalidOperationException("Can't withdraw from account due to lack of fund");
            }

            if (flowFund.Balance > 0)
            {
                var defaultAccount = distribution.Accounts.Select(x => x.Account).GetDefaultAccount();
                if (defaultAccount != null)
                {
                    var distributionAccount = distribution.Accounts.Single(x => x.Account.Id == defaultAccount.Id);
                    distributionAccount.Topup(flowFund.Balance);
                    _repository.Update(distributionAccount.Account.ToEntity(_currentSession.UserId));
                    flowFund.Withdraw(flowFund.Balance);
                }
            }

            distribution.FundDistributed = flowFund.Distributed;

            distribution.Accounts.ForEach(async x => await CreateOrUpdateAcccountSettings(x));
            distribution.Items.ForEach(async x => await CreateOrUpdateFlowSettings(x));

            await _repository.SaveChangesAsync();
        }

        private bool Withdraw(ICollection<IFlowEndPoint> sources, decimal amount)
        {
            var rest = amount;
            while (rest > 0)
            {
                var source = sources.FirstOrDefault(x => x.Balance > 0);
                if (source == null) return false;
                var withrawTotal = Math.Min(source.Balance, rest);
                source.Withdraw(withrawTotal);
                rest -= withrawTotal;
                if (!(source is DistributionAccount account)) continue;
                _repository.Update(account.Account.ToEntity(_currentSession.UserId));
            }

            return true;
        }

        private async Task CreateOrUpdateAcccountSettings(DistributionAccount distributionAccount)
        {
            var settings = await _repository.GetQuery<AccountFlowSettings>()
                .Where(x => x.AccountId == distributionAccount.Account.Id)
                .SingleOrDefaultAsync();
            if (settings == null)
            {
                settings = new AccountFlowSettings
                {
                    AccountId = distributionAccount.Account.Id,
                    CanFlow = distributionAccount.UseInDistribution
                };
                _repository.Create(settings);
            }
            else
            {
                settings.CanFlow = distributionAccount.UseInDistribution;
                _repository.Update(settings);
            }
        }

        private async Task CreateOrUpdateFlowSettings(DistributionItem item)
        {
            var settings = await _repository.GetQuery<ExpenseFlowSettings>()
                .Where(x => x.ExpenseFlowId == item.Flow.Id)
                .SingleOrDefaultAsync();
            if (settings == null)
            {
                settings = new ExpenseFlowSettings
                {
                    ExpenseFlowId = item.Flow.Id,
                    IsRegularExpenses = item.Mode == DistributionMode.RegularExpenses,
                    Amount = item.Amount,
                    CanFlow = true
                };
                _repository.Create(settings);
            }
            else
            {
                settings.IsRegularExpenses = item.Mode == DistributionMode.RegularExpenses;
                settings.CanFlow = true;
                settings.Amount = item.Amount;
            }
        }
    }

    public class FlowFundEndPoint : IFlowEndPoint
    {
        public FlowFundEndPoint(decimal balance)
        {
            Balance = balance;
        }

        public int Id => -1;

        public string Name => nameof(FlowFundEndPoint);

        public decimal Balance { get; private set; }

        public decimal Distributed { get; private set; }

        public void Topup(decimal amount)
        {
            Balance += amount;
        }

        public void Withdraw(decimal amount)
        {
            Balance -= amount;
            Distributed += amount;
        }

        public DistributionFlowRule FlowRule { get; set; }
    }
}