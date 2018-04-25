﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Auth;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Incomes
{
    public class IncomeItem : IHasId, IHasOwnerId
    {
        public int Id { get; set; }

        public int IncomeTypeId { get; set; }

        [ForeignKey("IncomeTypeId")]
        public IncomeType IncomeType { get; set; }

        public DateTime DateTime { get; set; }

        public decimal Total { get; set; }

        public int AccountId { get; set; }

        [ForeignKey("AccountId")]
        public Account Account { get; set; }
        
        public int? OwnerId { get; set; }
        
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        public bool IsCorrection { get; set; }
    }
}