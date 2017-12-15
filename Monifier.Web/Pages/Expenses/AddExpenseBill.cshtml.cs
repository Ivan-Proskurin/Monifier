using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Web.Api.Models;
using Monifier.Web.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;
using Monifier.Web.Models.Validation;
using Newtonsoft.Json;

namespace Monifier.Web.Pages.Expenses
{
    public class AddExpenseBillModel : PageModel
    {
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly ICategoriesQueries _categoriesQueries;
        private readonly IProductQueries _productQueries;
        private readonly IExpensesBillQueries _expensesBillQueries;
        private readonly IExpensesBillCommands _expensesBillCommands;
        private readonly ICategoriesCommands _categoriesCommands;
        private readonly IProductCommands _productCommands;

        public AddExpenseBillModel(
            IExpenseFlowQueries expenseFlowQueries,
            ICategoriesQueries categoriesQueries,
            IProductQueries productQueries,
            IExpensesBillQueries expensesBillQueries,
            IExpensesBillCommands expensesBillCommands,
            ICategoriesCommands categoriesCommands,
            IProductCommands productCommands)
        {
            _expenseFlowQueries = expenseFlowQueries;
            _categoriesQueries = categoriesQueries;
            _productQueries = productQueries;
            _expensesBillQueries = expensesBillQueries;
            _expensesBillCommands = expensesBillCommands;
            _categoriesCommands = categoriesCommands;
            _productCommands = productCommands;
        }

        private async Task PrepareModelsAsync(int expenseId)
        {
            ExpenseFlow = await _expenseFlowQueries.GetById(expenseId);
            Categories = await _categoriesQueries.GetFlowCategories(expenseId);
            Products = await _productQueries.GetExpensesFlowProducts(expenseId);
        }

        [BindProperty]
        public AddExpenseBill Good { get; set; }

        public ExpenseFlowModel ExpenseFlow { get; private set; }

        public List<CategoryModel> Categories { get; private set; }

        public List<ProductModel> Products { get; private set; }

        public ExpenseBillModel Bill { get; private set; }

        private async Task PrepareEditBill(int flowId, Func<Task<ExpenseBillModel>> prepareBill)
        {
            await PrepareModelsAsync(flowId);

            Bill = await prepareBill();

            Good = new AddExpenseBill
            {
                ExpenseFlowId = flowId,
                Bill = JsonConvert.SerializeObject(Bill),
            };
        }

        private async Task PrepareInputNewBill(int flowId)
        {
            await PrepareEditBill(flowId, () => Task.FromResult(new ExpenseBillModel()));
        }

        private async Task PrepareEditExistingBill(int flowId, int billId)
        {
            await PrepareEditBill(flowId, async () => await _expensesBillQueries.GetById(billId));
        }

        public async Task OnGetAsync(int expenseId, int? billId = null)
        {
            if (billId != null)
                await PrepareEditExistingBill(expenseId, billId.Value);
            else
                await PrepareInputNewBill(expenseId);
        }

        private async Task<ExpenseItemModel> GetExpenseItem()
        {
            var item = Good.ToItemModel();
            var category = string.IsNullOrEmpty(Good.Category)
                ? null
                : await _categoriesQueries.GetByName(Good.Category);
            item.CategoryId = category?.Id;
            var product = string.IsNullOrEmpty(Good.Product)
                ? null
                : await _productQueries.GetByName(Good.Product);
            item.ProductId = product?.Id;
            return item;
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            await PrepareModelsAsync(Good.ExpenseFlowId);
            Bill = JsonConvert.DeserializeObject<ExpenseBillModel>(Good.Bill);

            return await Good.ProcessAsync(ModelState,
                async () =>
                {
                    Bill.AddItem(await GetExpenseItem());
                    Good.ClearInput();
                    Good.Bill = JsonConvert.SerializeObject(Bill);                    
                    return Page();
                },
                async () => await Task.FromResult(Page()),
                async vrList =>
                {
                    CategoryModel category = null;
                    if (!string.IsNullOrEmpty(Good.Category))
                    {
                        category = await _categoriesQueries.GetFlowCategoryByName(Good.ExpenseFlowId, Good.Category);
                        if (category == null)
                        {
                            if (Good.Category == Good.CategoryToAdd)
                            {
                                category = await _categoriesCommands.CreateNewOrBind(
                                    Good.ExpenseFlowId, Good.Category);
                                Good.CategoryToAdd = null;
                            }
                            else
                            {
                                vrList.Add(new ModelValidationResult("Good.Category", 
                                    "Нет такой категории, добавить ее?"));
                                Good.CategoryToAdd = Good.Category;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(Good.Product))
                    {
                        var product = await _productQueries.GetFlowProductByName(Good.ExpenseFlowId, Good.Product);
                        if (product == null)
                        {
                            if (Good.Product == Good.ProductToAdd && category != null)
                            {
                                await _productCommands.AddProductToCategory(category.Id, Good.Product);
                                Good.ProductToAdd = null;
                            }
                            else
                            {
                                vrList.Add(new ModelValidationResult("Good.Product", 
                                    "Нет такого товара, добавить его?"));
                                Good.ProductToAdd = Good.Product;
                            }
                        }
                    }
                });
        }

        public async Task<IActionResult> OnPostConfirmAsync()
        {
            ModelState.Clear();
            
            Bill = JsonConvert.DeserializeObject<ExpenseBillModel>(Good.Bill);
            if (Bill.Items.Count == 0)
            {
                await PrepareModelsAsync(Good.ExpenseFlowId);
                ModelState.AddModelError(string.Empty, "Добавьте в чек хотя бы один товар");
                return Page();
            }
            Bill.ExpenseFlowId = Good.ExpenseFlowId;

            if (Bill.IsNew)
            {
                await _expensesBillCommands.Create(Bill);
                await PrepareInputNewBill(Bill.ExpenseFlowId);
                return Page();
            }
            else
            {
                await _expensesBillCommands.Update(Bill);
                return RedirectToPage("./ExpenseFlows");
            }
        }

        public async Task<IActionResult> OnPostRemoveLastAsync()
        {
            Bill = JsonConvert.DeserializeObject<ExpenseBillModel>(Good.Bill);
            if (Bill.Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, 
                    @"Невозможно отменить последнюю добавленную позицию, так как счет пуст. 
                      Добавьте хотя бы одну запись, чтобы ее можно было отменить.");
            }
            else
            {
                Bill.RemoveLastAddedItem();
                Good.Bill = JsonConvert.SerializeObject(Bill);
                ModelState.Clear();
            }
            await PrepareModelsAsync(Good.ExpenseFlowId);
            return Page();
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