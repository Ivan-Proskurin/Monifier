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
        private readonly IUnitOfWork _unitOfWork;

        public TransitionBalancesUpdater(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Update(ExpenseBill bill, decimal oldSum, int? oldAccountId)
        {
            var flowQueries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();
            var accountQueries = _unitOfWork.GetQueryRepository<Account>();
            var accountCommands = _unitOfWork.GetCommandRepository<Account>();

            var flow = await flowQueries.GetById(bill.ExpenseFlowId);
            var newAccount = bill.AccountId != null ? await accountQueries.GetById(bill.AccountId.Value) : null;
            flow.Balance = flow.Balance + oldSum;
            var lack = bill.SumPrice - Math.Max(flow.Balance, 0);
            var withdrawTotal = bill.SumPrice;
            if (lack > 0 && newAccount != null)
            {
                var compensation = Math.Min(Math.Max(newAccount.AvailBalance, 0), lack);
                withdrawTotal -= compensation;
                newAccount.AvailBalance -= compensation;
            }
            flow.Balance -= withdrawTotal;
            flow.Version++;
            flowCommands.Update(flow);

            if (!bill.IsCorrection)
            {
                if (oldAccountId != bill.AccountId)
                {
                    if (oldAccountId != null)
                    {
                        var oldAccount = await accountQueries.GetById(oldAccountId.Value);
                        oldAccount.Balance += oldSum;
                        accountCommands.Update(oldAccount);
                    }

                    if (newAccount != null)
                    {
                        newAccount.Balance -= bill.SumPrice;
                        newAccount.LastWithdraw = DateTime.Now;
                        accountCommands.Update(newAccount);
                    }
                }
                else if (newAccount != null)
                {
                    newAccount.Balance += oldSum - bill.SumPrice;
                    accountCommands.Update(newAccount);
                }
            }
            else if (newAccount != null)
            {
                accountCommands.Update(newAccount);
            }


        }
    }
}