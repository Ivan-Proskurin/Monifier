using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Base;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Expenses
{
    public class AddExpenseModel : PageModel
    {
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IProductQueries _productQueries;
        private readonly IExpenseFlowCommands _expenseFlowCommands;
        private readonly ICategoriesQueries _categoriesQueries;

        public AddExpenseModel(IAccountQueries accountQueries, 
            IExpenseFlowQueries expenseFlowQueries,
            IProductQueries productQueries,
            IExpenseFlowCommands expenseFlowCommands,
            ICategoriesQueries categoriesQueries)
        {
            _expenseFlowQueries = expenseFlowQueries;
            _productQueries = productQueries;
            _expenseFlowCommands = expenseFlowCommands;
            _categoriesQueries = categoriesQueries;
        }

        private async Task PrepareModels(int expenseId)
        {
            Categories = await _categoriesQueries.GetFlowCategories(expenseId);
            Products = await _productQueries.GetExpensesFlowProducts(expenseId);
        }
        
        [BindProperty]
        public EditExpense Expense { get; set; }
        
        public List<CategoryModel> Categories { get; private set; }
        
        public List<ProductModel> Products { get; private set; }

        public async Task OnGetAsync(int expenseId)
        {
            await PrepareModels(expenseId);
            var flow = await _expenseFlowQueries.GetById(expenseId);
            Expense = new EditExpense
            {
                ExpenseFlowId = expenseId,
                ExpenseFlow = flow.Name,
                DateTime = DateTime.Now.ToStandardString()
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Expense.ProcessAsync(ModelState,
                async () =>
                {
                    await _expenseFlowCommands.AddExpense(Expense.ToModel());
                    return RedirectToPage("./ExpenseFlows");
                },
                async () =>
                {
                    await PrepareModels(Expense.ExpenseFlowId);
                    return Page();
                },
                async vrList =>
                {
                    if (!string.IsNullOrEmpty(Expense.Category))
                    {
                        var category = await _categoriesQueries.GetFlowCategoryByName(Expense.ExpenseFlowId, Expense.Category);
                        if (category == null) vrList.Add(new ModelValidationResult("Expense.Category", "Нет такой категории"));
                    }
                    if (!string.IsNullOrEmpty(Expense.Product))
                    {
                        var product = await _productQueries.GetFlowProductByName(Expense.ExpenseFlowId, Expense.Product);
                        if (product == null) vrList.Add(new ModelValidationResult("Expense.Product", "Нет такого продукта"));
                    }
                });
        }
    }
}