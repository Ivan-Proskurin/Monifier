using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;
using Monifier.DataAccess.Model.Transactions;

namespace Monifier.BusinessLogic.Queries.Transactions
{
    public class TransactionBuilder : ITransactionBuilder
    {
        private readonly IEntityRepository _repository;
        private readonly ITransactionQueries _transactionQueries;

        public TransactionBuilder(IEntityRepository repository, ITransactionQueries transactionQueries)
        {
            _repository = repository;
            _transactionQueries = transactionQueries;
        }

        public void CreateExpense(ExpenseBill bill, decimal balance)
        {
            if (bill.AccountId == null) return;
            var transation = new Transaction
            {
                OwnerId = bill.OwnerId,
                DateTime = bill.DateTime,
                InitiatorId = bill.AccountId.Value,
                Bill = bill,
                Total = bill.SumPrice,
                Balance = balance
            };
            _repository.Create(transation);
        }

        public async Task UpdateExpense(ExpenseBill bill, int? oldAccountId, decimal balance)
        {
            var transactionQueries = _repository.GetQuery<Transaction>();
            if (oldAccountId == bill.AccountId)
            {
                var transaction = await transactionQueries
                    .SingleOrDefaultAsync(x => x.OwnerId == bill.OwnerId
                                               && x.InitiatorId == bill.AccountId && x.BillId == bill.Id)
                    .ConfigureAwait(false);
                if (transaction != null)
                {
                    transaction.DateTime = bill.DateTime;
                    transaction.Total = bill.SumPrice;
                    transaction.Balance = balance;
                    _repository.Update(transaction);
                }
            }
            else if (bill.AccountId != null)
            {
                var transaction = await transactionQueries
                    .SingleOrDefaultAsync(x => x.OwnerId == bill.OwnerId
                                               && x.InitiatorId == oldAccountId && x.BillId == bill.Id)
                    .ConfigureAwait(false);

                if (transaction == null)
                {
                    transaction = new Transaction
                    {
                        OwnerId = bill.OwnerId,
                        DateTime = bill.DateTime,
                        BillId = bill.Id,
                        InitiatorId = bill.AccountId.Value,
                        Total = bill.SumPrice,
                        Balance = balance
                    };
                    _repository.Create(transaction);
                }
                else
                {
                    transaction.InitiatorId = bill.AccountId.Value;
                    transaction.Total = bill.SumPrice;
                    transaction.Balance = balance;
                    _repository.Update(transaction);
                }
            }
        }

        public async Task DeleteExpense(ExpenseBill bill)
        {
            var queries = _repository.GetQuery<Transaction>();
            var transaction = await queries.SingleOrDefaultAsync(
                    x => x.InitiatorId == bill.AccountId && x.BillId == bill.Id)
                .ConfigureAwait(false);
            if (transaction == null) return;
            _repository.Delete(transaction);
        }

        public void CreateIncome(IncomeItem income, decimal balance)
        {
            var transaction = new Transaction
            {
                OwnerId = income.OwnerId,
                InitiatorId = income.AccountId,
                DateTime = income.DateTime,
                IncomeId = income.Id,
                Total = income.Total,
                Balance = balance
            };
            _repository.Create(transaction);
        }

        public async Task UpdateIncome(int accountId, IncomeItem income, decimal balance)
        {
            var transaction =
                await _transactionQueries.GetIncomeTransaction(accountId, income.Id).ConfigureAwait(false);
            if (transaction == null) return;
            transaction.InitiatorId = income.AccountId;
            transaction.Total = income.Total;
            transaction.Balance = balance;
            _repository.Update(transaction.ToEntity());
        }

        public void CreateTransfer(Account accountFrom, Account accountTo, DateTime transferTime, decimal amount)
        {
            if (accountFrom.Id == accountTo.Id)
                throw new InvalidOperationException("Cannot process transfer between same participants");

            var transaction1 = new Transaction
            {
                OwnerId = accountFrom.OwnerId,
                DateTime = transferTime,
                InitiatorId = accountFrom.Id,
                ParticipantId = accountTo.Id,
                Total = -amount,
                Balance = accountFrom.Balance
            };
            var transaction2 = new Transaction
            {
                OwnerId = accountTo.OwnerId,
                DateTime = transferTime,
                InitiatorId = accountTo.Id,
                ParticipantId = accountFrom.Id,
                Total = amount,
                Balance = accountTo.Balance
            };
            _repository.Create(transaction1);
            _repository.Create(transaction2);
        }
    }
}