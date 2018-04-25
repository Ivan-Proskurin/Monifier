﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Auth;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Expenses
{
    public class ExpenseBill : IHasId, IHasOwnerId
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public virtual ICollection<ExpenseItem> Items { get; set; }

        public decimal SumPrice { get; set; }

        public int ExpenseFlowId { get; set; }
        
        [ForeignKey("ExpenseFlowId")]
        public ExpenseFlow ExpenseFlow { get; set; }
        
        public int? AccountId { get; set; }
        
        [ForeignKey("AccountId")]
        public Account Account { get; set; }
        
        public int? OwnerId { get; set; }
        
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        public bool IsCorrection { get; set; }
    }
}