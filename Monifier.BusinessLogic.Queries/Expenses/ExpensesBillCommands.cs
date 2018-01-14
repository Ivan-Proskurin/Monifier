using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Expenses;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.DataAccess.Contract;

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

            await ExpenseBillModel.Delete(id, _unitOfWork);
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
