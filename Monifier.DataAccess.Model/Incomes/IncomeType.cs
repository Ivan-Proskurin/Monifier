using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Auth;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Incomes
{
    public class IncomeType : IHasId, IHasName, IHasOwnerId
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        public int? OwnerId { get; set; }
        
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }
    }
}
