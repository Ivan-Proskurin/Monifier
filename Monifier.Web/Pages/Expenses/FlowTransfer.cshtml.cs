﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.Web.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Expenses
{
    public class FlowTransferModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IExpenseFlowQueries _expenseFlowQueries;
        private readonly IAccountCommands _accountCommands;

        public FlowTransferModel(IAccountQueries accountQueries,
            IExpenseFlowQueries expenseFlowQueries,
            IAccountCommands accountCommands)
        {
            _accountQueries = accountQueries;
            _expenseFlowQueries = expenseFlowQueries;
            _accountCommands = accountCommands;
        }

        [BindProperty]
        public FlowTransfer Transfer { get; set; }

        public ExpenseFlowModel Flow { get; set; }

        public List<AccountModel> Accounts { get; set; }
        
        public string AvailableBalance { get; private set; }
        
        private async Task LoadModelsAsync(int expenseId)
        {
            Flow = await _expenseFlowQueries.GetById(expenseId);
            Accounts = await _accountQueries.GetAll();
        }

        private async Task<string> GetAvailableBalanceAsync()
        {
            const string unknownBalance = "-";
            if (string.IsNullOrEmpty(Transfer?.AccountFrom)) return unknownBalance;
            var account = await _accountQueries.GetByName(Transfer.AccountFrom);
            return account == null ? unknownBalance : account.Balance.ToMoney();
        }

        public async Task OnGetAsync(int expenseId)
        {
            await LoadModelsAsync(expenseId);
            Transfer = new FlowTransfer
            {
                FlowId = expenseId
            };
            AvailableBalance = "-";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Transfer.ProcessAsync(ModelState,
                async () =>
                {
                    var account = _accountQueries.GetByName(Transfer.AccountFrom);
                    await _accountCommands.TransferToExpenseFlow(Transfer.FlowId,
                        account.Id, Transfer.Amount.ParseMoneyInvariant());
                    return RedirectToPage("./ExpenseFlows");
                },
                async () =>
                {
                    await LoadModelsAsync(Transfer.FlowId);
                    AvailableBalance = await GetAvailableBalanceAsync();
                    return Page();
                },
                async vrList =>
                {
                    var account = await _accountQueries.GetByName(Transfer.AccountFrom); 
                    if (account == null) vrList.Add(new ModelValidationResult("Transfer.AccountFrom", "Нет такого счета"));
                    await Task.CompletedTask;
                }
            );
        }

        public async Task<JsonResult> OnPostAccountBalanceAsync()
        {
            return await this.ProcessAjaxPostRequestAsync(async name =>
            {
                var account = await _accountQueries.GetByName(name);
                return account == null ? "-" : account.Balance.ToMoney();
            });
        }
    }
}