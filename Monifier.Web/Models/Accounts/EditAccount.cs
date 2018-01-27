using System;
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
        
        public string OriginalName { get; set; }
        
        [Display(Name = "Номер")]
        [Required(ErrorMessage = "Введите номер")]
        [Range(1, Double.MaxValue, ErrorMessage = "Номер счета должен начинаться с единицы (1)")]
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
                OriginalName = account.Name,
                Name = account.Name,
                CreationDate = account.DateCreated.ToStandardString(),
                Balance = account.Balance.ToStandardString()
            };
        }

        public static void ToAccountModel(this EditAccount account, AccountModel model)
        {
            model.Number = account.Number;
            model.DateCreated = account.CreationDate.ParseDtFromStandardString();
            model.Name = account.Name;
            model.Balance = account.Balance.ParseMoneyInvariant();
        }
    }
}