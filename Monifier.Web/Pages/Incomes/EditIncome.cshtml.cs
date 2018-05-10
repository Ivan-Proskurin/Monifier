using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Incomes;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Incomes;
using Monifier.Common.Extensions;
using Monifier.Common.Validation;
using Monifier.Web.Models;
using Monifier.Web.Models.Incomes;

namespace Monifier.Web.Pages.Incomes
{
    public class EditIncomeModel : PageModel
    {
        private readonly IIncomesQueries _incomesQueries;
        private readonly IAccountQueries _accountQueries;
        private readonly IIncomeTypeQueries _incomeTypeQueries;
        private readonly IIncomeItemCommands _incomeItemCommands;

        public EditIncomeModel(
            IIncomesQueries incomesQueries, 
            IAccountQueries accountQueries,
            IIncomeTypeQueries incomeTypeQueries,
            IIncomeItemCommands incomeItemCommands)
        {
            _incomesQueries = incomesQueries;
            _accountQueries = accountQueries;
            _incomeTypeQueries = incomeTypeQueries;
            _incomeItemCommands = incomeItemCommands;
        }

        [BindProperty]
        public EditIncome Income { get; set; }

        public List<AccountModel> Accounts { get; private set; }

        public List<IncomeTypeModel> IncomeTypes { get; private set; }

        private async Task PrepareModelsAsync()
        {
            Accounts = await _accountQueries.GetAll();
            IncomeTypes = await _incomeTypeQueries.GetAll();
        }

        private async Task<EditIncome> GetEditIncomeAsync(int id)
        {
            var incomeItem = await _incomesQueries.GetById(id);
            Income = incomeItem.ToViewModel();
            var account = await _accountQueries.GetById(Income.AccountId);
            Income.Account = account?.Name;
            var incomeType = await _incomeTypeQueries.GetById(Income.IncomeTypeId);
            Income.IncomeType = incomeType?.Name;
            return Income;
        }

        public async Task OnGetAsync(int id)
        {
            await PrepareModelsAsync();
            Income = await GetEditIncomeAsync(id);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Income.ProcessAsync(ModelState, nameof(Income),
                async () =>
                {
                    var model = Income.ToItemModel();
                    await _incomeItemCommands.Update(model);
                    return RedirectToPage("./IncomesTable");
                },
                async () =>
                {
                    await PrepareModelsAsync();
                    return Page();
                },
                async vrList =>
                {
                    if (!Income.Account.IsNullOrEmpty())
                    {
                        var account = await _accountQueries.GetByName(Income.Account);
                        if (account == null)
                            vrList.Add(new ModelValidationResult(nameof(Income.Account), "Нет такого счета"));
                        else
                            Income.AccountId = account.Id;
                    }

                    if (!Income.IncomeType.IsNullOrEmpty())
                    {
                        var incomeType = await _incomeTypeQueries.GetByName(Income.IncomeType);
                        if (incomeType == null)
                            vrList.Add(new ModelValidationResult(nameof(Income.IncomeType), "Нет такой статьи дохода"));
                        else
                            Income.IncomeTypeId = incomeType.Id;
                    }
                }
            );
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            await _incomeItemCommands.Delete(Income.Id, false);
            return RedirectToPage("./IncomesTable");
        }
    }
}