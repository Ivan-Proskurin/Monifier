using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface IExpensesQueries
    {
        Task<ExpensesListModel> GetExpensesByDay(ExpensesFilter filter, PaginationArgs paginationArgs);
        Task<ExpensesListModel> GetExpensesByMonth(ExpensesFilter filter, PaginationArgs paginationArgs);
        Task<ExpensesListModel> GetExpensesForDay(ExpensesFilter filter);
    }
}