using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.Web.Models;
using Monifier.Web.Models.Products;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Products
{
    public class CategoriesModel : PageModel
    {
        private readonly ICategoriesQueries _categoriesQueries;
        private readonly ICategoriesCommands _categoriesCommands;

        public CategoriesModel(ICategoriesQueries categoriesQueries, ICategoriesCommands categoriesCommands)
        {
            _categoriesQueries = categoriesQueries;
            _categoriesCommands = categoriesCommands;
        }
        
        public List<CategoryModel> Categories { get; private set; }
        
        [BindProperty]
        public AddCategory AddCategory { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _categoriesQueries.GetAll();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await AddCategory.TryProcessAsync<ArgumentException>(ModelState,
                async () =>
                {
                    await _categoriesCommands.Update(new CategoryModel {Name = AddCategory.Category});
                    return RedirectToPage("./Categories");
                },
                async () =>
                {
                    Categories = await _categoriesQueries.GetAll();
                    return Page();
                },
                async vrList =>
                {
                    if (string.IsNullOrEmpty(AddCategory.Category)) return;
                    var category = await _categoriesQueries.GetByName(AddCategory.Category);
                    if (category != null) vrList.Add(new ModelValidationResult("AddCategory.Category", 
                        "Категория товаров с таким именем уже есть"));
                });
        }
    }
}