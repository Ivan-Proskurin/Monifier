using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Contract.Incomes
{
    public interface IIncomesQueries
    {
        Task<IncomesListModel> GetIncomesByMonth(DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs);
        Task<IncomesListModel> GetIncomesList(DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs);
        
    }
}