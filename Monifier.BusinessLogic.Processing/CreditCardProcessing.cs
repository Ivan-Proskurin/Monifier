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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentSession _currentSession;
        private readonly ITimeService _timeService;
        private readonly IExpensesBillCommands _expensesBillCommands;

        public CreditCardProcessing(IUnitOfWork unitOfWork, 
            ICurrentSession currentSession,
            ITimeService timeService,
            IExpensesBillCommands expensesBillCommands)
        {
            _unitOfWork = unitOfWork;
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

            var productCommands = _unitOfWork.GetCommandRepository<Product>();
            var productQueries = _unitOfWork.GetNamedModelQueryRepository<Product>();
            var categoriesCommands = _unitOfWork.GetCommandRepository<Category>();
            var categoriesQueries = _unitOfWork.GetNamedModelQueryRepository<Category>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();
            var flowQueries = _unitOfWork.GetNamedModelQueryRepository<ExpenseFlow>();

            var category = await categoriesQueries.GetByName(_currentSession.UserId, "Кредиты");
            if (category == null)
            {
                category = new Category
                {
                    OwnerId = _currentSession.UserId,
                    Name = "Кредиты",
                };
                categoriesCommands.Create(category);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }

            var mustSave = false;
            var product = await productQueries.GetByName(_currentSession.UserId, account.Name);
            if (product == null)
            {
                product = new Product
                {
                    OwnerId = _currentSession.UserId,
                    Name = account.Name,
                    CategoryId = category.Id
                };
                productCommands.Create(product);
                mustSave = true;
            }

            var flow = await flowQueries.GetByName(_currentSession.UserId, "Кредиты");
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
                flowCommands.Create(flow);
                mustSave = true;
            }

            if (mustSave)
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

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