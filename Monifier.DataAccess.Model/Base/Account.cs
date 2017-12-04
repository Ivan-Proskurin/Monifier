using System;
using System.ComponentModel.DataAnnotations;
using Monifier.DataAccess.Contract.Model;

namespace Monifier.DataAccess.Model.Base
{
    public class Account : IHasId, IHasName
    {
        public int Id { get; set; }
        
        public int Number { get; set; }
        
        public DateTime DateCreated { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsDeleted { get; set; }
    }
}