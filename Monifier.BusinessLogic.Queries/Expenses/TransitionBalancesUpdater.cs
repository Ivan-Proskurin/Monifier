using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class TransitionBalancesUpdater : ITransitionBalanceUpdater
    {
        private readonly IEntityRepository _repository;

        public TransitionBalancesUpdater(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task<decimal> Update(ExpenseBill bill, decimal oldSum, int? oldAccountId)
        {
            var flow = await _repository.LoadAsync<ExpenseFlow>(bill.ExpenseFlowId);
            var newAccount = bill.AccountId != null ? await _repository.LoadAsync<Account>(bill.AccountId.Value) : null;
            flow.Balance = flow.Balance + oldSum;
            var lack = bill.SumPrice - Math.Max(flow.Balance, 0);
            decimal withdrawTotal = 0m;
            if (lack > 0 && newAccount != null)
            {
                withdrawTotal = bill.SumPrice - lack;
                newAccount.AvailBalance -= lack;
            }
            else
                withdrawTotal = bill.SumPrice;

            flow.Balance -= withdrawTotal;
            flow.Version++;
            _repository.Update(flow);

            if (!bill.IsCorrection)
            {
                if (oldAccountId != bill.AccountId)
                {
                    if (oldAccountId != null)
                    {
                        var oldAccount = await _repository.LoadAsync<Account>(oldAccountId.Value);
                        oldAccount.Balance += oldSum;
                        _repository.Update(oldAccount);
                    }

                    if (newAccount != null)
                    {
                        newAccount.Balance -= bill.SumPrice;
                        newAccount.LastWithdraw = DateTime.Now;
                        _repository.Update(newAccount);
                    }
                }
                else if (newAccount != null)
                {
                    newAccount.Balance += oldSum - bill.SumPrice;
                    _repository.Update(newAccount);
                }
            }
            else if (newAccount != null)
            {
                _repository.Update(newAccount);
            }

            return newAccount?.Balance ?? 0;
        }
    }
}