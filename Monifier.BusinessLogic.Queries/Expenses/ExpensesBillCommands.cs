using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Expenses
{
    public class ExpensesBillCommands : IExpensesBillCommands
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExpensesBillCommands(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task Delete(int id, bool onlyMark = true)
        {
            throw new NotImplementedException();
        }

        public async Task<ExpenseBillModel> Update(ExpenseBillModel model)
        {
            var commands = _unitOfWork.GetCommandRepository<ExpenseItem>();
            foreach (var item in model.Items)
            {
                if (item.Id > 0 && !item.IsModified && !item.IsDeleted) continue;
                if (item.Id < 0 || item.IsModified)
                {
                    var itemModel = new ExpenseItem
                    {
                        Id = item.Id,
                        BillId = model.Id,
                        CategoryId = item.CategoryId,
                        ProductId = item.ProductId,
                        DateTime = item.DateTime,
                        Price = item.Cost,
                        Quantity = item.Quantity,
                        Comment = item.Comment
                    };
                    if (item.Id < 0)
                        commands.Create(itemModel);
                    else
                        commands.Update(itemModel);
                }
                else if (item.IsDeleted)
                {
                    var itemModel = await _unitOfWork.GetQueryRepository<ExpenseItem>().GetById(item.Id);
                    if (itemModel == null) continue;
                    commands.Delete(itemModel);
                }
            }

            var billCommands = _unitOfWork.GetCommandRepository<ExpenseBill>();
            billCommands.Update(new ExpenseBill
            {
                Id = model.Id,
                DateTime = model.DateTime,
                SumPrice = model.Cost
            });

            await _unitOfWork.SaveChangesAsync();

            return model;
        }

        public async Task Create(ExpenseBillModel model)
        {
            var billRepo = _unitOfWork.GetCommandRepository<ExpenseBill>();

            var bill = new ExpenseBill
            {
                Id = -1,
                DateTime = model.DateTime,
                SumPrice = model.Cost
            };

            billRepo.Create(bill);

            var itemsRepo = _unitOfWork.GetCommandRepository<ExpenseItem>();

            foreach (var item in model.Items)
            {
                itemsRepo.Create(new ExpenseItem
                {
                    Id = -1,
                    Bill = bill,
                    CategoryId = item.CategoryId,
                    ProductId = item.ProductId,
                    DateTime = item.DateTime,
                    Price = item.Cost,
                    Quantity = item.Quantity,
                    Comment = item.Comment
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
