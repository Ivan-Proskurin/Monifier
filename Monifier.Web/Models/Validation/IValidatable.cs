using System.Collections.Generic;

namespace Monifier.Web.Models.Validation
{
    public interface IValidatable
    {
        IEnumerable<ModelValidationResult> Validate();
    }
}