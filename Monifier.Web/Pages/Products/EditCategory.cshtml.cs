using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Products;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Products
{
    public class EditCategoryModel : PageModel
    {
        private readonly ICategoriesQueries _categoriesQueries;
        private readonly ICategoriesCommands _categoriesCommands;
        private readonly IProductQueries _productQueries;
        private readonly IProductCommands _productCommands;

        public EditCategoryModel(ICategoriesQueries categoriesQueries,
            ICategoriesCommands categoriesCommands,
            IProductQueries productQueries,
            IProductCommands productCommands)
        {
            _categoriesQueries = categoriesQueries;
            _categoriesCommands = categoriesCommands;
            _productQueries = productQueries;
            _productCommands = productCommands;
        }

        [BindProperty]
        public EditCategory Category { get; set; }

        public List<ProductModel> Products { get; private set; }

        public async Task OnGetAsync(int id)
        {
            var category = await _categoriesQueries.GetById(id);
            Category = new EditCategory
            {
                Id = category.Id,
                Category = category.Name
            };

            Products = await _productQueries.GetCategoryProducts(id);
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            return await Category.TryProcessAsync<ArgumentException>(ModelState,
                async () =>
                {
                    await _categoriesCommands.Update(new CategoryModel
                    {
                        Id = Category.Id,
                        Name = Category.Category
                    });
                    return RedirectToPage("./Categories");
                },
                async () =>
                {
                    Products = await _productQueries.GetCategoryProducts(Category.Id);
                    return Page();
                });
        }

        public async Task<IActionResult> OnPostAddProductAsync()
        {
            const string prop = "Category.AddProduct";
            Products = await _productQueries.GetCategoryProducts(Category.Id);
            
            return await Category.TryProcessAsync<ArgumentException>(ModelState,
                async () =>
                {
                    await _productCommands.AddProductToCategory(Category.Id, Category.AddProduct);
                    return RedirectToPage("./EditCategory", new {id = Category.Id});
                },
                async () => Page(),
                async vrList =>
                {
                    if (string.IsNullOrEmpty(Category.AddProduct))
                    {
                        vrList.Add(new ModelValidationResult(
                            prop, "Введите название товара"
                        ));
                        return;
                    }
                    if (Category.AddProduct.Length > 50)
                    {
                        vrList.Add(new ModelValidationResult(
                            prop, "Название товара должно содержать не более 50 символолв"
                        ));
                        return;
                    }
                    if (Products.FindByName(Category.AddProduct) != null)
                    {
                        vrList.Add(new ModelValidationResult(
                            prop, "Товар с таким именем уже есть"
                        ));
                    }
                }
            );
        }

        public async Task<IActionResult> OnPostDeleteAsync(bool permanent)
        {
            await _categoriesCommands.Delete(Category.Id, !permanent);
            return RedirectToPage("./Categories");
        }
    }
}