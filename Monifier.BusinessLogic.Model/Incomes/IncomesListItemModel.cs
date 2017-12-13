using System.Collections.Generic;

namespace Monifier.BusinessLogic.Model.Incomes
{
    public class IncomesListItemModel
    {
        public List<int> ItemIds { get; set; }
        public decimal Sum { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string Interval { get; set; }
        public string Caption { get; set; }
        public string Types { get; set; }
    }
}