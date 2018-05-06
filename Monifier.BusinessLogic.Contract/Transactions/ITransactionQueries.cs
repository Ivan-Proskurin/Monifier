using System.Collections.Generic;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Pagination;
using Monifier.BusinessLogic.Model.Transactions;

namespace Monifier.BusinessLogic.Contract.Transactions
{
    public interface ITransactionQueries : ICommonModelQueries<TransactionModel>
    {
        Task<List<TransactionModel>> GetInitiatorTransactions(int initiatorId);
        Task<TransactionModel> GetBillTransaction(int initiatorId, int billId);
        Task<TransactionModel> GetIncomeTransaction(int initiatorId, int incomeId);
        Task<TransactionModel> GetTransferTransaction(int initiatorId, int participantId);
        Task<List<TransactionViewModel>> GetLastTransactions(int initiatorId, int limit = 5);
        Task<TransactionPaginatedList> GetAllTransactions(TransactionFilter filter, PaginationArgs paginationArgs);
    }
}