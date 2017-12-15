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

        public async Task Delete(int id, bool onlyMark = true)
        {
            if (onlyMark)
                throw new ArgumentException("Операция не поддерживается", nameof(onlyMark));

            var billQueries = _unitOfWork.GetQueryRepository<ExpenseBill>();
            var bill = await billQueries.GetById(id);
            if (bill == null)
                throw new ArgumentException($"Нет счета с Id = {id}");

            var billCommands = _unitOfWork.GetCommandRepository<ExpenseBill>();
            var flowQueries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();

            var flow = await flowQueries.GetById(bill.ExpenseFlowId);
            flow.Balance += bill.SumPrice;
            flowCommands.Update(flow);
            
            billCommands.Delete(bill);
            
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ExpenseBillModel> Update(ExpenseBillModel model)
        {
            var commands = _unitOfWork.GetCommandRepository<ExpenseItem>();
            foreach (var item in model.Items)
            {
                if (item.Id > 0 && !item.IsModified && !item.IsDeleted) continue;
                if (item.Id <= 0 || item.IsModified)
                {
                    var itemModel = new ExpenseItem
                    {
                        BillId = model.Id,
                        CategoryId = item.CategoryId,
                        ProductId = item.ProductId,
                        Price = item.Cost,
                        Quantity = item.Quantity,
                        Comment = item.Comment
                    };
                    if (item.Id <= 0)
                        commands.Create(itemModel);
                    else
                    {
                        itemModel.Id = item.Id;
                        commands.Update(itemModel);
                    }
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
                SumPrice = model.Cost,
                ExpenseFlowId = model.ExpenseFlowId
            });

            await _unitOfWork.SaveChangesAsync();

            return model;
        }

        public async Task Create(ExpenseBillModel model)
        {
            model.Validate();
            
            var billCommands = _unitOfWork.GetCommandRepository<ExpenseBill>();

            var bill = new ExpenseBill
            {
                ExpenseFlowId = model.ExpenseFlowId,
                DateTime = model.DateTime,
                SumPrice = model.Cost
            };

            billCommands.Create(bill);

            var itemsCommands = _unitOfWork.GetCommandRepository<ExpenseItem>();

            foreach (var item in model.Items)
            {
                itemsCommands.Create(new ExpenseItem
                {
                    Bill = bill,
                    CategoryId = item.CategoryId,
                    ProductId = item.ProductId,
                    Price = item.Cost,
                    Quantity = item.Quantity,
                    Comment = item.Comment
                });
            }

            var flowQieries = _unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flowCommands = _unitOfWork.GetCommandRepository<ExpenseFlow>();
            var flow = await flowQieries.GetById(bill.ExpenseFlowId);
            flow.Balance -= bill.SumPrice;
            flowCommands.Update(flow);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
