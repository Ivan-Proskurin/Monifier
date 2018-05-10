using System.Collections.Generic;

namespace Monifier.Common.Validation
{
    public interface IValidatable
    {
        IEnumerable<ModelValidationResult> Validate();
    }
}