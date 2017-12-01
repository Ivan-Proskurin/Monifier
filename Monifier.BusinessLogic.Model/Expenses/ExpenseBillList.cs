using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Model.Expenses
{
    public class ExpenseBillList
    {
        public List<ExpenseBillModel> List { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}
