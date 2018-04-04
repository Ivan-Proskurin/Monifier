using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.Web.Models;
using Monifier.Web.Models.Products;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Products
{
    [Authorize]
    public class EditProductModel : PageModel
    {
        private readonly IProductQueries _productQueries;
        private readonly IProductCommands _productCommands;

        public EditProductModel(IProductQueries productQueries, IProductCommands productCommands)
        {
            _productQueries = productQueries;
            _productCommands = productCommands;
        }

        [BindProperty]
        public EditProduct Product { get; set; }

        public async Task OnGetAsync(int id)
        {
            Product = (await _productQueries.GetById(id)).ToEditProduct();
        }

        public async Task<IActionResult> OnPostCommitAsync()
        {
            return await Product.ProcessAsync(ModelState, nameof(Product),
                async () =>
                {
                    await _productCommands.Update(Product.ToModel());
                    return RedirectToPage("./EditCategory", new {id = Product.CategoryId});
                },
                async () => await Task.FromResult(Page()),
                async vrList =>
                {
                    if (string.IsNullOrEmpty(Product.Name)) return;
                    var product = await _productQueries.GetByName(Product.Name);
                    if (product != null && product.Id != Product.Id)
                        vrList.Add(new ModelValidationResult(nameof(Product.Name), "Товар с таким именем уже есть"));
                }
            );
        }

        public async Task<IActionResult> OnPostDeleteAsync(bool permanent)
        {
            await _productCommands.Delete(Product.Id, !permanent);
            return RedirectToPage("./EditCategory", new {id = Product.CategoryId});
        }
    }
}