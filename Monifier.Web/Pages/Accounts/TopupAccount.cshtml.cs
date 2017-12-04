using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Accounts;
using Monifier.Web.Models.Validation;

namespace Monifier.Web.Pages.Accounts
{
    public class TopupAccountModel : PageModel
    {
        private readonly IAccountQueries _accountQueries;
        private readonly IAccountCommands _accountCommands;
        private readonly IIncomeTypeQueries _incomeTypeQueries;

        public TopupAccountModel(IAccountQueries accountQueries, 
            IIncomeTypeQueries incomeTypeQueries,
            IAccountCommands accountCommands)
        {
            _accountQueries = accountQueries;
            _accountCommands = accountCommands;
            _incomeTypeQueries = incomeTypeQueries;
        }
        
        [BindProperty]
        public TopupAccount Topup { get; set; }
        
        public List<IncomeTypeModel> IncomeTypes { get; private set; }
        
        public bool SuggestAddIncomeType { get; set; }
        
        public async Task OnGetAsync(int id)
        {
            var account = await _accountQueries.GetById(id);
            Topup = new TopupAccount
            {
                Id = id,
                AccountName = account.Name,
                TopupDate = DateTime.Now.ToStandardString(false)
            };
            IncomeTypes = await _incomeTypeQueries.GetAll();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Topup.ProcessAsync(ModelState,
                async () =>
                {
                    var incomeType = await _incomeTypeQueries.GetByName(Topup.IncomeType);
                    await _accountCommands.Topup(new BusinessLogic.Model.Accounts.TopupAccountModel
                    {
                        AccountId = Topup.Id,
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
                    return Page();
                },
                
                async vrList =>
                {
                    if (string.IsNullOrEmpty(Topup.IncomeType)) return;
                    var incomeType = await _incomeTypeQueries.GetByName(Topup.IncomeType);
                    if (incomeType == null && !Topup.AddNonexistentIncomeType)
                    {
                        vrList.Add(new ModelValidationResult("Topup.IncomeType", "Нет такой статьи"));
                        SuggestAddIncomeType = true;
                        Topup.AddNonexistentIncomeType = true;
                    }
                });
        }
    }
}