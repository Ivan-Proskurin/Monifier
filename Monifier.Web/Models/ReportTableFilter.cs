﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.Common.Extensions;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models
{
    public class ReportTableFilter : IValidatable
    {
        public ReportTableFilter()
        {
        }

        public ReportTableFilter(string dateFrom, string dateTo)
        {
            DateFrom = dateFrom;
            DateTo = dateTo;
        }
        
        [Required(ErrorMessage = "Укажите начальную дату")]
        public string DateFrom { get; set; }
        
        [Required(ErrorMessage = "Укажите конечную дату")]
        public string DateTo { get; set; }

        public DateTime? DateFromAsDateTime
        {
            get => DateFrom?.ParseDtFromStandardString();
            set => DateFrom = value?.ToStandardString();
        }

        public DateTime? DateToAsDateTime
        {
            get => DateTo?.ParseDtFromStandardString();
            set => DateTo = value?.ToStandardString();
        }

        public static ReportTableFilter CurrentWeek()
        {
            return new ReportTableFilter
            {
                DateFromAsDateTime = DateTime.Now.StartOfTheWeek(),
                DateToAsDateTime = DateTime.Now.EndOfTheWeek(),
            };
        }

        public static ReportTableFilter CurrentMonth()
        {
            return new ReportTableFilter
            {
                DateFromAsDateTime = DateTime.Now.StartOfTheMonth(),
                DateToAsDateTime = DateTime.Now.EndOfTheMonth()
            };
        }

        public static ReportTableFilter CurrentYear()
        {
            return new ReportTableFilter
            {
                DateFromAsDateTime = DateTime.Now.StartOfTheYear(),
                DateToAsDateTime = DateTime.Now.EndOfTheYear()
            };
        }

        public IEnumerable<ModelValidationResult> Validate()
        {
            var fromResult = DateFrom.ValidateDateTime("Filter.DateFrom");
            if (fromResult != null) yield return fromResult;
            var toResult = DateTo.ValidateDateTime("Filter.DateTo");
            if (toResult != null) yield return toResult;
            
            if (!string.IsNullOrEmpty(DateFrom) && fromResult == null && 
                !string.IsNullOrEmpty(DateTo) && toResult == null && 
                DateFromAsDateTime > DateToAsDateTime)
            {
                yield return new ModelValidationResult("Filter.DateFrom", "Дата \"от\" не может быть больше даты \"до\"");
                yield return new ModelValidationResult("Filter.DateTo", "Дата \"до\" не может быть меньше даты \"от\"");
            }
        }
    }
}