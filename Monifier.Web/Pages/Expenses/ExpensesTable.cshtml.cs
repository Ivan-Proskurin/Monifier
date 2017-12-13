﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Settings;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.Web.Models;
using Monifier.Web.Models.Expenses;

namespace Monifier.Web.Pages.Expenses
{
    public class ExpensesTableModel : PageModel
    {
        private readonly IExpensesQueries _expensesQueries;
        private readonly IUserSettings _userSettings;

        public ExpensesTableModel(IExpensesQueries expensesQueries, IUserSettings userSettings)
        {
            _expensesQueries = expensesQueries;
            _userSettings = userSettings;
        }
        
        [BindProperty]
        public ExpensesTableFilter Filter { get; set; }
        
        public ExpensesListModel Expenses { get; private set; }
        
        public bool IsDataValid { get; private set; }

        private async Task<ExpensesListModel> LoadExpensesAsync(int pageNumber = 1)
        {
            if (Filter.DateFromAsDateTime != null && Filter.DateToAsDateTime != null)
            {
                var expenses = await _expensesQueries.GetExpensesByDay(
                    Filter.DateFromAsDateTime.Value, Filter.DateToAsDateTime.Value, new PaginationArgs
                    {
                        IncludeDeleted = false,
                        ItemsPerPage = _userSettings.ItemsPerPage,
                        PageNumber = pageNumber
                    });
                IsDataValid = true;
                return expenses;
            }
            else
            {
                IsDataValid = false;
            }
            return null;
        } 

        public async Task OnGetAsync(string dateFrom, string dateTo, int pageNumber = 1)
        {
            if (string.IsNullOrEmpty(dateFrom) || string.IsNullOrEmpty(dateTo))
                Filter = ExpensesTableFilter.CurrentWeek();
            else
            {
                Filter = new ExpensesTableFilter(dateFrom, dateTo);
                foreach (var error in Filter.Validate())
                {
                    ModelState.AddModelError(error.PropertyName, error.Message);
                }
            }
            if (ModelState.IsValid)
                Expenses = await LoadExpensesAsync(pageNumber);
            else
                IsDataValid = false;
        }

        public async Task<IActionResult> OnPostRefreshAsync()
        {
            return await Filter.ProcessAsync(ModelState,
                async () =>
                {
                    Expenses = await LoadExpensesAsync();
                    return Page();
                },
                async () =>
                {
                    IsDataValid = false;
                    return await Task.FromResult(Page());
                });
        }
    }
}