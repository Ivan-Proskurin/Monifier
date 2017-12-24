using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Products
{
    public class EditCategory : IValidatable
    {
        public int Id { get; set; } 
        
        [Required(ErrorMessage = "Введите название категории товаров")]
        [StringLength(50, ErrorMessage = "Название не должно превышать 50 символов")]
        public string Category { get; set; }
        
        public string AddProduct { get; set; }
        
        public int PageNumber { get; set; }

        public IEnumerable<ModelValidationResult> Validate()
        {
            yield break;
        }
    }
}