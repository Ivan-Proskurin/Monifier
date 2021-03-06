﻿using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class DefaultBalancesUpdater : IBalancesUpdater
    {
        private readonly IEntityRepository _repository;

        public DefaultBalancesUpdater(IEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task Create(Account account, ExpenseBill bill)
        {
            var flow = await _repository.LoadAsync<ExpenseFlow>(bill.ExpenseFlowId).ConfigureAwait(false);
            if (flow == null)
                throw new InvalidOperationException($"Can't find expense flow with id {bill.ExpenseFlowId}");
            var lack = bill.SumPrice - Math.Max(flow.Balance, 0);
            var withdrawTotal = 0m;
            var needUpdateAccount = false;
            if (lack > 0 && account != null)
            {
                withdrawTotal = bill.SumPrice - lack;
                account.AvailBalance -= lack;
                needUpdateAccount = true;
            }
            else
                withdrawTotal = bill.SumPrice;

            flow.Balance -= withdrawTotal;
            flow.Version++;
            _repository.Update(flow);

            if (account != null && !bill.IsCorrection)
            {
                account.Balance -= bill.SumPrice;
                account.LastWithdraw = DateTime.Now;
                needUpdateAccount = true;
            }

            if (needUpdateAccount)
                _repository.Update(account);
        }

        public async Task Delete(ExpenseBill bill)
        {
            var flow = await _repository.LoadAsync<ExpenseFlow>(bill.ExpenseFlowId).ConfigureAwait(false);
            flow.Balance += bill.SumPrice;
            _repository.Update(flow);

            if (bill.AccountId != null && !bill.IsCorrection)
            {
                var account = await _repository.LoadAsync<Account>(bill.AccountId.Value);
                account.Balance += bill.SumPrice;
                _repository.Update(account);
            }
        }
    }
}