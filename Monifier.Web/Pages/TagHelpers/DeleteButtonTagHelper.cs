using Microsoft.AspNetCore.Mvc.Rendering;

namespace Monifier.Web.Pages.TagHelpers
{
    public class DeleteButtonTagHelper : PartialTagHelper
    {
        public DeleteButtonTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }
        
        public string ModalId { get; set; }
        public string Text { get; set; }

        protected override void Setup()
        {
            Name = "_DeleteButtonPartial";
            Model = this;
        }
    }
}