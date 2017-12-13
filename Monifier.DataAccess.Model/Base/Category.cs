using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Base
{
    public class Category : IHasName, IHasId
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
