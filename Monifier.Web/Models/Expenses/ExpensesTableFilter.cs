using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.Common.Extensions;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Expenses
{
    public class ExpensesTableFilter : IValidatable
    {
        [Required(ErrorMessage = "Укажите начальную дату")]
        public string DateFrom { get; set; }
        
        [Required(ErrorMessage = "Укажите конечную дату")]
        public string DateTo { get; set; }

        public DateTime DateFromAsDateTime
        {
            get => DateFrom.ParseDtFromStandardString();
            set => DateFrom = value.ToStandardString();
        }

        public DateTime DateToAsDateTime
        {
            get => DateTo.ParseDtFromStandardString();
            set => DateTo = value.ToStandardString();
        }

        public static ExpensesTableFilter CurrentWeek()
        {
            return new ExpensesTableFilter
            {
                DateFromAsDateTime = DateTime.Now.StartOfTheWeek(),
                DateToAsDateTime = DateTime.Now.EndOfTheWeek(),
            };
        }

        public IEnumerable<ModelValidationResult> Validate()
        {
            yield break;
        }
    }
}