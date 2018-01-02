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
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            await model.Update(_unitOfWork);
            return model;
        }

        public async Task Save(ExpenseBillModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            await model.Save(_unitOfWork);
        }
    }
}
