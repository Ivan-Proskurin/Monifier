using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Auth;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Base
{
    public class Account : IHasId, IHasName, IHasOwnerId
    {
        public int Id { get; set; }
        
        [Range(1, Double.MaxValue, ErrorMessage = "Номер должен начинаться с единицы (1)")]
        public int Number { get; set; }
        
        public DateTime DateCreated { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Название счета не должно превышать 50 символов")]
        public string Name { get; set; }

        public decimal Balance { get; set; }
        
        public decimal AvailBalance { get; set; }
        
        public DateTime? LastWithdraw { get; set; }

        public bool IsDeleted { get; set; }
        
        public int? OwnerId { get; set; }
        
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        public bool IsDefault { get; set; }

        public AccountType AccountType { get; set; }
    }
}