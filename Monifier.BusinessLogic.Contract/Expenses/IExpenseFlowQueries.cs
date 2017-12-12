using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Expenses;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface IExpenseFlowQueries : ICommonModelQueries<ExpenseFlowModel>
    {
        Task<int> GetNextNumber();
        Task<ExpenseFlowModel> GetWithCategories(int id);
    }
}