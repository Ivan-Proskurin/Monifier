using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Contract.Processing;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Processing
{
    public class CreditCardProcessing : ICreditCardProcessing
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;
        private readonly ITimeService _timeService;
        private readonly IExpensesBillCommands _expensesBillCommands;

        public CreditCardProcessing(IEntityRepository repository, 
            ICurrentSession currentSession,
            ITimeService timeService,
            IExpensesBillCommands expensesBillCommands)
        {
            _repository = repository;
            _currentSession = currentSession;
            _timeService = timeService;
            _expensesBillCommands = expensesBillCommands;
        }

        public async Task<int> ProcessReducingBalanceAsCreditFees(Account account, decimal reduceAmount)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
            if (account.AccountType != AccountType.CreditCard)
                throw new InvalidOperationException("Счет должен быть кредитной картой");

            var category = await _repository.FindByNameAsync<Category>(
                _currentSession.UserId, "Кредиты").ConfigureAwait(false);
            if (category == null)
            {
                category = new Category
                {
                    OwnerId = _currentSession.UserId,
                    Name = "Кредиты",
                };
                _repository.Create(category);
                await _repository.SaveChangesAsync().ConfigureAwait(false);
            }

            var mustSave = false;
            var product = await _repository.FindByNameAsync<Product>(_currentSession.UserId, account.Name).ConfigureAwait(false);
            if (product == null)
            {
                product = new Product
                {
                    OwnerId = _currentSession.UserId,
                    Name = account.Name,
                    CategoryId = category.Id
                };
                _repository.Create(product);
                mustSave = true;
            }

            var flow = await _repository.FindByNameAsync<ExpenseFlow>(_currentSession.UserId, "Кредиты").ConfigureAwait(false);
            if (flow == null)
            {
                flow = new ExpenseFlow
                {
                    OwnerId = _currentSession.UserId,
                    Balance = 0,
                    DateCreated = _timeService.ClientLocalNow,
                    Name = "Кредиты",
                    Number = 1,
                    Version = 1
                };
                _repository.Create(flow);
                mustSave = true;
            }

            if (mustSave)
                await _repository.SaveChangesAsync().ConfigureAwait(false);

            var bill = new ExpenseBillModel
            {
                DateTime = _timeService.ClientLocalNow,
                OwnerId = _currentSession.UserId,
                AccountId = account.Id,
                ExpenseFlowId = flow.Id,
                IsCorection = true,
            };
            var item = new ExpenseItemModel
            {
                CategoryId = category.Id,
                ProductId = product.Id,
                Cost = reduceAmount
            };
            bill.AddItem(item);

            return await _expensesBillCommands.Create(bill, true).ConfigureAwait(false);
        }
    }
}