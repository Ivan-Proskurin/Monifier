using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Model.Transactions
{
    public class TransactionPaginatedList
    {
        public List<TransactionViewModel> Transactions { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}