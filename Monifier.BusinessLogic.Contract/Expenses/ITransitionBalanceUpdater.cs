using System.Threading.Tasks;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface ITransitionBalanceUpdater
    {
        Task<decimal> Update(ExpenseBill bill, decimal oldSum, int? oldAccountId);
    }
}