using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Expenses;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface IExpensesBillCommands : ICommonModelCommands<ExpenseBillModel>
    {
        Task Create(ExpenseBillModel model);
    }
}
