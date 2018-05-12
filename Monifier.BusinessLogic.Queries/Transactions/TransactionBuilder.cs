using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Accounts;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Queries.Transactions
{
    public class TransactionBuilder : ITransactionBuilder
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionQueries _transactionQueries;

        public TransactionBuilder(IUnitOfWork unitOfWork, ITransactionQueries transactionQueries)
        {
            _unitOfWork = unitOfWork;
            _transactionQueries = transactionQueries;
        }

        public void CreateExpense(ExpenseBill bill)
        {
            if (bill.AccountId == null) return;
            var transation = new Transaction
            {
                OwnerId = bill.OwnerId,
                DateTime = bill.DateTime,
                InitiatorId = bill.AccountId.Value,
                Bill = bill,
                Total = bill.SumPrice,
            };
            var transactionCommands = _unitOfWork.GetCommandRepository<Transaction>();
            transactionCommands.Create(transation);
        }

        public async Task UpdateExpense(ExpenseBill bill, int? oldAccountId)
        {
            var transactionCommands = _unitOfWork.GetCommandRepository<Transaction>();
            var transactionQueries = _unitOfWork.GetQueryRepository<Transaction>();
            if (oldAccountId == bill.AccountId)
            {
                var transaction = await transactionQueries.Query
                    .SingleOrDefaultAsync(x => x.OwnerId == bill.OwnerId
                                               && x.InitiatorId == bill.AccountId && x.BillId == bill.Id);
                if (transaction != null)
                {
                    transaction.DateTime = bill.DateTime;
                    transaction.Total = bill.SumPrice;
                    transactionCommands.Update(transaction);
                }
            }
            else if (bill.AccountId != null)
            {
                var transaction = await transactionQueries.Query
                    .SingleOrDefaultAsync(x => x.OwnerId == bill.OwnerId
                                               && x.InitiatorId == oldAccountId && x.BillId == bill.Id);
                if (transaction == null)
                {
                    transaction = new Transaction
                    {
                        OwnerId = bill.OwnerId,
                        DateTime = bill.DateTime,
                        BillId = bill.Id,
                        InitiatorId = bill.AccountId.Value,
                        Total = bill.SumPrice
                    };
                    transactionCommands.Create(transaction);
                }
                else
                {
                    transaction.InitiatorId = bill.AccountId.Value;
                    transaction.Total = bill.SumPrice;
                    transactionCommands.Update(transaction);
                }
            }
        }

        public async Task DeleteExpense(ExpenseBill bill)
        {
            var commands = _unitOfWork.GetCommandRepository<Transaction>();
            var queries = _unitOfWork.GetQueryRepository<Transaction>();
            var transaction = await queries.Query.SingleOrDefaultAsync(
                x => x.InitiatorId == bill.AccountId && x.BillId == bill.Id);
            if (transaction == null) return;
            commands.Delete(transaction);
        }

        public void CreateIncome(IncomeItem income)
        {
            var commands = _unitOfWork.GetCommandRepository<Transaction>();
            var transaction = new Transaction
            {
                OwnerId = income.OwnerId,
                InitiatorId = income.AccountId,
                DateTime = income.DateTime,
                IncomeId = income.Id,
                Total = income.Total
            };
            commands.Create(transaction);
        }

        public async Task UpdateIncome(int accountId, IncomeItem income)
        {
            var commands = _unitOfWork.GetCommandRepository<Transaction>();
            var transaction =
                await _transactionQueries.GetIncomeTransaction(accountId, income.Id).ConfigureAwait(false);
            if (transaction == null) return;
            transaction.InitiatorId = income.AccountId;
            transaction.Total = income.Total;
            commands.Update(transaction.ToEntity());
        }

        public void CreateTransfer(Account accountFrom, Account accountTo, DateTime transferTime, decimal amount)
        {
            if (accountFrom.Id == accountTo.Id)
                throw new InvalidOperationException("Cannot process transfer between same participants");

            var commands = _unitOfWork.GetCommandRepository<Transaction>();
            var transaction1 = new Transaction
            {
                OwnerId = accountFrom.OwnerId,
                DateTime = transferTime,
                InitiatorId = accountFrom.Id,
                ParticipantId = accountTo.Id,
                Total = -amount,
            };
            var transaction2 = new Transaction
            {
                OwnerId = accountTo.OwnerId,
                DateTime = transferTime,
                InitiatorId = accountTo.Id,
                ParticipantId = accountFrom.Id,
                Total = amount
            };
            commands.Create(transaction1);
            commands.Create(transaction2);
        }
    }
}