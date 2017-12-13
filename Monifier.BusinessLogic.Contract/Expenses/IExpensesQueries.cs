using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface IExpensesQueries
    {
        Task<ExpensesListModel> GetExpensesByDay(DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs);
        Task<ExpensesListModel> GetExpensesByMonth(DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs);
        Task<ExpensesListModel> GetExpensesForDay(DateTime day);
    }
}