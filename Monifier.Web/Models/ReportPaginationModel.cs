using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.Web.Models
{
    public class ReportPaginationModel
    {
        public PaginationInfo Pagination { get; set; }
        public string Page { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
    }
}