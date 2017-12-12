using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;
using Monifier.Web.Models.Validation;
using Newtonsoft.Json;

namespace Monifier.Web.Pages.Expenses
{
    public class AddExpenseBillModel : PageModel
    {
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IAccountQueries _accountQueries;
        private readonly ICategoriesQueries _categoriesQueries;
        private readonly IProductQueries _productQueries;
        private readonly IExpensesBillCommands _expensesBillCommands;

        public AddExpenseBillModel(IExpenseFlowQueries expenseFlowQueries,
            IAccountQueries accountQueries,
            ICategoriesQueries categoriesQueries,
            IProductQueries productQueries,
            IExpensesBillCommands expensesBillCommands)
        {
            _expenseFlowQueries = expenseFlowQueries;
            _accountQueries = accountQueries;
            _categoriesQueries = categoriesQueries;
            _productQueries = productQueries;
            _expensesBillCommands = expensesBillCommands;
        }

        private async Task PrepareModelsAsync(int expenseId)
        {
            ExpenseFlow = await _expenseFlowQueries.GetById(expenseId);
            Accounts = await _accountQueries.GetAll();
            Categories = await _categoriesQueries.GetFlowCategories(expenseId);
            Products = await _productQueries.GetExpensesFlowProducts(expenseId);
        }

        [BindProperty]
        public AddExpenseBill Good { get; set; }

        public ExpenseFlowModel ExpenseFlow { get; private set; }

        public List<AccountModel> Accounts { get; set; }

        public List<CategoryModel> Categories { get; private set; }

        public List<ProductModel> Products { get; private set; }

        public ExpenseBillModel Bill { get; private set; }

        public async Task OnGetAsync(int expenseId)
        {
            await PrepareModelsAsync(expenseId);

            Bill = new ExpenseBillModel();

            Good = new AddExpenseBill
            {
                ExpenseFlowId = expenseId,
                DateTime = DateTime.Today.ToStandardString(),
                Bill = JsonConvert.SerializeObject(Bill),
            };
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

            return await Good.TryProcessAsync<ArgumentException>(ModelState,
                async () =>
                {
                    Bill.AddItem(await GetExpenseItem());
                    Good.Bill = JsonConvert.SerializeObject(Bill);
                    return Page();
                },
                async () => Page(),
                async vrList =>
                {
                    if (!string.IsNullOrEmpty(Good.Category))
                    {
                        var category =
                            await _categoriesQueries.GetFlowCategoryByName(Good.ExpenseFlowId, Good.Category);
                        if (category == null)
                            vrList.Add(new ModelValidationResult("Good.Category", "Нет такой категории товаров"));
                    }
                    if (!string.IsNullOrEmpty(Good.Product))
                    {
                        var product = await _productQueries.GetFlowProductByName(Good.ExpenseFlowId, Good.Product);
                        if (product == null)
                            vrList.Add(new ModelValidationResult("Good.Product", "Нет такого товара"));
                    }
                    if (!string.IsNullOrEmpty(Good.Account))
                    {
                        var account = await _accountQueries.GetByName(Good.Account);
                        if (account == null)
                            vrList.Add(new ModelValidationResult("Good.Account", "Нет такого счета"));
                    }
                });
        }

        public async Task<IActionResult> OnPostConfirmAsync()
        {
            Bill = JsonConvert.DeserializeObject<ExpenseBillModel>(Good.Bill);

            if (string.IsNullOrEmpty(Good.Account))
            {
                ModelState.AddModelError("Good.Account", "Укажите счет списания");
            }
            
            if (!string.IsNullOrEmpty(Good.Account))
            {
                var account = await _accountQueries.GetByName(Good.Account);
                if (account == null)
                {
                    ModelState.AddModelError("Good.Account", "Нет такого счета");
                    return Page();
                }
                else
                {
                    Bill.AccountId = account.Id;
                }
            }
            
            if (Bill.Items.Count == 0)
            {
                await PrepareModelsAsync(Good.ExpenseFlowId);
                ModelState.AddModelError(string.Empty, "Добавьте в чек хотя бы один товар");
                return Page();
            }
            Bill.ExpenseFlowId = Good.ExpenseFlowId;
            await _expensesBillCommands.Create(Bill);
            return RedirectToPage("./ExpenseFlows");
        }
    }
}