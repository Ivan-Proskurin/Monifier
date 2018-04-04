using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;

namespace Monifier.Web.Pages.Expenses
{
    [Authorize]
    public class EditExpenseFlowModel : PageModel
    {
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IExpenseFlowCommands _expenseFlowCommands;
        private readonly ICategoriesQueries _categoriesQueries;

        public EditExpenseFlowModel(IExpenseFlowQueries expenseFlowQueries, 
            IExpenseFlowCommands expenseFlowCommands,
            ICategoriesQueries categoriesQueries)
        {
            _expenseFlowQueries = expenseFlowQueries;
            _expenseFlowCommands = expenseFlowCommands;
            _categoriesQueries = categoriesQueries;
        }
        
        [BindProperty]
        public EditExpenseFlow ExpenseFlow { get; set; }
        
        public List<CategoryModel> Categories { get; private set; }
        
        public List<CategoryModel> FlowCategories { get; private set; }

        private async Task LoadCategories()
        {
            Categories = await _categoriesQueries.GetAll();
            FlowCategories = ExpenseFlow.Categories.ToIntList()
                .Select(id => Categories.FirstOrDefault(c => c.Id == id))
                .Where(x => x != null)
                .ToList();
        }

        public async Task OnGetAsync(int id)
        {
            ExpenseFlow = (await _expenseFlowQueries.GetWithCategories(id)).ToEditExpenseFlow();
            await LoadCategories();
        }

        public async Task<IActionResult> OnPostCommitAsync()
        {
            return await ExpenseFlow.ProcessAsync(ModelState, nameof(ExpenseFlow),
                async () =>
                {
                    var model = await _expenseFlowQueries.GetById(ExpenseFlow.Id);
                    ExpenseFlow.ToExpenseFlowModel(model);
                    await _expenseFlowCommands.Update(model);
                    return RedirectToPage("./ExpenseFlows");
                },
                
                async () =>
                {
                    await LoadCategories();
                    return Page();
                });
        }
        
        public async Task<IActionResult> OnPostDeleteAsync(bool permanent)
        {
            await _expenseFlowCommands.Delete(ExpenseFlow.Id, !permanent);
            return RedirectToPage("./ExpenseFlows");
        }

        public async Task<IActionResult> OnPostAddCategoryAsync()
        {
            await LoadCategories();
            if (string.IsNullOrEmpty(ExpenseFlow.AddOrDeleteCategory))
            {
                ModelState.AddModelError(nameof(ExpenseFlow.AddOrDeleteCategory), "Введите категорию товаров");
                return Page();
            }
            
            if (FlowCategories.FindByName(ExpenseFlow.AddOrDeleteCategory) != null)
            {
                ModelState.AddModelError(nameof(ExpenseFlow.AddOrDeleteCategory), 
                    $"Категория \"{ExpenseFlow.AddOrDeleteCategory}\" уже есть в списке");
            }
            else
            {
                var category = Categories.FindByName(ExpenseFlow.AddOrDeleteCategory);
                if (category == null)
                {
                    ModelState.AddModelError(nameof(ExpenseFlow.AddOrDeleteCategory),
                        $"Нет категории \"{ExpenseFlow.AddOrDeleteCategory}\"");
                }
                else
                {
                    FlowCategories.Add(category);
                    ExpenseFlow.Categories = FlowCategories.Select(x => x.Id).ToCsvString();
                    ExpenseFlow.AddOrDeleteCategory = string.Empty;
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteCategoryAsync()
        {
            await LoadCategories();
            
            if (string.IsNullOrEmpty(ExpenseFlow.AddOrDeleteCategory))
            {
                ModelState.AddModelError(nameof(ExpenseFlow.AddOrDeleteCategory), "Введите категорию товаров");
                return Page();
            }

            var category = FlowCategories.FindByName(ExpenseFlow.AddOrDeleteCategory); 
            if (category == null)
            {
                ModelState.AddModelError(nameof(ExpenseFlow.AddOrDeleteCategory), $"Нет такой категории \"{ExpenseFlow.AddOrDeleteCategory}\"");
            }
            else
            {
                FlowCategories.Remove(category);
                ExpenseFlow.Categories = FlowCategories.Select(x => x.Id).ToCsvString();
                ExpenseFlow.AddOrDeleteCategory = string.Empty;
            }
            return Page();
        }
    }
}