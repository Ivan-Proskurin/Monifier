using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Contract.Transactions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Accounts;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Transactions
{
    public class TransactionBuilder : ITransactionBuilder
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionBuilder(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void Create(ExpenseBill bill)
        {
            if (bill.AccountId == null) return;
            var transation = new Transaction
            {
                OwnerId = bill.OwnerId,
                DateTime = bill.DateTime,
                InitiatorId = bill.AccountId.Value,
                Bill = bill,
                Total = bill.SumPrice,
            };
            var transactionCommands = _unitOfWork.GetCommandRepository<Transaction>();
            transactionCommands.Create(transation);
        }

        public async Task Update(ExpenseBill bill, int? oldAccountId)
        {
            var transactionCommands = _unitOfWork.GetCommandRepository<Transaction>();
            var transactionQueries = _unitOfWork.GetQueryRepository<Transaction>();
            if (oldAccountId == bill.AccountId)
            {
                var transaction = await transactionQueries.Query
                    .SingleOrDefaultAsync(x => x.OwnerId == bill.OwnerId
                                               && x.InitiatorId == bill.AccountId && x.BillId == bill.Id);
                if (transaction != null)
                {
                    transaction.DateTime = bill.DateTime;
                    transaction.Total = bill.SumPrice;
                    transactionCommands.Update(transaction);
                }
            }
            else if (bill.AccountId != null)
            {
                var transaction = await transactionQueries.Query
                    .SingleOrDefaultAsync(x => x.OwnerId == bill.OwnerId
                                               && x.InitiatorId == oldAccountId && x.BillId == bill.Id);
                if (transaction == null)
                {
                    transaction = new Transaction
                    {
                        OwnerId = bill.OwnerId,
                        DateTime = bill.DateTime,
                        BillId = bill.Id,
                        InitiatorId = bill.AccountId.Value,
                        Total = bill.SumPrice
                    };
                    transactionCommands.Create(transaction);
                }
                else
                {
                    transaction.InitiatorId = bill.AccountId.Value;
                    transaction.Total = bill.SumPrice;
                    transactionCommands.Update(transaction);
                }
            }
        }
    }
}