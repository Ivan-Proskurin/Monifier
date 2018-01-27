using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IUnitOfWork _unitOfWork;

        public ExpenseFlowCommands(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<ExpenseFlowModel> Update(ExpenseFlowModel model)
        {
            var queries = _unitOfWork.GetNamedModelQueryRepository<ExpenseFlow>();
            var commands = _unitOfWork.GetCommandRepository<ExpenseFlow>();

            var other = await queries.GetByName(model.Name);
            if (other != null)
                if (other.Id != model.Id)
                    throw new ArgumentException("Категория расходов с таким названием уже есть");
                else
                    commands.Detach(other);

            var flow = new ExpenseFlow
            {
                Name = model.Name,
                Balance = model.Balance,
                DateCreated = model.DateCreated,
                Number = model.Number,
                IsDeleted = false
            };
            if (model.Id < 0)
            {
                flow.Version = 1;
                commands.Create(flow);
            }
            else
            {
                flow.Id = model.Id;
                flow.Version = model.Version + 1;
                commands.Update(flow);
                if (model.Categories != null)
                    UpdateFlowCategories(flow.Id, model.Categories);
            }
            
            await _unitOfWork.SaveChangesAsync();

            model.Id = flow.Id;
            model.Version = flow.Version;
            return model;
        }

        private void UpdateFlowCategories(int flowId, IEnumerable<int> categoriesId)
        {
            var flowcatsQueries = _unitOfWork.GetQueryRepository<ExpensesFlowProductCategory>();
            var flowcatsCommands = _unitOfWork.GetCommandRepository<ExpensesFlowProductCategory>();
            foreach (var flowcat in flowcatsQueries.Query.Where(x => x.ExpensesFlowId == flowId))
            {
                flowcatsCommands.Delete(flowcat);
            }
            foreach (var catId in categoriesId)
            {
                var flowcat = new ExpensesFlowProductCategory
                {
                    ExpensesFlowId = flowId,
                    CategoryId = catId
                };
                flowcatsCommands.Create(flowcat);
            }
        } 

        public async Task Delete(int id, bool onlyMark = true)
        {
            var expenseRepo = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flow = await expenseRepo.GetById(id);
            if (flow == null)
                throw new ArgumentException($"Нет счета с Id = {id}");
            
            if (onlyMark)
            {
                flow.IsDeleted = true;
            }
            else
            {
                var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();
                flowCommands.Delete(flow);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AddExpense(ExpenseFlowExpense expense)
        {
            if (expense.Account.IsNullOrEmpty())
                throw new ArgumentException("Необходимо указать счет");
            
            var account = await _unitOfWork.GetNamedModelQueryRepository<Account>().GetByName(expense.Account);
            if (account == null)
                throw new ArgumentException($"Нет счета с именем \"{expense.Account}\"");
            
            var flow = await _unitOfWork.GetQueryRepository<ExpenseFlow>().GetById(expense.ExpenseFlowId);
            if (flow == null)
                throw new ArgumentException($"Нет статьи расходов с идентификатором Id = {expense.ExpenseFlowId}");
            
            if (string.IsNullOrEmpty(expense.Category) && string.IsNullOrEmpty(expense.Product))
                throw new ArgumentException("Необходимо ввести хотя бы категорию или продукт");

            Category category = null;
            if (!string.IsNullOrEmpty(expense.Category))
            {
                var repo = _unitOfWork.GetNamedModelQueryRepository<Category>();
                category = await repo.GetByName(expense.Category);
                if (category == null)
                    throw new ArgumentException($"Нет категории продуктов с именем \"{expense.Category}\"");
            }

            Product product = null;
            if (!string.IsNullOrEmpty(expense.Product))
            {
                product = await _unitOfWork.GetNamedModelQueryRepository<Product>().GetByName(expense.Product);
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

            await billModel.Create(_unitOfWork, expense.Correcting);
        }
    }
}