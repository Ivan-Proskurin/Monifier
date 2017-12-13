﻿using System.ComponentModel.DataAnnotations;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Incomes
{
    public class IncomeType : IHasId, IHasName
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
