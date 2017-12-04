using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Monifier.Web.Pages.TagHelpers
{
    public class AutocompleteTagHelper : PartialTagHelper
    {
        public AutocompleteTagHelper(IHtmlHelper htmlHelper) : base(htmlHelper)
        {
        }
        
        public string AspFor { get; set; }
        
        public string Value { get; set; }
        
        public IEnumerable<string> ValueList { get; set; } 

        protected override void Setup()
        {
            Name = "_AutocompletePartial";
            Model = new AutocompleteBinding
            {
                Name = AspFor,
                Value = Value,
                ValueList = ValueList
            };
        }
    }
}