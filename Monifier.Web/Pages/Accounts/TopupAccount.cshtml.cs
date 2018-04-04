using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Contract.Inventorization;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Accounts;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Accounts
{
    [Authorize]
    public class TopupAccountModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IAccountCommands _accountCommands;
        private readonly IInventorizationQueries _inventorizationQueries;
        private readonly IIncomeTypeQueries _incomeTypeQueries;

        public TopupAccountModel(IAccountQueries accountQueries, 
            IIncomeTypeQueries incomeTypeQueries,
            IAccountCommands accountCommands,
            IInventorizationQueries inventorizationQueries)
        {
            _accountQueries = accountQueries;
            _accountCommands = accountCommands;
            _inventorizationQueries = inventorizationQueries;
            _incomeTypeQueries = incomeTypeQueries;
        }
        
        [BindProperty]
        public TopupAccount Topup { get; set; }
        
        public List<IncomeTypeModel> IncomeTypes { get; private set; }
        
        public List<AccountModel> Accounts { get; private set; }
        
        public bool SuggestAddIncomeType { get; set; }

        public async Task OnGetAsync(int id, bool correcting = false)
        {
            var account = correcting ? null : await _accountQueries.GetById(id);
            Topup = new TopupAccount
            {
                Id = id,
                Correcting = correcting,
                AccountName = account?.Name,
                TopupDate = DateTime.Now.ToStandardString(false)
            };
            IncomeTypes = await _incomeTypeQueries.GetAll();
            if (correcting)
            {
                Accounts = await _accountQueries.GetAll();
                var balanceState = await _inventorizationQueries.GetBalanceState();
                Topup.Amount = balanceState.Balance.ToStandardString();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Topup.ProcessAsync(ModelState, nameof(Topup),
                async () =>
                {
                    var incomeType = await _incomeTypeQueries.GetByName(Topup.IncomeType);
                    var accountId = Topup.Correcting ? (await _accountQueries.GetByName(Topup.AccountName)).Id : Topup.Id;
                    await _accountCommands.Topup(new BusinessLogic.Model.Accounts.TopupAccountModel
                    {
                        Correcting = Topup.Correcting,
                        AccountId = accountId,
                        IncomeTypeId = incomeType?.Id,
                        AddIncomeTypeName = incomeType == null ? Topup.IncomeType : null,
                        TopupDate = Topup.TopupDate.ParseDtFromStandardString(),
                        Amount = Topup.Amount.ParseMoneyInvariant()
                    });
                    return RedirectToPage("./AccountsList");
                },
                
                async () =>
                {
                    IncomeTypes = await _incomeTypeQueries.GetAll();
                    if (Topup.Correcting)
                    {
                        Accounts = await _accountQueries.GetAll();
                    }
                    return Page();
                },
                
                async vrList =>
                {
                    if (Topup.Correcting)
                    {
                        var account = await _accountQueries.GetByName(Topup.AccountName);
                        if (account == null)
                        {
                            vrList.Add(new ModelValidationResult(nameof(Topup.AccountName), "Нет такого счета"));
                        }
                    }
                    
                    var incomeType = await _incomeTypeQueries.GetByName(Topup.IncomeType);
                    if (incomeType == null && !Topup.AddNonexistentIncomeType)
                    {
                        vrList.Add(new ModelValidationResult(nameof(Topup.IncomeType), "Нет такой статьи"));
                        SuggestAddIncomeType = true;
                        Topup.AddNonexistentIncomeType = true;
                    }
                });
        }
    }
}