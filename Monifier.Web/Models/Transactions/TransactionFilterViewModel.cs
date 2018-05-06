using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Transactions;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Transactions
{
    public class TransactionFilterViewModel : IValidatable
    {
        public string Type { get; set; }
        public int PageNumber { get; set; } 
        public int AccountId { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            yield break;
        }
    }

    public static class TransactonFilterExtentions
    {
        public static TransactionFilter ToFilter(this TransactionFilterViewModel model)
        {
            return new TransactionFilter
            {
                AccountId = model.AccountId,
                Operation = model.Type,
                PageNumber = model.PageNumber
            };
        }
    }
}