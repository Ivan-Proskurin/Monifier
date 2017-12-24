using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Base;
using Monifier.Common.Extensions;
using Monifier.Web.Api.Models;
using Monifier.Web.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;
using Monifier.Web.Models.Validation;
using Newtonsoft.Json;

namespace Monifier.Web.Pages.Expenses
{
    public class AddExpenseModel : PageModel
    {
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IProductQueries _productQueries;
        private readonly IExpenseFlowCommands _expenseFlowCommands;
        private readonly ICategoriesQueries _categoriesQueries;
        private readonly ICategoriesCommands _categoriesCommands;
        private readonly IProductCommands _productCommands;

        public AddExpenseModel(
            IExpenseFlowQueries expenseFlowQueries,
            IProductQueries productQueries,
            IExpenseFlowCommands expenseFlowCommands,
            ICategoriesQueries categoriesQueries,
            ICategoriesCommands categoriesCommands,
            IProductCommands productCommands)
        {
            _expenseFlowQueries = expenseFlowQueries;
            _productQueries = productQueries;
            _expenseFlowCommands = expenseFlowCommands;
            _categoriesQueries = categoriesQueries;
            _categoriesCommands = categoriesCommands;
            _productCommands = productCommands;
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

        private async Task PrepareToInputNewExpense(int flowId)
        {
            await PrepareModels(flowId);
            var flow = await _expenseFlowQueries.GetById(flowId);
            Expense = new EditExpense
            {
                ExpenseFlowId = flowId,
                FlowName = flow.Name,
                DateTime = DateTime.Now.ToStandardString(),
                Cost = string.Empty,
                ContinueInput = true,
            };
            if (Categories.Count == 1)
            {
                Expense.Category = Categories.First().Name;
            }
        }

        public async Task OnGetAsync(int expenseId)
        {
            await PrepareToInputNewExpense(expenseId);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Expense.ProcessAsync(ModelState,
                async () =>
                {
                    await _expenseFlowCommands.AddExpense(Expense.ToModel());
                    if (!Expense.ContinueInput) return RedirectToPage("./ExpenseFlows");
                    await PrepareToInputNewExpense(Expense.ExpenseFlowId);
                    return Page();
                },
                async () =>
                {
                    await PrepareModels(Expense.ExpenseFlowId);
                    return Page();
                },
                async vrList =>
                {
                    CategoryModel category = null;
                    if (!string.IsNullOrEmpty(Expense.Category))
                    {
                        category = await _categoriesQueries.GetFlowCategoryByName(Expense.ExpenseFlowId, Expense.Category);
                        if (category == null)
                        {
                            if (Expense.Category == Expense.CategoryToAdd)
                            {
                                category = await _categoriesCommands.CreateNewOrBind(
                                    Expense.ExpenseFlowId, Expense.Category);
                                Expense.CategoryToAdd = null;
                            }
                            else
                            {
                                vrList.Add(new ModelValidationResult("Expense.Category", 
                                    "Нет такой категории, добавить ее?"));
                                Expense.CategoryToAdd = Expense.Category;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(Expense.Product))
                    {
                        var product = await _productQueries.GetFlowProductByName(Expense.ExpenseFlowId, Expense.Product);
                        if (product == null)
                        {
                            if (Expense.Product == Expense.ProductToAdd && category != null)
                            {
                                await _productCommands.AddProductToCategory(category.Id, Expense.Product);
                                Expense.ProductToAdd = null;
                            }
                            else
                            {
                                vrList.Add(new ModelValidationResult("Expense.Product", 
                                    "Нет такого товара, добавить его?"));
                                Expense.ProductToAdd = Expense.Product;
                            }
                        }
                    }
                });
        }

        public async Task<JsonResult> OnPostGetCategoryProductsAsync()
        {
            return await this.ProcessAjaxPostRequestAsync(async name =>
            {
                var products = await _categoriesQueries.GetProductsByCategoryName(name);
                return products.Select(x => x.Name).ToList();
            });
        }

        public async Task<JsonResult> OnPostGetCategoryByProductAsync()
        {
            return await this.ProcessAjaxPostRequestAsync(async rspargs =>
                {
                    var args = JsonConvert.DeserializeObject<GetCategoryArgs>(rspargs);
                    return (await _categoriesQueries.GetFlowCategoryByProductName(args.FlowId, args.Product))?.Name;
                }
            );
        }

        public async Task<JsonResult> OnPostGetFlowProducts()
        {
            return await this.ProcessAjaxPostRequestAsync(async args =>
            {
                var products = await _productQueries.GetExpensesFlowProducts(int.Parse(args));
                return products.Select(x => x.Name).ToList();
            });
        }
    }
}