using Microsoft.AspNetCore.Mvc.Rendering;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Web.Models;

namespace Monifier.Web.Pages.TagHelpers
{
    public class ReportPaginationTagHelper : PartialTagHelper
    {
        public ReportPaginationTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }
        
        public string AspPage { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public int? FlowId { get; set; }
        
        public PaginationInfo Pagination { get; set; }

        protected override void Setup()
        {
            Name = "_ReportPaginationPartial";
            Model = new ReportPaginationModel
            {
                Page = AspPage,
                Pagination = Pagination,
                DateFrom = DateFrom,
                DateTo = DateTo,
                FlowId = FlowId
            };
        }
    }
}