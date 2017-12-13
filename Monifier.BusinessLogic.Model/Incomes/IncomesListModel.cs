using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Agregation;
using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.BusinessLogic.Model.Incomes
{
    public class IncomesListModel
    {
        public List<IncomesListItemModel> Incomes { get; set; }
        public PaginationInfo Pagination { get; set; }
        public TotalsInfoModel Totals { get; set; }
    }
}