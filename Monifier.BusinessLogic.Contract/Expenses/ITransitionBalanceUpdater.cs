using System.Threading.Tasks;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface ITransitionBalanceUpdater
    {
        Task Update(ExpenseBill bill, decimal oldSum, int? oldAccountId);
    }
}