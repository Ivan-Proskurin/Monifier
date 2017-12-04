using System.Collections.Generic;

namespace Monifier.Web.Pages.TagHelpers
{
    public class AutocompleteBinding
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public IEnumerable<string> ValueList { get; set; }
    }
}