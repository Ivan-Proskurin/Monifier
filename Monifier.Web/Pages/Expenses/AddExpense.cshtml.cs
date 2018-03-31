using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Inventorization;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.Web.Api.Models;
using Monifier.Web.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;
using Monifier.Web.Models.Validation;
using Newtonsoft.Json;

namespace Monifier.Web.Pages.Expenses
{
    [Authorize]
    public class AddExpenseModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IProductQueries _productQueries;
        private readonly IExpenseFlowCommands _expenseFlowCommands;
        private readonly ICategoriesQueries _categoriesQueries;
        private readonly ICategoriesCommands _categoriesCommands;
        private readonly IProductCommands _productCommands;
        private readonly IInventorizationQueries _inventorizationQueries;

        public AddExpenseModel(
            IAccountQueries accountQueries,
            IExpenseFlowQueries expenseFlowQueries,
            IProductQueries productQueries,
            IExpenseFlowCommands expenseFlowCommands,
            ICategoriesQueries categoriesQueries,
            ICategoriesCommands categoriesCommands,
            IProductCommands productCommands,
            IInventorizationQueries inventorizationQueries)
        {
            _accountQueries = accountQueries;
            _expenseFlowQueries = expenseFlowQueries;
            _productQueries = productQueries;
            _expenseFlowCommands = expenseFlowCommands;
            _categoriesQueries = categoriesQueries;
            _categoriesCommands = categoriesCommands;
            _productCommands = productCommands;
            _inventorizationQueries = inventorizationQueries;
        }

        private async Task PrepareModels(int expenseId)
        {
            Accounts = await _accountQueries.GetAll();
            Categories = await _categoriesQueries.GetFlowCategories(expenseId);
            Products = await _productQueries.GetExpensesFlowProducts(expenseId);
            Flows = await _expenseFlowQueries.GetAll();
        }
        
        [BindProperty]
        public EditExpense Expense { get; set; }
        
        public List<ExpenseFlowModel> Flows { get; private set; }
        
        public List<AccountModel> Accounts { get; private set; }
        
        public List<CategoryModel> Categories { get; private set; }
        
        public List<ProductModel> Products { get; private set; }

        private async Task PrepareToInputNewExpense(int flowId, bool correcting)
        {
            await PrepareModels(flowId);
            var flow = correcting ? null : await _expenseFlowQueries.GetById(flowId);
            Expense = new EditExpense
            {
                Correcting = correcting,
                Account = Accounts.GetLastUsedAccount()?.Name,
                ExpenseFlowId = flowId,
                FlowName = flow?.Name,
                DateTime = DateTime.Now.ToStandardString(),
                Cost = string.Empty,
            };
            if (Categories.Count == 1)
            {
                Expense.Category = Categories.First().Name;
            }

            if (correcting)
            {
                var balanceState = await _inventorizationQueries.GetBalanceState();
                if (balanceState.Balance < 0)
                    Expense.Cost = (-balanceState.Balance).ToStandardString();
            }
        }

        public async Task OnGetAsync(int expenseId, bool correcting = false)
        {
            await PrepareToInputNewExpense(expenseId, correcting);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Expense.ProcessAsync(ModelState, nameof(Expense),
                async () =>
                {
                    await _expenseFlowCommands.AddExpense(Expense.ToModel());
                    return RedirectToPage("./ExpenseFlows");
                },
                async () =>
                {
                    if (Expense.Correcting)
                    {
                        var flow = await _expenseFlowQueries.GetByName(Expense.FlowName);
                        if (flow != null) Expense.ExpenseFlowId = flow.Id;
                    }
                    await PrepareModels(Expense.ExpenseFlowId);
                    return Page();
                },
                async vrList =>
                {
                    var account = await _accountQueries.GetByName(Expense.Account);
                    if (account == null)
                    {
                        vrList.Add(new ModelValidationResult("Expense.Account", "Нет такого счета"));
                    }

                    if (Expense.Correcting)
                    {
                        var flow = await _expenseFlowQueries.GetByName(Expense.FlowName);
                        if (flow == null)
                        {
                            vrList.Add(new ModelValidationResult("Expense.FlowName", "Нет такой статьи расхода"));
                        }
                        else
                        {
                            Expense.ExpenseFlowId = flow.Id;
                        }
                    }
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

        public async Task<JsonResult> OnPostGetFlowCategoriesAsync()
        {
            return await this.ProcessAjaxPostRequestAsync(async name =>
            {
                var flow = await _expenseFlowQueries.GetByName(name);
                if (flow == null) return new { flowId = 0, categories = Enumerable.Empty<string>()};
                var categories = await _categoriesQueries.GetFlowCategories(flow.Id);
                return new { flowId = flow.Id, categories = categories.Select(x => x.Name)};
            });
        }

        public async Task<JsonResult> OnPostGetCategoryProductsAsync()
        {
            return await this.ProcessAjaxPostRequestAsync(async name =>
            {
                try
                {
                    var products = await _categoriesQueries.GetProductsByCategoryName(name);
                    return products.Select(x => x.Name);
                }
                catch (ArgumentException)
                {
                    return Enumerable.Empty<string>();
                }
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
                return products.Select(x => x.Name);
            });
        }
    }
}