﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Base;
using Monifier.Common.Extensions;
using Monifier.Web.Models;
using Monifier.Web.Models.Accounts;

namespace Monifier.Web.Pages.Accounts
{
    [Authorize]
    public class CreateAccountModel : PageModel
    {
        private readonly IAccountCommands _accountCommands;
        private readonly IAccountQueries _accountQueries;
        private readonly ITimeProvider _timeProvider;

        public CreateAccountModel(
            IAccountCommands accountCommands, 
            IAccountQueries accountQueries,
            ITimeProvider timeProvider)
        {
            _accountCommands = accountCommands;
            _accountQueries = accountQueries;
            _timeProvider = timeProvider;
        }
        
        [BindProperty]
        public EditAccount Account { get; set; }

        public async Task OnGetAsync()
        {
            Account = new EditAccount
            {
                Id = -1,
                Number = await _accountQueries.GetNextNumber(),
                CreationDate = _timeProvider.ClientLocalNow.ToStandardString(false)
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Account.ProcessAsync(ModelState, nameof(Account),
                async () =>
                {
                    var model = new AccountModel {Id = -1};
                    Account.ToAccountModel(model);
                    await _accountCommands.Update(model);
                    return RedirectToPage("./AccountsList");
                },

                async () => await Task.FromResult(Page())
            );
        }
    }
}