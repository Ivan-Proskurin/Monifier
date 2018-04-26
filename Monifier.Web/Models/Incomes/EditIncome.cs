using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Model.Incomes;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Incomes
{
    public class EditIncome : IValidatable
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        [Display(Name = "Счет зачисления")]
        [Required(ErrorMessage = "Укажите счет зачисления")]
        public string Account { get; set; }

        public int IncomeTypeId { get; set; }

        [Display(Name = "Статья дохода")]
        [Required(ErrorMessage = "Укажите статью дохода")]
        public string IncomeType { get; set; }

        [Display(Name = "Дата/время дохода")]
        [Required(ErrorMessage = "Укажите дату и время дохода")]
        public string DateTime { get; set; }

        [Display(Name = "Сумма")]
        [Required(ErrorMessage = "Укажите сумму дохода")]
        public string Total { get; set; }

        public int? OwnerId { get; set; }

        public bool IsCorrection { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            var validation = DateTime.ValidateDateTime(nameof(DateTime));
            if (validation != null) yield return validation;
            validation = Total.ValidateMoney(nameof(Total));
            if (validation != null) yield return validation;
        }
    }

    public static class EditIncomeExtensions
    {
        public static EditIncome ToViewModel(this IncomeItem item)
        {
            return new EditIncome
            {
                Id = item.Id,
                AccountId = item.AccountId,
                IncomeTypeId = item.IncomeTypeId,
                DateTime = item.DateTime.ToStandardString(),
                Total = item.Total.ToStandardString(),
                OwnerId = item.OwnerId,
                IsCorrection = item.IsCorrection
            };
        }

        public static IncomeItemModel ToItemModel(this EditIncome income)
        {
            return new IncomeItemModel
            {
                Id = income.Id,
                AccountId = income.AccountId,
                IncomeTypeId = income.IncomeTypeId,
                DateTime = income.DateTime.ParseDtFromStandardString(),
                Total = income.Total.ParseMoneyInvariant(),
                IsCorrection = income.IsCorrection,
            };
        }
    }
}