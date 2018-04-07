using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface IExpenseFlowQueries : ICommonModelQueries<ExpenseFlowModel>
    {
        Task<int> GetNextNumber();
        Task<ExpenseFlowModel> GetWithCategories(int id);
        Task<ExpenseFlowList> GetList(PaginationArgs args);
        Task<int?> GetIdByName(string name);
        Task<string> GetNameById(int? id);
    }
}