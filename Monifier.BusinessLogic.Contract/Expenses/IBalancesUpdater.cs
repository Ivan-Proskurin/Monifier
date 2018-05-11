using System.Threading.Tasks;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface IBalancesUpdater
    {
        Task Create(Account account, ExpenseBill bill);
        Task Delete(ExpenseBill bill);
    }
}