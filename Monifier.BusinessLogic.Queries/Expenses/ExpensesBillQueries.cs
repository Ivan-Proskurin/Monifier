using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class ExpensesBillQueries : IExpensesBillQueries
    {
        private readonly IEntityRepository _repository;

        public ExpensesBillQueries(IEntityRepository repository)
        {
            _repository = repository;
        }

        public Task<List<ExpenseBillModel>> GetAll(bool includeDeleted = false)
        {
            throw new NotSupportedException();
        }

        public async Task<ExpenseBillModel> GetById(int id)
        {
            var query = _repository.GetQuery<ExpenseBill>();
            var bill = await query.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            var model = new ExpenseBillModel
            {
                Id = bill.Id,
                AccountId = bill.AccountId,
                ExpenseFlowId = bill.ExpenseFlowId,
                DateTime = bill.DateTime,
                Cost = bill.SumPrice,
                OwnerId = bill.OwnerId ?? 0,
                IsCorection = bill.IsCorrection,
                Items = bill.Items.Select(x => new ExpenseItemModel
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    Comment = x.Comment,
                    Cost = x.Price,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                }).ToList()
            };
            await FullfillItemsFields(model).ConfigureAwait(false);
            return model;
        }

        private async Task FullfillItemsFields(ExpenseBillModel bill)
        {
            foreach (var item in bill.Items)
            {
                if (item.ProductId != null)
                {
                    var product = await _repository.LoadAsync<Product>(item.ProductId.Value);
                    item.Product = product.Name;
                }
                if (item.CategoryId != null)
                {
                    var category = await _repository.LoadAsync<Category>(item.CategoryId.Value);
                    item.Category = category.Name;
                }
            }
        }

        public Task<ExpenseBillModel> GetByName(string name, bool includeDeleted = false)
        {
            throw new NotSupportedException();
        }
    }
}
