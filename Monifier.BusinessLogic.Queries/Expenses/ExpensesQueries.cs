using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class ExpensesQueries : IExpensesQueries
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExpensesQueries(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<ExpensesListModel> GetExpensesByDay(
            DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs)
        {
            // отбираем нужные счета и группируем их по дате
            var expenseQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();
            var itemsQuery = expenseQueries.Query
                .Where(x => x.DateTime >= dateFrom && x.DateTime < dateTo)
                .OrderBy(x => x.DateTime)
                .ThenByDescending(x => x.SumPrice)
                .GroupBy(x => x.DateTime.Date)
                .Select(x => new ExpensesListItemModel
                {
                    BillIds = x.Select(v => v.Id).ToList(),
                    Sum = x.Sum(v => v.SumPrice),
                    DateTime = x.Key
                });
            
            var totalCount = itemsQuery.Count();
            var pagination = new PaginationInfo(paginationArgs, totalCount);
            var expenseItems = await itemsQuery
                .Skip(pagination.Skipped).Take(pagination.Taken)
                .ToListAsync();

            var billIds = expenseItems.SelectMany(x => x.BillIds).Distinct();
            var billItemsQuery = _unitOfWork.GetQueryRepository<ExpenseItem>();
            

            return new ExpensesListModel
            {
                Expenses = expenseItems,
                Pagination = pagination
            };
//            var billIds = bills.Select(x => x.Id).ToList();
        }
    }
}