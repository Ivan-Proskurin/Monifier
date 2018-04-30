using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Contract.Expenses;
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
    public class AddExpenseBillModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly ICategoriesQueries _categoriesQueries;
        private readonly IProductQueries _productQueries;
        private readonly IExpensesBillQueries _expensesBillQueries;
        private readonly IExpensesBillCommands _expensesBillCommands;
        private readonly ICategoriesCommands _categoriesCommands;
        private readonly IProductCommands _productCommands;
        private readonly ITimeService _timeService;

        public AddExpenseBillModel(
            IAccountQueries accountQueries,
            IExpenseFlowQueries expenseFlowQueries,
            ICategoriesQueries categoriesQueries,
            IProductQueries productQueries,
            IExpensesBillQueries expensesBillQueries,
            IExpensesBillCommands expensesBillCommands,
            ICategoriesCommands categoriesCommands,
            IProductCommands productCommands,
            ITimeService timeService)
        {
            _accountQueries = accountQueries;
            _expenseFlowQueries = expenseFlowQueries;
            _categoriesQueries = categoriesQueries;
            _productQueries = productQueries;
            _expensesBillQueries = expensesBillQueries;
            _expensesBillCommands = expensesBillCommands;
            _categoriesCommands = categoriesCommands;
            _productCommands = productCommands;
            _timeService = timeService;
        }

        private async Task PrepareModelsAsync(int flowId)
        {
            Accounts = await _accountQueries.GetAll();
            Flows = await _expenseFlowQueries.GetAll();
            if (flowId != 0)
            {
                Categories = await _categoriesQueries.GetFlowCategories(flowId);
                Products = await _productQueries.GetExpensesFlowProducts(flowId);
            }
            else
            {
                Categories = new List<CategoryModel>();
                Products = new List<ProductModel>();
            }
        }

        [BindProperty]
        public AddExpenseBill Good { get; set; }
             
        public List<AccountModel> Accounts { get; private set; }

        public List<CategoryModel> Categories { get; private set; }

        public List<ProductModel> Products { get; private set; }

        public List<ExpenseFlowModel> Flows { get; private set; }

        public ExpenseBillModel Bill { get; private set; }

        private async Task PrepareEditBill(int flowId, Func<Task<ExpenseBillModel>> prepareBill, string returnUrl)
        {
            await PrepareModelsAsync(flowId);

            Bill = await prepareBill();

            Good = new AddExpenseBill
            {
                FlowId = flowId,
                Bill = JsonConvert.SerializeObject(Bill),
                FlowName = Flows.FirstOrDefault(x => x.Id == flowId)?.Name,
                ReturnUrl = returnUrl,
                ReturnPage = returnUrl.Replace("/Expenses/", "./"),
                IsCorrection = Bill.IsCorection,
            };
            if (Categories.Count == 1)
            {
                Good.Category = Categories.First().Name;
            }
        }

        private async Task PrepareInputNewBill(int flowId, string returnUrl)
        {
            await PrepareEditBill(flowId, () => Task.FromResult(new ExpenseBillModel
            {
                ExpenseFlowId = flowId,
                DateTime = _timeService.ClientLocalNow.ToMinutes()
            }), returnUrl);
            Good.Account = Accounts.GetDefaultAccount()?.Name;
        }

        private async Task PrepareEditExistingBill(int flowId, int billId, string returnUrl)
        {
            await PrepareEditBill(flowId, async () => await _expensesBillQueries.GetById(billId), returnUrl);
            Good.Account = Accounts.FirstOrDefault(x => x.Id == Bill.AccountId)?.Name;
        }

        public async Task OnGetAsync(int flowId, int? billId = null, string returnUrl = "/Expenses/ExpenseFlows")
        {
            if (billId != null)
                await PrepareEditExistingBill(flowId, billId.Value, returnUrl);
            else
                await PrepareInputNewBill(flowId, returnUrl);
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
            return await Good.ProcessAsync(ModelState, nameof(Good),
                async () =>
                {
                    await PrepareModelsAsync(Good.FlowId);
                    Bill = JsonConvert.DeserializeObject<ExpenseBillModel>(Good.Bill);
                    Bill.AddItem(await GetExpenseItem());
                    Good.ClearInput();
                    Good.Bill = JsonConvert.SerializeObject(Bill);
                    return Page();
                },
                async () =>
                {
                    await PrepareModelsAsync(Good.FlowId);
                    Bill = JsonConvert.DeserializeObject<ExpenseBillModel>(Good.Bill);
                    return Page();
                },
                async vrList =>
                {
                    if (Good.Account.IsNullOrEmpty())
                        vrList.Add(new ModelValidationResult(nameof(Good.Account), "Укажите счет"));
                    else
                    {
                        var account = await _accountQueries.GetByName(Good.Account);
                        if (account == null)
                            vrList.Add(new ModelValidationResult(nameof(Good.Account), "Нет такого счета"));
                    }

                    if (Good.FlowName.IsNullOrEmpty())
                    {
                        vrList.Add(new ModelValidationResult(nameof(Good.FlowName), "Укажите статью расходов"));
                        return;
                    }

                    var flowId = await _expenseFlowQueries.GetIdByName(Good.FlowName);
                    if (flowId == null)
                    {
                        vrList.Add(new ModelValidationResult(nameof(Good.FlowName), "Нет такой статьи расходов"));
                        return;
                    }

                    Good.FlowId = flowId.Value;

                    CategoryModel category = null;
                    if (!Good.Category.IsNullOrEmpty())
                    {
                        category = await _categoriesQueries.GetFlowCategoryByName(Good.FlowId, Good.Category);
                        if (category == null)
                        {
                            if (Good.Category == Good.CategoryToAdd)
                            {
                                category = await _categoriesCommands.CreateNewOrBind(Good.FlowId, Good.Category);
                                Good.CategoryToAdd = null;
                            }
                            else
                            {
                                vrList.Add(new ModelValidationResult(nameof(Good.Category),
                                    "Нет такой категории, добавить ее?"));
                                Good.CategoryToAdd = Good.Category;
                            }
                        }
                    }
                    if (!Good.Product.IsNullOrEmpty())
                    {
                        var product = await _productQueries.GetFlowProductByName(Good.FlowId, Good.Product);
                        if (product == null)
                        {
                            if (Good.Product == Good.ProductToAdd && category != null)
                            {
                                await _productCommands.AddProductToCategory(category.Id, Good.Product);
                                Good.ProductToAdd = null;
                            }
                            else
                            {
                                vrList.Add(new ModelValidationResult(nameof(Good.Product),
                                    "Нет такого товара, добавить его?"));
                                Good.ProductToAdd = Good.Product;
                            }
                        }
                    }
                });
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
            await PrepareModelsAsync(Good.FlowId);
            return Page();
        }
        
        public async Task<JsonResult> OnPostConfirmAsync()
        {
            return await this.ProcessAjaxPostRequestAsync(async request =>
            {
                try
                {
                    var args = request.FromJson<ConfirmBillArgs>();
                    if (args.Account.IsNullOrEmpty()) return AjaxResponse.ErrorResponse("Выберите счет");
                    var account = await _accountQueries.GetByName(args.Account);
                    if (account == null) return AjaxResponse.ErrorResponse($"Нет счета с именем \"{args.Account}\"");
                    var bill = args.Bill.FromJson<ExpenseBillModel>();
                    if (bill.Items.Count == 0) return AjaxResponse.ErrorResponse("Добавьте в чек хотя бы один товар");
                    bill.ExpenseFlowId = Good.FlowId;
                    bill.AccountId = account.Id;
                    await _expensesBillCommands.Save(bill);
                    return AjaxResponse.SuccessResponse();
                }
                catch (Exception exc)
                {
                    return AjaxResponse.ErrorResponse(exc.Message);
                }
            });
        }

        public async Task<JsonResult> OnPostGetFlowCategoriesAsync()
        {
            return await this.ProcessAjaxPostRequestAsync(async name =>
            {
                var flow = await _expenseFlowQueries.GetByName(name);
                if (flow == null) return new { flowId = 0, categories = Enumerable.Empty<string>() };
                var categories = await _categoriesQueries.GetFlowCategories(flow.Id);
                return new { flowId = flow.Id, categories = categories.Select(x => x.Name) };
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
            });
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