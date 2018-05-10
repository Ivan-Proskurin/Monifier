using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.Common.Validation;
using Monifier.Web.Extensions;

namespace Monifier.Web.Models.Expenses
{
    public class EditExpenseFlow : IValidatable
    {
        public int Id { get; set; }
        
        public string OriginalName { get; set; }
        
        [Display(Name = "Номер")]
        [Range(1, Double.MaxValue, ErrorMessage = "Номер категории должен начинаться с единицы (1)")]
        public int Number { get; set; }
        
        [Display(Name = "Дата создания")]
        [Required(ErrorMessage = "Введите время создания")]
        public string CreationDate { get; set; }
        
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Введите название категории")]
        public string Name { get; set; }
        
        [Display(Name = "Баланс")]
        public string Balance { get; set; }
        
        [Display(Name = "Добавить/удалить категорию")]
        public string AddOrDeleteCategory { get; set; }
        
        public string Categories { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            var dateResult = CreationDate.ValidateDateTime(nameof(CreationDate));
            if (dateResult != null) yield return dateResult;
        }
    }

    public static class EditExpenseFlowMapper
    {
        public static EditExpenseFlow ToEditExpenseFlow(this ExpenseFlowModel model)
        {
            return new EditExpenseFlow
            {
                Id = model.Id,
                Number = model.Number,
                CreationDate = model.DateCreated.ToStandardString(),
                Name = model.Name,
                OriginalName = model.Name,
                Balance = model.Balance.ToStandardString(),
                Categories = model.Categories.ToCsvString()
            };
        }

        public static void ToExpenseFlowModel(this EditExpenseFlow flow, ExpenseFlowModel model)
        {
            model.Number = flow.Number;
            model.DateCreated = flow.CreationDate.ParseDtFromStandardString();
            model.Name = flow.Name;
            model.Balance = flow.Balance.ParseMoneyInvariant();
            model.Categories = flow.Categories.ToIntList();
        }
    }
}