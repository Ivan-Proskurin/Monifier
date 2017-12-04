using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Agregation;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.Web.Models.Expenses
{
    public class ExpensesList
    {
        public List<ExpensesListItem> Items { get; set; }
        public TotalsInfoModel Totals { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}