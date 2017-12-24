using Microsoft.AspNetCore.Mvc.Rendering;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Web.Models;

namespace Monifier.Web.Pages.TagHelpers
{
    public class PaginationTagHelper : PartialTagHelper
    {
        public PaginationTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }
        
        public string AspPage { get; set; }
        
        public PaginationInfo Pagination { get; set; }

        protected override void Setup()
        {
            Name = "_PaginationPartial";
            Model = new PaginationPartialModel
            {
                Page = AspPage,
                Pagination = Pagination
            };
        }
    }
}