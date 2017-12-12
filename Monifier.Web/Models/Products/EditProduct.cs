using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Monifier.BusinessLogic.Model.Base;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Models.Products
{
    public class EditProduct : IValidatable
    {
        public int CategoryId { get; set; }
        
        public int Id { get; set; }
        
        public string OriginalName { get; set; }
        
        [Required(ErrorMessage = "Введите назавние товара")]
        [StringLength(50, ErrorMessage = "Назавние товара не должно превышать 50 символов")]
        public string Name { get; set; }
        
        public IEnumerable<ModelValidationResult> Validate()
        {
            yield break;
        }
    }

    public static class EditProductMapper
    {
        public static ProductModel ToModel(this EditProduct product)
        {
            return new ProductModel
            {
                CategoryId = product.CategoryId,
                Id = product.Id,
                Name = product.Name
            };
        }

        public static EditProduct ToEditProduct(this ProductModel model)
        {
            return new EditProduct
            {
                CategoryId = model.CategoryId,
                Id = model.Id,
                Name = model.Name,
                OriginalName = model.Name
            };
        }
    }
}