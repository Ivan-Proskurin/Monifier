using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.BusinessLogic.Model.Base;
using Monifier.Common.Extensions;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Accounts
{
    public class EditAccount : IValidatable
    {
        public int Id { get; set; }
        
        [Display(Name = "Номер")]
        public int Number { get; set; }
        
        [Display(Name = "Дата создания")]
        [Required(ErrorMessage = "Введите время создания")]
        public string CreationDate { get; set; }
        
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Введите название счета")]
        public string Name { get; set; }
        
        [Display(Name = "Баланс")]
        [Required(ErrorMessage = "Введите баланс")]
        public string Balance { get; set; }
        
        public IEnumerable<ModelValidationResult> Validate()
        {
            var balanceResult = Balance.ValidateMoney("Account.Balance");
            if (balanceResult != null) yield return balanceResult;
            var dateResult = CreationDate.ValidateDateTime("Account.CreationDate");
            if (dateResult != null) yield return dateResult;
        }
    }

    public static class EditAccountMapper
    {
        public static EditAccount ToEditAccount(this AccountModel account)
        {
            return new EditAccount
            {
                Id = account.Id,
                Number = account.Number,
                Name = account.Name,
                CreationDate = account.DateCreated.ToStandardString(),
                Balance = account.Balance.ToStandardString()
            };
        }

        public static AccountModel ToAccountModel(this EditAccount account)
        {
            return new AccountModel
            {
                Id = account.Id,
                Number = account.Number,
                Name = account.Name,
                DateCreated = account.CreationDate.ParseDtFromStandardString(),
                Balance = account.Balance.ParseMoneyInvariant()
            };
        }
    }
}