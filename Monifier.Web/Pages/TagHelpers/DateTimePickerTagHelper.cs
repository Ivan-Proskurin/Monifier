using Microsoft.AspNetCore.Mvc.Rendering;

namespace Monifier.Web.Pages.TagHelpers
{
    public class DateTimePickerTagHelper : PartialTagHelper
    {
        public DateTimePickerTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }
        
        public string AspFor { get; set; }
        
        public string Value { get; set; }

        protected override void Setup()
        {
            Name = "_DatetimePickerPartial";
            Model = new FieldBinding
            {
                Name = AspFor,
                Value = Value
            };
        }
    }
}