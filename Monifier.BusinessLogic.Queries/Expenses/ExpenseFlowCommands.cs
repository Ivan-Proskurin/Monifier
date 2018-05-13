using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class ExpenseFlowCommands : IExpenseFlowCommands
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;
        private readonly IExpensesBillCommands _expensesBillCommands;

        public ExpenseFlowCommands(
            IEntityRepository repository, 
            ICurrentSession currentSession,
            IExpensesBillCommands expensesBillCommands)
        {
            _repository = repository;
            _currentSession = currentSession;
            _expensesBillCommands = expensesBillCommands;
        }
        
        public async Task<ExpenseFlowModel> Update(ExpenseFlowModel model)
        {
            var other = await _repository.FindByNameAsync<ExpenseFlow>(_currentSession.UserId, model.Name);
            if (other != null)
                if (other.Id != model.Id)
                    throw new ArgumentException("Категория расходов с таким названием уже есть");
                else
                    _repository.Detach(other);

            var flow = new ExpenseFlow
            {
                Name = model.Name,
                Balance = model.Balance,
                DateCreated = model.DateCreated,
                Number = model.Number,
                IsDeleted = false,
                OwnerId = _currentSession.UserId
            };
            if (model.Id < 0)
            {
                flow.Version = 1;
                _repository.Create(flow);
            }
            else
            {
                flow.Id = model.Id;
                flow.Version = model.Version + 1;
                _repository.Update(flow);
                if (model.Categories != null)
                    UpdateFlowCategories(flow.Id, model.Categories);
            }
            
            await _repository.SaveChangesAsync().ConfigureAwait(false);

            model.Id = flow.Id;
            model.Version = flow.Version;
            return model;
        }

        private void UpdateFlowCategories(int flowId, IEnumerable<int> categoriesId)
        {
            foreach (var flowcat in _repository.GetQuery<ExpensesFlowProductCategory>()
                .Where(x => x.ExpensesFlowId == flowId))
            {
                _repository.Delete(flowcat);
            }
            foreach (var catId in categoriesId)
            {
                var flowcat = new ExpensesFlowProductCategory
                {
                    ExpensesFlowId = flowId,
                    CategoryId = catId
                };
                _repository.Create(flowcat);
            }
        } 

        public async Task Delete(int id, bool onlyMark = true)
        {
            var flow = await _repository.LoadAsync<ExpenseFlow>(id).ConfigureAwait(false);
            if (flow == null)
                throw new ArgumentException($"Нет счета с Id = {id}");
            
            if (onlyMark)
            {
                flow.IsDeleted = true;
            }
            else
            {
                _repository.Delete(flow);
            }
            await _repository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task AddExpense(ExpenseFlowExpense expense)
        {
            if (expense.Account.IsNullOrEmpty())
                throw new ArgumentException("Необходимо указать счет");
            
            var account = await _repository.FindByNameAsync<Account>(_currentSession.UserId, expense.Account);
            if (account == null)
                throw new ArgumentException($"Нет счета с именем \"{expense.Account}\"");
            
            var flow = await _repository.LoadAsync<ExpenseFlow>(expense.ExpenseFlowId);
            if (flow == null)
                throw new ArgumentException($"Нет статьи расходов с идентификатором Id = {expense.ExpenseFlowId}");
            
            if (string.IsNullOrEmpty(expense.Category) && string.IsNullOrEmpty(expense.Product))
                throw new ArgumentException("Необходимо ввести хотя бы категорию или продукт");

            Category category = null;
            if (!string.IsNullOrEmpty(expense.Category))
            {
                category = await _repository.FindByNameAsync<Category>(_currentSession.UserId, expense.Category);
                if (category == null)
                    throw new ArgumentException($"Нет категории продуктов с именем \"{expense.Category}\"");
            }

            Product product = null;
            if (!string.IsNullOrEmpty(expense.Product))
            {
                product = await _repository.FindByNameAsync<Product>(_currentSession.UserId, expense.Product);
                if (product == null)
                    throw new ArgumentException($"Нет товара с именем \"{expense.Product}\"");
            }
            
            if (product == null && category == null)
                throw new ArgumentException("Были введены или категория или продукт, но ни один из них не найден");
            
            var billModel = new ExpenseBillModel
            {
                AccountId = account.Id,
                ExpenseFlowId = expense.ExpenseFlowId,
                DateTime = expense.DateCreated,
                Cost = expense.Cost,
                OwnerId = _currentSession.UserId,
                Items = new List<ExpenseItemModel>
                {
                    new ExpenseItemModel
                    {
                        CategoryId = category?.Id,
                        ProductId = product?.Id,
                        Comment = null,
                        Quantity = null,
                        Cost = expense.Cost,
                    }
                }
            };

            await _expensesBillCommands.Create(billModel, expense.Correcting).ConfigureAwait(false);
        }
    }
}