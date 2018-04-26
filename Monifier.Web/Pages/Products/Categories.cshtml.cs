using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Inventorization;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Inventorization;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Products;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Products
{
    [Authorize]
    public class CategoriesModel : PageModel
    {
        private readonly ICategoriesQueries _categoriesQueries;
        private readonly ICategoriesCommands _categoriesCommands;
        private readonly IInventorizationQueries _inventorizationQueries;

        public CategoriesModel(
            ICategoriesQueries categoriesQueries, 
            ICategoriesCommands categoriesCommands,
            IInventorizationQueries inventorizationQueries)
        {
            _categoriesQueries = categoriesQueries;
            _categoriesCommands = categoriesCommands;
            _inventorizationQueries = inventorizationQueries;
        }
        
        public CategoryList Categories { get; private set; }
        
        [BindProperty]
        public AddCategory AddCategory { get; set; }

        public BalanceState BalanceState { get; private set; }

        private async Task LoadCategoriesAsync(int pageNumber)
        {
            Categories = await _categoriesQueries.GetList(new PaginationArgs
            {
                IncludeDeleted = false,
                ItemsPerPage = 6,
                PageNumber = pageNumber
            });
            
            AddCategory = new AddCategory
            {
                PageNumber = Categories.Pagination.PageNumber,
            };
        }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            BalanceState = await _inventorizationQueries.GetBalanceState();
            await LoadCategoriesAsync(pageNumber);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            BalanceState = await _inventorizationQueries.GetBalanceState();
            return await AddCategory.ProcessAsync(ModelState, nameof(AddCategory),
                async () =>
                {
                    await _categoriesCommands.Update(new CategoryModel {Name = AddCategory.Category});
                    return RedirectToPage("./Categories", new { pageNumber = -1 });
                },
                async () =>
                {
                    await LoadCategoriesAsync(AddCategory.PageNumber);
                    return Page();
                },
                async vrList =>
                {
                    if (!AddCategory.Category.IsNullOrEmpty())
                    {
                        var category = await _categoriesQueries.GetByName(AddCategory.Category);
                        if (category != null) vrList.Add(new ModelValidationResult(nameof(AddCategory.Category),
                            "Категория товаров с таким именем уже есть"));
                    }
                });
        }
    }
}