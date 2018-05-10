using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Expenses;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface IExpensesBillCommands : ICommonModelCommands<ExpenseBillModel>
    {
        Task<int> Create(ExpenseBillModel model, bool correction = false);
        Task Save(ExpenseBillModel model);
    }
}
