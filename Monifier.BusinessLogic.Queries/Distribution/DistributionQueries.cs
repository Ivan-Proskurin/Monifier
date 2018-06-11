using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Distribution;
using Monifier.BusinessLogic.Distribution;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Distribution;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Distribution
{
    public class DistributionQueries : IDistributionQueries
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;

        public DistributionQueries(IEntityRepository repository, ICurrentSession currentSession)
        {
            _repository = repository;
            _currentSession = currentSession;
        }
        
        public async Task<DistributionBoard> GetDistributionBoard()
        {
            var board = new DistributionBoard();
            await board.Load(_repository, _currentSession.UserId);
            return board;
        }

        public async Task<DistributionModel> Load()
        {
            var ownerId = _currentSession.UserId;
            var model = new DistributionModel
            {
                Accounts = await _repository.GetQuery<Account>()
                    .Where(x => !x.IsDeleted && x.OwnerId == ownerId && 
                                x.AccountType != AccountType.CreditCard && x.AvailBalance > 0)
                    .Select(x => new DistributionAccount
                    {
                        Account = x.ToModel(),
                        UseInDistribution = true,
                        StartBalance = x.AvailBalance,
                        Distributed = 0,
                    })
                    .ToListAsync(),

                Items = await _repository.GetQuery<ExpenseFlow>()
                    .Where(x => !x.IsDeleted && x.OwnerId == ownerId)
                    .Select(x => new DistributionItem
                    {
                        Flow = x.ToModel(),
                        Mode = DistributionMode.RegularExpenses,
                        StartBalance = x.Balance,
                        Amount = 0,
                    })
                    .ToListAsync(),

                FundDistributed = 0,
            };

            var accountIds = model.Accounts.Select(x => x.Account.Id).ToList();
            var flowIds = model.Items.Select(x => x.Flow.Id).ToList();
            var accountSettings = (await _repository.GetQuery<AccountFlowSettings>()
                .Where(x => accountIds.Contains(x.AccountId))
                .ToListAsync()).ToDictionary(x => x.AccountId);
            var flowSettings = (await _repository.GetQuery<ExpenseFlowSettings>()
                .Where(x => flowIds.Contains(x.ExpenseFlowId))
                .ToListAsync()).ToDictionary(x => x.ExpenseFlowId);

            foreach (var account in model.Accounts)
            {
                var settings = accountSettings.GetOrDefault(account.Account.Id);
                account.UseInDistribution = settings?.CanFlow ?? true;
            }

            foreach (var item in model.Items)
            {
                var settings = flowSettings.GetOrDefault(item.Flow.Id);
                item.Mode = (settings?.IsRegularExpenses ?? true)
                    ? DistributionMode.RegularExpenses
                    : DistributionMode.Accumulation;
                item.Amount = item.Mode == DistributionMode.RegularExpenses 
                    ? settings?.Amount ?? 0
                    : item.Flow.Balance;
            }

            return model;
        }
    }
}