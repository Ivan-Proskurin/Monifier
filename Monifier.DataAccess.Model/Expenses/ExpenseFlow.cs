using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Auth;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Expenses
{
    public class ExpenseFlow : IHasId, IHasName, IHasOwnerId
    {
        public int Id { get; set; }
        
        [Range(1, Double.MaxValue, ErrorMessage = "Номер должен начинаться с единицы (1)")]
        public int Number { get; set; }
        
        [Required]
        [StringLength(50, ErrorMessage = "Длина названия категории расходов не должна превышать 50 символов")]
        public string Name { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        public decimal Balance { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public int Version { get; set; }
        
        public int? OwnerId { get; set; }
        
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

    }
}