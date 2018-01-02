using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpenseFlowList
    {
        public List<ExpenseFlowModel> ExpenseFlows { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}