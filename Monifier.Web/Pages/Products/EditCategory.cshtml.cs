using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Products;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Products
{
    [Authorize]
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

        public ProductList Products { get; private set; }

        private async Task<ProductList> LoadProductsAsync(int categoryId, int pageNumber)
        {
            return await _productQueries.GetList(categoryId, new PaginationArgs
            {
                IncludeDeleted = false,
                ItemsPerPage = 5,
                PageNumber = pageNumber
            });
        }

        public async Task OnGetAsync(int id, int pageNumber = 1)
        {
            var category = await _categoriesQueries.GetById(id);
            Category = new EditCategory
            {
                Id = category.Id,
                Category = category.Name
            };

            Products = await LoadProductsAsync(id, pageNumber);

            Category.PageNumber = Products.Pagination.PageNumber;
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            return await Category.ProcessAsync(ModelState, nameof(Category),
                async () =>
                {
                    var category = await _categoriesQueries.GetByName(Category.Category);
                    if (category == null)
                    {
                        await _categoriesCommands.Update(new CategoryModel
                        {
                            Id = Category.Id,
                            Name = Category.Category
                        });
                    }
                    return RedirectToPage("./Categories", new {pageNumber = 1});
                },
                async () =>
                {
                    Products = await LoadProductsAsync(Category.Id, Category.PageNumber);
                    return Page();
                });
        }

        public async Task<IActionResult> OnPostAddProductAsync()
        {
            const string prop = "Category.AddProduct";
            
            return await Category.ProcessAsync(ModelState, nameof(Category),
                async () =>
                {
                    await _productCommands.AddProductToCategory(Category.Id, Category.AddProduct);
                    return RedirectToPage("./EditCategory", new {id = Category.Id, pageNumber = -1});
                },
                async () =>
                {
                    Products = await LoadProductsAsync(Category.Id, Category.PageNumber);
                    return Page();
                },
                async vrList =>
                {
                    if (Category.AddProduct.IsNullOrEmpty())
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
                    var product = await _productQueries.GetByName(Category.AddProduct); 
                    if (product != null)
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