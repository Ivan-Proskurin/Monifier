using Microsoft.AspNetCore.Mvc.Rendering;

namespace Monifier.Web.Pages.TagHelpers
{
    public class DeletionModalTagHelper : PartialTagHelper
    {
        public DeletionModalTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }
        
        public string ModalId { get; set; }
        public string Caption { get; set; }
        public string Text { get; set; }
        public string PageHandler { get; set; }
        public string ButtonSoftText { get; set; }
        public string ButtonToughtText { get; set; }

        protected override void Setup()
        {
            Name = "_DeletionModalPartial";
            Model = this;
            base.Setup();
        }
    }
}