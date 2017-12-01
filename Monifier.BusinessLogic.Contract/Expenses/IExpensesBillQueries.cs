using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Agregation;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Contract.Expenses
{
    public interface IExpensesBillQueries : ICommonModelQueries<ExpenseBillModel>
    {
        Task<ExpenseBillList> GetFiltered(DateTime dateFrom, DateTime dateTo, PaginationArgs args);
        TotalsInfoModel GetTotals(List<ExpenseBillModel> bills);
    }
}
