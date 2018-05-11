using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class DefaultBalancesUpdater : IBalancesUpdater
    {
        private readonly IUnitOfWork _unitOfWork;

        public DefaultBalancesUpdater(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Create(Account account, ExpenseBill bill)
        {
            var flowQieries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();
            var flow = await flowQieries.GetById(bill.ExpenseFlowId);
            if (flow == null)
                throw new InvalidOperationException($"Can't find expense flow with id {bill.ExpenseFlowId}");
            var lack = bill.SumPrice - Math.Max(flow.Balance, 0);
            var withdrawTotal = bill.SumPrice;
            if (lack > 0 && account != null && !bill.IsCorrection)
            {
                var compensation = Math.Min(Math.Max(account.AvailBalance, 0), lack);
                withdrawTotal -= compensation;
                account.AvailBalance -= compensation;
            }
            flow.Balance -= withdrawTotal;
            flow.Version++;
            flowCommands.Update(flow);

            if (account != null && !bill.IsCorrection)
            {
                var accountCommands = _unitOfWork.GetCommandRepository<Account>();
                account.Balance -= bill.SumPrice;
                account.LastWithdraw = DateTime.Now;
                accountCommands.Update(account);
            }
        }

        public async Task Delete(ExpenseBill bill)
        {
            var flowQueries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();

            var flow = await flowQueries.GetById(bill.ExpenseFlowId);
            flow.Balance += bill.SumPrice;
            flowCommands.Update(flow);

            if (bill.AccountId != null && !bill.IsCorrection)
            {
                var accountQueries = _unitOfWork.GetQueryRepository<Account>();
                var accountCommands = _unitOfWork.GetCommandRepository<Account>();
                var account = await accountQueries.GetById(bill.AccountId.Value);
                account.Balance += bill.SumPrice;
                accountCommands.Update(account);
            }
        }
    }
}