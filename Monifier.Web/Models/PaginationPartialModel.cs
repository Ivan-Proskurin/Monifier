using Monifier.BusinessLogic.Model.Pagination;

namespace Monifier.Web.Models
{
    public class PaginationPartialModel
    {
        public PaginationInfo Pagination { get; set; }
        public string Page { get; set; }
    }
}