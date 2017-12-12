using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Expenses;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface IExpenseFlowCommands : ICommonModelCommands<ExpenseFlowModel>
    {
        Task AddExpense(ExpenseFlowExpense expense);
    }
}