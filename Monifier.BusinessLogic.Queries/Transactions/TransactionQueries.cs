using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.BusinessLogic.Model.Transactions;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;
using Monifier.DataAccess.Model.Transactions;

namespace Monifier.BusinessLogic.Queries.Transactions
{
    public class TransactionQueries : ITransactionQueries
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;

        public TransactionQueries(IEntityRepository repository, ICurrentSession currentSession)
        {
            _repository = repository;
            _currentSession = currentSession;
        }

        public Task<List<TransactionModel>> GetAll(bool includeDeleted = false)
        {
            var queries = _repository.GetQuery<Transaction>();
            var ownerId = _currentSession.UserId;
            return queries
                .Where(x => x.OwnerId == ownerId && (!includeDeleted || x.IsDeleted == false))
                .Select(x => x.ToModel())
                .ToListAsync();
        }

        public async Task<TransactionModel> GetById(int id)
        {
            var entity = await _repository.LoadAsync<Transaction>(id).ConfigureAwait(false);
            return entity?.ToModel();
        }

        public Task<TransactionModel> GetByName(string name, bool includeDeleted = false)
        {
            throw new NotImplementedException();
        }

        public Task<List<TransactionModel>> GetInitiatorTransactions(int initiatorId)
        {
            var queries = _repository.GetQuery<Transaction>();
            var ownerId = _currentSession.UserId;
            return queries
                .Where(x => x.OwnerId == ownerId && x.InitiatorId == initiatorId && !x.IsDeleted)
                .Select(x => x.ToModel())
                .ToListAsync();
        }

        public Task<TransactionModel> GetBillTransaction(int initiatorId, int billId)
        {
            var queries = _repository.GetQuery<Transaction>();
            var ownerId = _currentSession.UserId;
            return queries
                .Where(x => x.OwnerId == ownerId && x.InitiatorId == initiatorId && x.BillId == billId && !x.IsDeleted)
                .Select(x => x.ToModel())
                .SingleOrDefaultAsync();
        }

        public Task<TransactionModel> GetIncomeTransaction(int initiatorId, int incomeId)
        {
            var queries = _repository.GetQuery<Transaction>();
            var ownerId = _currentSession.UserId;
            return queries
                .Where(x => x.OwnerId == ownerId && x.InitiatorId == initiatorId && x.IncomeId == incomeId &&
                            !x.IsDeleted)
                .Select(x => x.ToModel())
                .SingleOrDefaultAsync();
        }

        public Task<TransactionModel> GetTransferTransaction(int initiatorId, int participantId)
        {
            var queries = _repository.GetQuery<Transaction>();
            var ownerId = _currentSession.UserId;
            return queries
                .Where(x => x.OwnerId == ownerId && x.InitiatorId == initiatorId && x.ParticipantId == participantId &&
                            !x.IsDeleted)
                .Select(x => x.ToModel())
                .SingleOrDefaultAsync();
        }

        public async Task<List<TransactionViewModel>> GetLastTransactions(int initiatorId, int limit = 5)
        {
            if (limit > 100)
                throw new ArgumentOutOfRangeException($"{nameof(limit)} is maximum of 100, sorry. Use GetAllTransaction nethod instead", nameof(limit));

            var paginated = await GetAllTransactions(new TransactionFilter
                {
                    AccountId = initiatorId
                },
                new PaginationArgs
                {
                    ItemsPerPage = limit, PageNumber = 1
                }).ConfigureAwait(false);
            return paginated.Transactions;
        }

        public async Task<TransactionPaginatedList> GetAllTransactions(TransactionFilter filter, PaginationArgs paginationArgs)
        {
            var initiatorId = filter.AccountId;
            var queries = _repository.GetQuery<Transaction>();
            var ownerId = _currentSession.UserId;
            var itemsQuery = queries
                .Where(x => x.OwnerId == ownerId && x.InitiatorId == initiatorId && !x.IsDeleted)
                .OrderByDescending(x => x.DateTime)
                .Select(x => new
                {
                    x.DateTime,
                    Total = GetTransactionTotal(x),
                    Type = GetTransactionViewType(x),
                    Transaction = x,
                })
                .Where(x => string.IsNullOrEmpty(filter.Operation) || x.Type == filter.Operation);

            var pagination = new PaginationInfo(paginationArgs, await itemsQuery.CountAsync().ConfigureAwait(false));

            var items = await itemsQuery
                .Skip(pagination.Skipped)
                .Take(pagination.Taken)
                .ToListAsync()
                .ConfigureAwait(false);

            var list = new List<TransactionViewModel>();
            foreach (var item in items)
            {
                var model = new TransactionViewModel
                {
                    IsExpense = item.Total < 0,
                    DateTime = item.DateTime,
                    Total = item.Total > 0 ? $"+{item.Total.ToMoney()}" : item.Total.ToMoney(),
                    Type = item.Type,
                    Target = await GetTransactionTarget(item.Transaction),
                    Balance = item.Transaction.Balance?.ToMoney()
                };
                list.Add(model);
            }

            return new TransactionPaginatedList
            {
                Transactions = list,
                Pagination = pagination
            };
        }

        private static decimal GetTransactionTotal(Transaction transaction)
        {
            if (transaction.BillId != null)
                return -transaction.Total;
            if (transaction.IncomeId != null)
                return transaction.Total;
            if (transaction.ParticipantId != null)
                return transaction.Total;
            throw new InvalidOperationException("Invalid transaction configuration");
        }

    private static string GetTransactionViewType(Transaction transaction)
        {
            if (transaction.BillId != null)
                return "Оплата";
            if (transaction.IncomeId != null)
                return "Поступление";
            if (transaction.ParticipantId != null)
                return "Перевод";
            return "Неизвестный тип транзакции";
        }

        private async Task<string> GetTransactionTarget(Transaction transaction)
        {
            if (transaction.BillId != null)
            {
                var bill = await _repository.LoadAsync<ExpenseBill>(transaction.BillId.Value).ConfigureAwait(false);
                var flow = await _repository.LoadAsync<ExpenseFlow>(bill.ExpenseFlowId).ConfigureAwait(false);
                return flow.Name;
            }

            if (transaction.IncomeId != null)
            {
                var income = await _repository.LoadAsync<IncomeItem>(transaction.IncomeId.Value).ConfigureAwait(false);
                var incomeType = await _repository.LoadAsync<IncomeType>(income.IncomeTypeId).ConfigureAwait(false);
                return incomeType.Name;
            }

            if (transaction.ParticipantId != null)
            {
                var account = await _repository.LoadAsync<Account>(transaction.ParticipantId.Value);
                return account.Name;
            }
            return "Неизвестный тип назначения";
        }
    }
}