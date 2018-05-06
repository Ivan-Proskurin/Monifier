using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Incomes;

namespace Monifier.BusinessLogic.Contract.Base
{
    public interface IAccountCommands : ICommonModelCommands<AccountModel>
    {
        Task<IncomeItemModel> Topup(TopupAccountModel topup);
        Task<DateTime> Transfer(int accountFromId, int accountToId, decimal amount);
        Task TransferToExpenseFlow(int flowId, int fromAccountId, decimal amount);
    }
}