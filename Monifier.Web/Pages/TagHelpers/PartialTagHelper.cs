using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Monifier.Web.Pages.TagHelpers
{
    public class PartialTagHelper : TagHelper
    {
        private readonly IHtmlHelper _htmlHelper;

        public PartialTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }
        
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        
        public string Name { get; set; }
        
        public object Model { get; set; }

        protected virtual void Setup()
        {
            
        }
        
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            Setup();
            ((IViewContextAware)_htmlHelper).Contextualize(ViewContext);
            var content = _htmlHelper.Partial(Name, Model);
            output.TagName = null;
            output.Content.SetHtmlContent(content);
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}