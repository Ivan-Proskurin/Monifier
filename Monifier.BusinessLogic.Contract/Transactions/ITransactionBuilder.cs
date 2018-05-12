using System;
using System.Threading.Tasks;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Contract.Transactions
{
    public interface ITransactionBuilder
    {
        void CreateExpense(ExpenseBill bill);
        Task UpdateExpense(ExpenseBill bill, int? oldAccountId);
        Task DeleteExpense(ExpenseBill bill);
        void CreateIncome(IncomeItem income);
        Task UpdateIncome(int accountId, IncomeItem income);
        void CreateTransfer(Account accountFrom, Account accountTo, DateTime transferTime, decimal amount);
    }
}