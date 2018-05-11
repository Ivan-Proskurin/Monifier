using System.Threading.Tasks;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Contract.Transactions
{
    public interface ITransactionBuilder
    {
        void Create(ExpenseBill bill);
        Task Update(ExpenseBill bill, int? oldAccountId);
    }
}