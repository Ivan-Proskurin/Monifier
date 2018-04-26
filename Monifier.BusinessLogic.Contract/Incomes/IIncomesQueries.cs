using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Contract.Incomes
{
    public interface IIncomesQueries : ICommonModelQueries<IncomeItem>
    {
        Task<IncomesListModel> GetIncomesByMonth(DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs);
        Task<IncomesListModel> GetIncomesList(DateTime dateFrom, DateTime dateTo, PaginationArgs paginationArgs);
        
    }
}