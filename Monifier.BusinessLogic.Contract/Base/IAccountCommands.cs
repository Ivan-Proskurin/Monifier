using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Base;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.BusinessLogic.Contract.Base
{
    public interface IAccountCommands : ICommonModelCommands<AccountModel>
    {
        Task<IncomeItem> Topup(TopupAccountModel topup);
        Task Transfer(int accountFromId, int accountToId, decimal amount);
        Task TransferToExpenseFlow(int flowId, int fromAccountId, decimal amount);
    }
}